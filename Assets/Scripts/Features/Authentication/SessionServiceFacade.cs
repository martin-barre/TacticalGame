using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Services.Multiplayer;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

public class SessionServiceFacade
{
    public event Action OnCurrentSessionChanged;
    
    public ISession CurrentSession;

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
    
    public async Task CreateSessionAsHost(string sessionName, string password, int maxPlayers)
    {
        SessionOptions options = new SessionOptions
            {
                Name = sessionName,
                Password = string.IsNullOrWhiteSpace(password) ? null : password,
                MaxPlayers = maxPlayers,
                PlayerProperties = GetPlayerProperties()
            }
            .WithRelayNetwork();

        try
        {
            IHostSession session = await MultiplayerService.Instance.CreateSessionAsync(options);
            SetCurrentSession(session);
            
            Debug.Log($"Session created : {session.Id}, code = {session.Code}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error session creation : {ex}");
        }
    }

    public async Task JoinSessionById(string sessionId)
    {
        try
        {
            ISession session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, new JoinSessionOptions
            {
                PlayerProperties = GetPlayerProperties()
            });
            SetCurrentSession(session);
            Debug.Log($"Session joined : {session.Id}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error session joining : {ex}");
        }
    }

    public async Task QuitSession()
    {
        try
        {
            await CurrentSession.LeaveAsync();
            SetCurrentSession(null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error session leaving : {ex}");
        }
    }

    public async Task RemovePlayerAsync(string playerId)
    {
        if (CurrentSession.IsHost && CurrentSession.Players.Any(p => p.Id == playerId))
        {
            await CurrentSession.AsHost().RemovePlayerAsync(playerId);
        }
    }

    public void LaunchGame()
    {
        if (CurrentSession.IsHost && CurrentSession.PlayerCount >= CurrentSession.MaxPlayers)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
    }

    private void SetCurrentSession(ISession session)
    {
        if (CurrentSession == session) return;
        
        CurrentSession = session;
        OnCurrentSessionChanged?.Invoke();

        if (CurrentSession != null)
        {
            CurrentSession.Changed += () => Debug.Log("[CurrentSession] Changed");
            CurrentSession.StateChanged += (sessionState) => Debug.Log($"[CurrentSession] StateChanged : {sessionState}");
            CurrentSession.PlayerJoined += (playerId) => Debug.Log($"[CurrentSession] PlayerJoined : {playerId}");
            CurrentSession.PlayerLeaving += (playerId) => Debug.Log($"[CurrentSession] PlayerLeaving : {playerId}");
            CurrentSession.PlayerHasLeft += (playerId) => Debug.Log($"[CurrentSession] PlayerHasLeft : {playerId}");
            CurrentSession.SessionPropertiesChanged += () => Debug.Log("[CurrentSession] SessionPropertiesChanged");
            CurrentSession.PlayerPropertiesChanged += () => Debug.Log("[CurrentSession] PlayerPropertiesChanged");
            CurrentSession.RemovedFromSession += () =>
            {
                Debug.Log("[CurrentSession] RemovedFromSession");
                SetCurrentSession(null);
            };
            CurrentSession.Deleted += () => Debug.Log("[CurrentSession] Deleted");
            CurrentSession.SessionHostChanged += (playerId) => Debug.Log($"[CurrentSession] SessionHostChanged : {playerId}");
        }
    }

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
