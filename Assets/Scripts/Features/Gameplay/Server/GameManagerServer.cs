using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameManagerServer
{
    private readonly MapManager _mapManager;
    private readonly SessionManager<SessionPlayerData> _sessionManager;
    private readonly GameplayServerState _serverState;
    private readonly IPublisher<PacketsNetwork> _packetsNetworkPublisher;
    private readonly IPublisher<TeamNetwork> _teamNetworkPublisher;

    public GameState GameState => _serverState.GameState;
    public Map Map => _serverState.Map;

    public GameManagerServer(
        MapManager mapManager,
        SessionManager<SessionPlayerData> sessionManager,
        GameplayServerState serverState,
        IPublisher<PacketsNetwork> packetsNetworkPublisher,
        IPublisher<TeamNetwork> teamNetworkPublisher)
    {
        _mapManager = mapManager;
        _sessionManager = sessionManager;
        _serverState = serverState;
        _packetsNetworkPublisher = packetsNetworkPublisher;
        _teamNetworkPublisher = teamNetworkPublisher;

        _serverState.Map = _mapManager.Current;
        _serverState.GameState = new GameState
        {
            IsStarted = true,
            CurrentEntityIndex = 0
        };

        StartGame();
    }

    private void StartGame()
    {
        SpellDatabase.LoadAllItems();
        RaceDatabase.LoadAllItems();
        BuffDatabase.LoadAllItems();
        
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            ulong clientId = NetworkManager.Singleton.ConnectedClientsIds[i];
            SessionPlayerData sessionData = _sessionManager.GetPlayerData(clientId) ?? default;
            sessionData.Team = i % 2 == 0 ? Team.Red : Team.Blue;
            _sessionManager.SetPlayerData(clientId, sessionData);
        }
        
        List<SessionPlayerData> playersByTeam = NetworkManager.Singleton.ConnectedClientsIds
            .Select(clientId => _sessionManager.GetPlayerData(clientId))
            .Where(p => p.HasValue)
            .Select(p => p.Value)
            .ToList();
        
        List<int> blueRaces = playersByTeam.Where(p => p.Team == Team.Blue).SelectMany(p => p.RaceSelections).ToList();
        List<int> redRaces = playersByTeam.Where(p => p.Team == Team.Red).SelectMany(p => p.RaceSelections).ToList();
        
        int maxCount = Mathf.Max(blueRaces.Count, redRaces.Count);

        for (int i = 0; i < maxCount; i++)
        {
            if (i < blueRaces.Count)
            {
                Node node = Map.SpawnsBlue.FirstOrDefault(node => GameState.GetEntityByGridPosition(node.GridPosition) == null);
                ServerEffectContext.SpawnEntity(GameState, Map, Team.Blue, blueRaces[i], node.GridPosition, true);
            }

            if (i < redRaces.Count)
            {
                Node node = Map.SpawnsRed.FirstOrDefault(node => GameState.GetEntityByGridPosition(node.GridPosition) == null);
                ServerEffectContext.SpawnEntity(GameState, Map, Team.Red, redRaces[i], node.GridPosition, true);
            }
        }
    }
    
    public void SetupClientData(ulong clientId)
    {
        SessionPlayerData? sessionData = _sessionManager.GetPlayerData(clientId);
        if (sessionData == null) return;

        List<IPacket> effects = new();
        foreach (Entity entity in GameState.Entities)
        {
            PacketSummonEntity packetSummonEntity = new()
            {
                EntityId = entity.Id,
                Team = entity.Team,
                RaceId = entity.Race.Id,
                GridPosition = entity.GridPosition,
                IsPlayer = entity.IsPlayer,
                SummonerId = entity.Summoner?.Id ?? -1
            };
            effects.Add(packetSummonEntity);
        }

        PacketSetGameLogic packetSetGameLogic = new()
        {
            IsStarted = GameState.IsStarted,
            CurrentPlayer = GameState.CurrentEntityIndex
        };
        effects.Add(packetSetGameLogic);

        _packetsNetworkPublisher.Publish(new PacketsNetwork
        {
            Packets = effects.ToArray(),
            TargetClientId = clientId
        });
        _teamNetworkPublisher.Publish(new TeamNetwork
        {
            Team = sessionData.Value.Team,
            TargetClientId = clientId
        });
    }

    public void AdvanceAiTurns()
    {
        _ = PlayAI();
    }

    private Task PlayAI()
    {
        while(!GameState.CurrentEntity.IsPlayer)
        {
            List<IPacket> clientEffects = AiManager.PlayOnAction(GameState.CurrentEntity, GameState, Map, AiBehaviorType.Aggressive);
            _packetsNetworkPublisher.Publish(new PacketsNetwork
            {
                Packets = clientEffects.ToArray()
            });
        }

        return Task.CompletedTask;
    }
}
