using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Services.Multiplayer;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

public class SessionServiceFacade : ISessionServiceFacade
{
    public Bindable<ISession> CurrentSession { get; } = new();

    public async Task<IList<ISessionInfo>> GetAllSessions()
    {
        try
        {
            QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());
            return results.Sessions;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error session list fetching : {ex}");
        }
        return new List<ISessionInfo>();
    }
    
    public async Task<ISession> TryCreateSessionAsync(string sessionName, string password, int maxPlayers)
    {
        SessionOptions options = new SessionOptions
        {
            Name = sessionName,
            Password = string.IsNullOrWhiteSpace(password) ? null : password,
            MaxPlayers = maxPlayers,
            PlayerProperties = GetPlayerProperties()
        }.WithRelayNetwork();

        try
        {
            IHostSession session = await MultiplayerService.Instance.CreateSessionAsync(options);
            SetCurrentSession(session);
            
            Debug.Log($"Session created : {session.Id}, code = {session.Code}");

            return session;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error session creation : {ex}");
        }

        return null;
    }

    public async Task<ISession> TryJoinSessionByIdAsync(string sessionId)
    {
        try
        {
            ISession session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, new JoinSessionOptions
            {
                PlayerProperties = GetPlayerProperties()
            });
            SetCurrentSession(session);
            Debug.Log($"Session joined : {session.Id}");
            return session;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error session joining : {ex}");
        }

        return null;
    }

    public async Task LeaveSessionAsync()
    {
        try
        {
            await CurrentSession.Value.LeaveAsync();
            SetCurrentSession(null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error session leaving : {ex}");
        }
    }

    public async Task RemovePlayerAsync(string playerId)
    {
        if (CurrentSession.Value.IsHost && CurrentSession.Value.Players.Any(p => p.Id == playerId))
        {
            await CurrentSession.Value.AsHost().RemovePlayerAsync(playerId);
        }
    }

    public void LaunchGame()
    {
        if (CurrentSession.Value.IsHost && CurrentSession.Value.PlayerCount >= CurrentSession.Value.MaxPlayers)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
    }

    private void SetCurrentSession(ISession session)
    {
        if (CurrentSession.Value == session) return;

        UnsubscribeFromJoinedSession();
        
        CurrentSession.Value = session;
        
        SubscribeToJoinedSession();
    }
    
    private void SubscribeToJoinedSession()
    {
        CurrentSession.Value.Changed += OnSessionChanged;
        CurrentSession.Value.StateChanged += OnSessionStateChanged;
        CurrentSession.Value.Deleted += OnSessionDeleted;
        CurrentSession.Value.PlayerJoined += OnPlayerJoined;
        CurrentSession.Value.PlayerHasLeft += OnPlayerHasLeft;
        CurrentSession.Value.RemovedFromSession += OnRemovedFromSession;
        CurrentSession.Value.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
        CurrentSession.Value.SessionPropertiesChanged += OnSessionPropertiesChanged;
        CurrentSession.Value.SessionHostChanged += OnSessionHostChanged;
    }

    private void UnsubscribeFromJoinedSession()
    {
        if (CurrentSession.Value == null) return;
        CurrentSession.Value.Changed -= OnSessionChanged;
        CurrentSession.Value.StateChanged -= OnSessionStateChanged;
        CurrentSession.Value.Deleted -= OnSessionDeleted;
        CurrentSession.Value.PlayerJoined -= OnPlayerJoined;
        CurrentSession.Value.PlayerHasLeft -= OnPlayerHasLeft;
        CurrentSession.Value.RemovedFromSession -= OnRemovedFromSession;
        CurrentSession.Value.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;
        CurrentSession.Value.SessionPropertiesChanged -= OnSessionPropertiesChanged;
        CurrentSession.Value.SessionHostChanged -= OnSessionHostChanged;
    }
    
    private void OnSessionChanged() => Debug.Log("[CurrentSession] Changed");
    private void OnSessionStateChanged(SessionState sessionState) => Debug.Log($"[CurrentSession] StateChanged : {sessionState}");
    private void OnSessionDeleted() => Debug.Log("[CurrentSession] Deleted");
    private void OnPlayerJoined(string playerId) => Debug.Log($"[CurrentSession] PlayerJoined : {playerId}");
    private void OnPlayerHasLeft(string playerId) => Debug.Log($"[CurrentSession] PlayerHasLeft : {playerId}");
    private void OnSessionPropertiesChanged() => Debug.Log("[CurrentSession] SessionPropertiesChanged");
    private void OnPlayerPropertiesChanged() => Debug.Log("[CurrentSession] PlayerPropertiesChanged");
    private void OnRemovedFromSession()
    {
        Debug.Log("[CurrentSession] RemovedFromSession");
        SetCurrentSession(null);
    }
    private void OnSessionHostChanged(string playerId) => Debug.Log($"[CurrentSession] SessionHostChanged : {playerId}");

    private Dictionary<string, PlayerProperty> GetPlayerProperties()
    {
        return new Dictionary<string, PlayerProperty>
        {
            {
                "playerName", new PlayerProperty(AuthenticationService.Instance.PlayerName, VisibilityPropertyOptions.Member)
            }
        };
    }
}
