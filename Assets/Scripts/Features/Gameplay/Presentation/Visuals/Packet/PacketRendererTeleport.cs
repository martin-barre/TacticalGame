using System;
using System.Threading.Tasks;
using MessagePack;
using UnityEngine;

public sealed class PacketRendererTeleport : IPacketRenderer<PacketTeleport>
{
    public Task RenderAsync(PacketTeleport packet)
    {
        Entity entity = GameManagerClient.Instance.GameState.GetEntityById(packet.TargetId);
        if(entity == null) throw new Exception($"Entity with id {packet.TargetId} not found.");

        EntityPrefabController entityPrefab = GameManagerClient.Instance.GetEntityPrefab(packet.TargetId);
        if(entityPrefab == null) throw new Exception($"EntityPrefab with id {packet.TargetId} not found.");
        
        Entity entity2 = GameManagerClient.Instance.GameState.GetEntityByGridPosition(packet.GridPosition);
        EntityPrefabController entityPrefab2 = entity2 != null ? GameManagerClient.Instance.GetEntityPrefab(entity2.Id) : null;
        Vector2Int oldPosition = entity.GridPosition;
        
        GameManagerClient.Instance.GameState.MoveOrSwapEntity(entity, packet.GridPosition);
        
        ViewModelFactory.Entity.NotifyUpdate(entity);
        ViewModelFactory.Entity.NotifyUpdate(entity2);
        
        entityPrefab.transform.position = MapManager.Instance.GridPositionToWorlPosition(packet.GridPosition);
        if (entityPrefab2 != null)
        {
            entityPrefab2.transform.position = MapManager.Instance.GridPositionToWorlPosition(oldPosition);
        }
        
        return Task.CompletedTask;
    }
}