using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawnerPrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool isGameInProgress = false;
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();


    #region Server
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (!isGameInProgress) { return; }
        conn.Disconnect();
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Remove(player);
        base.OnServerDisconnect(conn);
    }
    public override void OnStopServer()
    {
        Players.Clear();
        isGameInProgress = false;
    }

    public void StartGame()
    {
        if(Players.Count < 2) { return; }
        isGameInProgress = true;
        ServerChangeScene("Scene_Map1");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        Players.Add(player);
        player.SetTeamColor(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));

        player.SetDisplayName($"Player {Players.Count}");

        player.SetIsPartyOwner(Players.Count == 1);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            GameOverHandler gameOverHandler = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandler.gameObject);

            foreach(RTSPlayer player in Players)
            {
                GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(unitSpawnerInstance, player.connectionToClient);
            }
        }
    }
    #endregion

    #region Client
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }
    #endregion
}
