using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

    public List<Unit> GetMyUnits() { return myUnits; }

    #region Server
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawn;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawn;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawn;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawn;
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
    #endregion

    #region Client
    public override void OnStartClient()
    {
        if(!isClientOnly) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawn;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawn;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawn;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawn;
    }

    private void AuthorityHandleUnitSpawn(Unit unit)
    {
        if(!isOwned) { return; }

        myUnits.Add(unit);
    }
    private void AuthorityHandleUnitDespawn(Unit unit)
    {
        if (!isOwned) { return; }

        myUnits.Remove(unit);
    }
    #endregion
}