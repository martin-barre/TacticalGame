using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStartedCallback;
        NetworkManager.Singleton.OnServerStopped += OnServerStoppedCallback;
        NetworkManager.Singleton.OnClientStarted += OnClientStartedCallback;
        NetworkManager.Singleton.OnClientStopped += OnClientStoppedCallback;
        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEventCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheckCallback;
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnServerStarted -= OnServerStartedCallback;
        NetworkManager.Singleton.OnServerStopped -= OnServerStoppedCallback;
        NetworkManager.Singleton.OnClientStarted -= OnClientStartedCallback;
        NetworkManager.Singleton.OnClientStopped -= OnClientStoppedCallback;
        NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEventCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheckCallback;
    }

    private static void OnServerStartedCallback() => Debug.Log("[NetworkSetup] OnServerStartedCallback -> Server started.");
    private static void OnServerStoppedCallback(bool isHost) => Debug.Log("[NetworkSetup] OnServerStoppedCallback -> Server stopped.");
    private static void OnClientStartedCallback()
    {
        Debug.Log("[NetworkSetup] OnClientStartedCallback -> Client started.");
        NetworkManager.Singleton.SceneManager.LoadScene("RaceSelection", LoadSceneMode.Single);
    }

    private static void OnClientStoppedCallback(bool isHost) => Debug.Log("[NetworkSetup] OnClientStoppedCallback -> Client stopped.");
    
    private static void OnConnectionEventCallback(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        string message = connectionEventData.EventType switch
        {
            ConnectionEvent.ClientConnected => "Client connected",
            ConnectionEvent.ClientDisconnected => "Client disconnected",
            ConnectionEvent.PeerConnected => "Peer connected",
            ConnectionEvent.PeerDisconnected => "Peer connected",
            _ => throw new ArgumentOutOfRangeException()
        };
        Debug.Log($"[NetworkSetup] OnConnectionEventCallback -> {message} with id {connectionEventData.ClientId}.");
    }
    
    private static void ApprovalCheckCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        byte[] payload = request.Payload;
        string playerId = Encoding.UTF8.GetString(payload);
        
        Debug.Log($"[NetworkSetup] ConnectionApproval -> Receive connection request with PlayerID : {playerId}.");
        
        if (SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(playerId))
        {
            response.Approved = false;
            response.Reason = "This player is already connected.";
            return;
        }

        SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(request.ClientNetworkId, playerId, new SessionPlayerData
        {
            ClientID = request.ClientNetworkId,
            IsConnected = true
        });
        
        response.Approved = true;
    }
}
