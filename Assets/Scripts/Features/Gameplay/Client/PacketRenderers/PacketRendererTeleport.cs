using System;
using System.Threading.Tasks;
using VContainer;
using UnityEngine;

public sealed class PacketRendererTeleport : IPacketRenderer<PacketTeleport>
{
    [Inject] private MapManager _mapManager;
    [Inject] private GameplayClientState _clientState;
    
    public Task RenderAsync(PacketTeleport packet)
    {
        Entity entity = _clientState.GameState.GetEntityById(packet.TargetId);
        if(entity == null) throw new Exception($"Entity with id {packet.TargetId} not found.");

        EntityPrefabController entityPrefab = _clientState.GetEntityPrefab(packet.TargetId);
        if(entityPrefab == null) throw new Exception($"EntityPrefab with id {packet.TargetId} not found.");
        
        Entity entity2 = _clientState.GameState.GetEntityByGridPosition(packet.GridPosition);
        EntityPrefabController entityPrefab2 = entity2 != null ? _clientState.GetEntityPrefab(entity2.Id) : null;
        Vector2Int oldPosition = entity.GridPosition;
        
        _clientState.GameState.MoveOrSwapEntity(entity, packet.GridPosition);
        
        ViewModelFactory.Entity.NotifyUpdate(entity);
        ViewModelFactory.Entity.NotifyUpdate(entity2);
        
        entityPrefab.transform.position = _mapManager.GridToWorld(packet.GridPosition);
        if (entityPrefab2 != null)
        {
            entityPrefab2.transform.position = _mapManager.GridToWorld(oldPosition);
        }
        
        return Task.CompletedTask;
    }
}
