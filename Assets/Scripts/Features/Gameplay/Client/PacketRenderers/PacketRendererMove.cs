using System;
using System.Linq;
using System.Threading.Tasks;
using VContainer;
using UnityEngine;

public sealed class PacketRendererMove : IPacketRenderer<PacketMove>
{
    [Inject] private MapManager _mapManager;
    [Inject] private GameplayClientState _clientState;
    
    public async Task RenderAsync(PacketMove packet)
    {
        Entity entity = _clientState.GameState.GetEntityById(packet.TargetId);
        if(entity == null) throw new Exception($"Entity with id {packet.TargetId} not found.");

        EntityPrefabController entityPrefab = _clientState.GetEntityPrefab(packet.TargetId);
        if(entityPrefab == null) throw new Exception($"EntityPrefab with id {packet.TargetId} not found.");

        ViewModelFactory.Entity.NotifyUpdate(entity);
        
        entityPrefab.GetComponentInChildren<Animator>()?.SetBool("Move", true);
        PathMover pathMover = new(packet.Path.Select(pos => _mapManager.GridToWorld(pos)).ToList(), 2.5f);
        await pathMover.Move(entityPrefab);
        entityPrefab.GetComponentInChildren<Animator>()?.SetBool("Move", false);
        
        if (packet.PmCost > 0)
        {
            InteractionManager.ShowInfo($"{packet.PmCost}", entityPrefab.transform.position + Vector3.up * 1f, Color.green);
        }
    }
}
