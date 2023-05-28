using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;

    private List<UnitBase> basesSpawned = new List<UnitBase>();

    #region Server
    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase) {
        basesSpawned.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase) { 
        basesSpawned.Remove(unitBase);

        if(basesSpawned.Count != 1) { return; }

        int playerID = basesSpawned[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerID}");

        ServerOnGameOver?.Invoke();
    }
    #endregion

    #region Client
    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }
    #endregion
}
