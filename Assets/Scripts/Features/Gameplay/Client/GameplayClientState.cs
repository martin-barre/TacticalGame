using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayClientState
{
    private readonly MapManager _mapManager;
    private readonly InjectedPrefabFactory _prefabFactory;

    private readonly Dictionary<int, EntityPrefabController> _entitiesPrefabs = new();

    public GameplayClientState(
        MapManager mapManager,
        InjectedPrefabFactory prefabFactory,
        PacketRendererRegistry packetRendererRegistry)
    {
        _mapManager = mapManager;
        _prefabFactory = prefabFactory;
        PacketRendererRegistry = packetRendererRegistry;
    }

    public event Action<string> OnChatMessage;
    public event Action Initialized;
    public event Action TeamAssigned;

    public GameState GameState { get; private set; }
    public Map Map { get; private set; }

    private Team? _team;
    public Team? Team
    {
        get => _team;
        set
        {
            _team = value;
            TeamAssigned?.Invoke();
        }
    }

    public PacketRendererRegistry PacketRendererRegistry { get; }
    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        Map = _mapManager.Current;
        GameState = new GameState
        {
            IsStarted = false,
            CurrentEntityIndex = -1
        };
        IsInitialized = true;
        Initialized?.Invoke();
    }

    public void SpawnEntity(int entityId, int raceId, Vector2Int gridPosition)
    {
        Race race = RaceDatabase.GetById(raceId);
        ViewModelFactory.Game.NotifyUpdate(GameState);
        EntityPrefabController prefab = _prefabFactory.Instantiate(race.Prefab, _mapManager.GridToWorld(gridPosition), Quaternion.identity);
        _entitiesPrefabs.Add(entityId, prefab);
    }

    public void KillEntity(int entityId)
    {
        Entity entity = GameState.GetEntityById(entityId);
        if (entity == null) throw new Exception($"Entity with id {entityId} not found.");

        EntityPrefabController entityPrefab = GetEntityPrefab(entityId);
        if (entityPrefab == null) throw new Exception($"EntityPrefab with id {entityId} not found.");

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

        UnityEngine.Object.Destroy(entityPrefab.gameObject);
    }

    public EntityPrefabController GetEntityPrefab(int entityId)
    {
        return _entitiesPrefabs.GetValueOrDefault(entityId);
    }

    public GameObject InstantiateObject(GameObject original, Vector3 position, Quaternion rotation)
    {
        return _prefabFactory.Instantiate(original, position, rotation);
    }

    public void SendChatMessage(string message)
    {
        OnChatMessage?.Invoke(message);
    }
}
