using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

[Serializable]
public class SFX
{
    public TargetEnum target;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "NewSpell", menuName = "ScriptableObjects/Spell")]
public class Spell : ScriptableObject
{
    [Header("GLOBAL")]
    public int Id;
    public Sprite iconSprite;
    public string spellName;
    public int paCost;
    
    [Header("FOV")]
    public FovMode fovMode;
    public bool xRay;
    public bool canLaunchOnEntity;
    public int poMin;
    public int poMax;
    
    [Header("ZONE")]
    [SerializeReference] public SpellZone zone;
    
    [Header("EFFECTS")]
    public List<SFX> sfx;
    [SerializeReference] public List<ServerEffectBase> effects;

    public List<Node> GetZoneNodes(Vector2Int launcherPosition, Vector2Int targetPosition, Map map)
    {
        return zone.GetZonePositions(launcherPosition, targetPosition)
            .AsValueEnumerable()
            .Select(position => map.GetNode(targetPosition + position))
            .Where(node => node.NodeType == NodeType.Ground)
            .ToList();
    }
    
    public List<Vector2Int> GetZonePositions(Vector2Int launcherPosition, Vector2Int targetPosition, Map map)
    {
        return zone.GetZonePositions(launcherPosition, targetPosition)
            .AsValueEnumerable()
            .Where(position => map.GetNode(targetPosition + position).NodeType == NodeType.Ground)
            .ToList();
    }

    public List<Entity> GetTouchedEntities(Vector2Int launcherPosition, Vector2Int targetPosition, GameState gameState, Map map)
    {
        return zone.GetZonePositions(launcherPosition, targetPosition)
            .AsValueEnumerable()
            .Select(position => gameState.GetEntityByGridPosition(targetPosition + position))
            .Where(e => e != null)
            .ToList();
    }

    public List<IPacket> Launch(Entity launcher, Vector2Int targetGridPosition, GameState gameState, Map map)
    {
        List<Entity> entities = GetTouchedEntities(launcher.GridPosition, targetGridPosition, gameState, map);
        List<IPacket> clientEffects = new();
        foreach (ServerEffectBase effect in effects)
        {
            clientEffects.AddRange(effect.Apply(launcher, entities, targetGridPosition, gameState, map));
        }
        return clientEffects;
    }
}
