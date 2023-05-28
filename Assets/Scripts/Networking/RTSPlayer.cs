using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();
    [SerializeField] private List<Building> myBuildings = new List<Building>();
    public List<Unit> GetMyUnits() { return myUnits; }
    public List<Building> GetMyBuildings() { return myBuildings; }

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
