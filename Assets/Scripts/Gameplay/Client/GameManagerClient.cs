using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManagerClient : MonoSingleton<GameManagerClient>
{
    public event Action<string> OnChatMessage;
    public GameState GameState;
    public Map Map;
    public Team Team;
    
    public readonly PacketRendererRegistry PacketRendererRegistry = new();
    
    private readonly Dictionary<int, EntityPrefabController> _entitiesPrefabs = new();
    
    private void Awake()
    {
        PacketRendererRegistry.Register(new PacketRendererBuff());
        PacketRendererRegistry.Register(new PacketRendererDamage());
        PacketRendererRegistry.Register(new PacketRendererHeal());
        PacketRendererRegistry.Register(new PacketRendererKillEntity());
        PacketRendererRegistry.Register(new PacketRendererLaunchSpell());
        PacketRendererRegistry.Register(new PacketRendererMove());
        PacketRendererRegistry.Register(new PacketRendererNextTurn());
        PacketRendererRegistry.Register(new PacketRendererSetGameLogic());
        PacketRendererRegistry.Register(new PacketRendererSummonEntity());
        PacketRendererRegistry.Register(new PacketRendererTeleport());
            
        MapManager.Instance.InitializeMap();
        Map = MapManager.Instance.GetMap();
        GameState = new GameState
        {
            IsStarted = false,
            CurrentEntityIndex = -1
        };
    }
    
    private IEnumerator Start()
    {
        // Attendre une frame pour laisser le temps à tous les Start() de s'exécuter
        yield return null;
        ActionRequestSender.Instance.NotifyClientReadyServerRpc();
    }

    public void SpawnEntity(int entityId, int raceId, Vector2Int gridPosition)
    {
        Race race = RaceDatabase.GetById(raceId);
        ViewModelFactory.Game.NotifyUpdate(GameState);
        EntityPrefabController prefab = Instantiate(race.Prefab, MapManager.Instance.GridPositionToWorlPosition(gridPosition), Quaternion.identity);
        _entitiesPrefabs.Add(entityId, prefab);
    }
    
    public void KillEntity(int entityId)
    {
        Entity entity = GameState.GetEntityById(entityId);
        if(entity == null) throw new Exception($"Entity with id {entityId} not found.");
        
        EntityPrefabController entityPrefab = GetEntityPrefab(entityId);
        if(entityPrefab == null) throw new Exception($"EntityPrefab with id {entityId} not found.");
        
        if (GameState.CurrentEntityIndex >= GameState.Entities.IndexOf(entity))
        {
            GameState.CurrentEntityIndex--;
        }
        
        GameState.Entities.ForEach(e =>
        {
            e.Buffs.RemoveAll(b => b.Launcher.Id == entity.Id);
            ViewModelFactory.Entity.NotifyUpdate(e);
        });
        
        GameState.Entities.Remove(entity);
        
        ViewModelFactory.Game.NotifyUpdate(GameState);
        
        Destroy(entityPrefab.gameObject);
    }

    public EntityPrefabController GetEntityPrefab(int entityId)
    {
        return _entitiesPrefabs[entityId];
    }

    public GameObject InstantiateObject(GameObject original, Vector3 position, Quaternion rotation)
    {
        return Instantiate(original, position, rotation);
    }

    public void SendChatMessage(string message)
    {
        OnChatMessage?.Invoke(message);
    }
}
