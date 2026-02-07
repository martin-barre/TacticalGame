using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class PacketRendererMove : IPacketRenderer<PacketMove>
{
    public async Task RenderAsync(PacketMove packet)
    {
        Entity entity = GameManagerClient.Instance.GameState.GetEntityById(packet.TargetId);
        if(entity == null) throw new Exception($"Entity with id {packet.TargetId} not found.");

        EntityPrefabController entityPrefab = GameManagerClient.Instance.GetEntityPrefab(packet.TargetId);
        if(entityPrefab == null) throw new Exception($"EntityPrefab with id {packet.TargetId} not found.");

        ViewModelFactory.Entity.NotifyUpdate(entity);
        
        entityPrefab.GetComponentInChildren<Animator>()?.SetBool("Move", true);
        PathMover pathMover = new(packet.Path.Select(pos => MapManager.Instance.GridPositionToWorlPosition(pos)).ToList(), 2.5f);
        await pathMover.Move(entityPrefab);
        InteractionManager.ShowInfo($"{packet.PmCost}", entityPrefab.transform.position + Vector3.up * 1f, Color.green);
        entityPrefab.GetComponentInChildren<Animator>()?.SetBool("Move", false);
    }
}