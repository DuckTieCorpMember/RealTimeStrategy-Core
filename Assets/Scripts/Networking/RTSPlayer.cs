using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5f;

    [SyncVar(hook = nameof(ClientHandleResourceUpdated))]
    private int resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    public event Action<int> ClientOnResourcesUpdated;

    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();
    public List<Unit> GetMyUnits() { return myUnits; }
    public List<Building> GetMyBuildings() { return myBuildings; }
    public int GetResources() { return resources; }
    public Color GetTeamColor() { return teamColor; }
    public Transform GetCameraTransform() { return cameraTransform; }
    public bool GetIsPartyOwner() { return isPartyOwner;}
    public string GetDisplayName() { return displayName; } 

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
        {
            return false;
        }

        foreach (Building building in myBuildings)
        {
            if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }
        return false;
    }

    #region Server
    [Server]
    public void SetTeamColor(Color newteamColor)
    {
        teamColor = newteamColor;
    }
    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }
    [Server]
    public void SetIsPartyOwner(bool i_isPartyOwner)
    {
        isPartyOwner = i_isPartyOwner;
    }
    [Server]
    public void SetDisplayName(string name)
    {
        displayName = name;
    }
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawn;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawn;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawn;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawn;

        DontDestroyOnLoad(gameObject);
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

        if(resources < buildingToPlace.GetPrice()) { return; }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
        if(!CanPlaceBuilding(buildingCollider, point)) { return; }

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        SetResources(resources - buildingToPlace.GetPrice());
    }

    [Command]
    public void CmdStartGame()
    {
        if(!isPartyOwner) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
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

    public override void OnStartClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if (NetworkServer.active) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);

        DontDestroyOnLoad(gameObject);
    }

    private void ClientHandleResourceUpdated(int oldResourceValue, int newResourceValue)
    {
        ClientOnResourcesUpdated?.Invoke(newResourceValue);
    }

    private void ClientHandleDisplayNameUpdated(string oldName, string newName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!isOwned) { return; }
        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
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
        if (!isClientOnly) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if(!isOwned) { return; }

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
