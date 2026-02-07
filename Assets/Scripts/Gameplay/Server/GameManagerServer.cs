using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Unity.Netcode;
using UnityEngine;

public class GameManagerServer : MonoSingleton<GameManagerServer>
{
    public GameState GameState;
    public Map Map;

    private void Awake()
    {
        MapManager.Instance.InitializeMap();
        Map = MapManager.Instance.GetMap();
        GameState = new GameState
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
            SessionPlayerData sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId) ?? default;
            sessionData.Team = i % 2 == 0 ? Team.Red : Team.Blue;
            SessionManager<SessionPlayerData>.Instance.SetPlayerData(clientId, sessionData);
        }
        
        List<SessionPlayerData> playersByTeam = NetworkManager.Singleton.ConnectedClientsIds
            .Select(clientId => SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId))
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
                SpawnEntity(Team.Blue, blueRaces[i], node.GridPosition, true, GameState);
            }

            if (i < redRaces.Count)
            {
                Node node = Map.SpawnsRed.FirstOrDefault(node => GameState.GetEntityByGridPosition(node.GridPosition) == null);
                SpawnEntity(Team.Red, redRaces[i], node.GridPosition, false, GameState);
            }
        }
    }
    
    public void SetupClientData(ulong clientId)
    {
        SessionPlayerData? sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
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
        
        ActionResultSender.Instance.SendPacketsClientRpc(MessagePackSerializer.Serialize(effects), new ClientRpcParams 
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });
        ActionResultSender.Instance.SendTeamClientRpc(sessionData.Value.Team, new ClientRpcParams 
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });
    }

    public PacketSummonEntity? SpawnEntity(Team team, int raceId, Vector2Int gridPosition, bool isPlayer, GameState gameState, Entity summoner = null)
    {
        Node node = Map.GetNode(gridPosition);

        if (node.NodeType != NodeType.Ground || gameState.GetEntityByGridPosition(node.GridPosition) != null) return null;

        PacketSummonEntity packetSummonEntity = new()
        {
            EntityId = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0),
            Team = team,
            RaceId = raceId,
            GridPosition = gridPosition,
            IsPlayer = isPlayer,
            SummonerId = summoner?.Id ?? -1
        };
        packetSummonEntity.Apply(GameState, Map);

        return packetSummonEntity;
    }
    
    public IList<IPacket> KillEntity(Entity entity, GameState gameState)
    {
        List<IPacket> packets = new();

        PacketKillEntity packetKillEntity = new() { TargetId = entity.Id };
        packetKillEntity.Apply(GameState, Map);
        packets.Add(packetKillEntity);

        if (entity == gameState.CurrentEntity)
        {
            packets.AddRange(GameServerAction.NextTurn(GameState, Map));
        }

        return packets;
    }

    public void TestPlayIA()
    {
        StartCoroutine(PlayAI());
    }

    private IEnumerator PlayAI()
    {
        while(!GameState.CurrentEntity.IsPlayer)
        {
            List<IPacket> clientEffects = AiManagerV2.PlayOnAction(GameState.CurrentEntity, GameState, Map, AiManagerV2.AiBehaviorType.Aggressive);
            ActionResultSender.Instance.SendPacketsClientRpc(MessagePackSerializer.Serialize(clientEffects));
            yield return null;
        }
    }
}
