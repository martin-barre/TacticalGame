using System.Collections.Generic;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;

public interface ISessionServiceFacade
{
    Bindable<ISession> CurrentSession { get; }
    
    Task<IList<ISessionInfo>> GetAllSessions();
    Task<ISession> TryCreateSessionAsync(string sessionName, string password, int maxPlayers);
    Task<ISession> TryJoinSessionByIdAsync(string sessionId);
    Task LeaveSessionAsync();
    Task RemovePlayerAsync(string playerId);
    void LaunchGame();
}
