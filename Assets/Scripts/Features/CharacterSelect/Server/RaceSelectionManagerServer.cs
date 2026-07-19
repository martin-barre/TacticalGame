using System.Collections.Generic;
using VContainer;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class RaceSelectionManagerServer : NetworkSingleton<RaceSelectionManagerServer>
{
    private const int MaxCharacters = 3;
    
    private readonly Dictionary<ulong, RaceSelectionState> _playerSelections = new();
    [Inject] private SessionManager<SessionPlayerData> _sessionManager;
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        UpdateClients();
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        _playerSelections.Remove(clientId);
        UpdateClients();
    }

    // --- RPCs ---

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestAddCharacterRpc(int characterId, RpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        if (!_playerSelections.TryGetValue(sender, out RaceSelectionState state))
        {
            state = new RaceSelectionState
            {
                ClientId = sender,
                CharacterIds = new List<int>()
            };
        }

        if (state.CharacterIds.Count < MaxCharacters && !state.CharacterIds.Contains(characterId))
        {
            state.CharacterIds.Add(characterId);
            _playerSelections[sender] = state;
            UpdateClients();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestRemoveCharacterRpc(int characterId, RpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        if (!_playerSelections.TryGetValue(sender, out RaceSelectionState state))
            return;

        for (int i = 0; i < state.CharacterIds.Count; i++)
        {
            if (state.CharacterIds[i] == characterId)
            {
                state.CharacterIds.RemoveAtSwapBack(i);
                break;
            }
        }

        _playerSelections[sender] = state;
        UpdateClients();
    }

    // --- Broadcast vers clients ---
    private void UpdateClients()
    {
        List<RaceSelectionState> allSelections = new(_playerSelections.Values);
        UpdateSelectionsClientRpc(allSelections.ToArray());

        if (TestIfReady())
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SessionPlayerData? playerData = _sessionManager.GetPlayerData(clientId);
                if (playerData != null)
                {
                    _sessionManager.SetPlayerData(clientId, new SessionPlayerData
                    {
                        ClientID = playerData.Value.ClientID,
                        IsConnected = playerData.Value.IsConnected,
                        Team = playerData.Value.Team,
                        RaceSelections = _playerSelections[clientId].CharacterIds
                    });
                }
            }
            _sessionManager.OnSessionStarted();
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
    }

    private bool TestIfReady()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count <= 1) return false;
        
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerSelections.TryGetValue(clientId, out RaceSelectionState state)) return false;
            if (state.CharacterIds.Count != MaxCharacters) return false;
        }
        return true;
    }

    [ClientRpc]
    private void UpdateSelectionsClientRpc(RaceSelectionState[] selections)
    {
        RaceSelectionManagerClient.Instance?.UpdateSelectionUI(selections);
    }
}
