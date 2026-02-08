using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneReadyTracker : NetworkBehaviour
{
    public static SceneReadyTracker Instance { get; private set; }

    private readonly HashSet<ulong> _readyClients = new();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _readyClients.Clear();
            Debug.Log("[SceneReadyTracker] Serveur initialisé");
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NotifyClientReadyRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (_readyClients.Add(clientId))
        {
            Debug.Log($"[SceneReadyTracker] Client {clientId} est prêt ({_readyClients.Count}/{NetworkManager.Singleton.ConnectedClientsIds.Count})");
            GameManagerServer.Instance.SetupClientData(clientId);
            
            if (AllClientsReady())
            {
                Debug.Log("[SceneReadyTracker] ✅ Tous les clients sont prêts !");
                OnAllClientsReady();
            }
        }
    }

    private bool AllClientsReady()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_readyClients.Contains(clientId))
                return false;
        }
        return true;
    }

    private void OnAllClientsReady()
    {
        // Exemple : démarrer la partie
        // GameManagerServer.Instance.StartGame();
    }
}