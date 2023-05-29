using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Building[] buildings = new Building[0];

    [SyncVar(hook = nameof(ClientHandleResourceUpdated))]
    private int resources = 500;

    public event Action<int> ClientOnResourcesUpdated;

    [SerializeField] private List<Unit> myUnits = new List<Unit>();
    [SerializeField] private List<Building> myBuildings = new List<Building>();
    public List<Unit> GetMyUnits() { return myUnits; }
    public List<Building> GetMyBuildings() { return myBuildings; }
    public int GetResources() { return resources; }
    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawn;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawn;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawn;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawn;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawn;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawn;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawn;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawn;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;
        foreach(Building building in buildings)
        {
            if(building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        } 

        if(buildingToPlace == null) { return; }

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);
    }

    private void ServerHandleUnitSpawn(Unit unit)
    {
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);
    }
    private void ServerHandleUnitDespawn(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);   
    }

    private void ServerHandleBuildingSpawn(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);
    }
    private void ServerHandleBuildingDespawn(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
    }
    #endregion

    #region Client

    private void ClientHandleResourceUpdated(int oldResourceValue, int newResourceValue)
    {
        ClientOnResourcesUpdated?.Invoke(newResourceValue);
    }

    public override void OnStartAuthority()
    {
        if(NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawn;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawn;

        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawn;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawn;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !isOwned) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawn;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawn;

        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawn;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawn;
    }

    private void AuthorityHandleUnitSpawn(Unit unit)
    {
        myUnits.Add(unit);
    }
    private void AuthorityHandleUnitDespawn(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawn(Building building)
    {
        myBuildings.Add(building);
    }
    private void AuthorityHandleBuildingDespawn(Building building)
    {
        myBuildings.Remove(building);
    }
    #endregion
}
