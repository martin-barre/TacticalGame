using System;
using System.Threading.Tasks;
using MessagePack;
using UnityEngine;

public sealed class PacketRendererDamage : IPacketRenderer<PacketDamage>
{
    public Task RenderAsync(PacketDamage packet)
    {
        Entity entity = GameManagerClient.Instance.GameState.GetEntityById(packet.TargetId);
        if(entity == null) throw new Exception($"Entity with id {packet.TargetId} not found.");
        
        EntityPrefabController entityPrefab = GameManagerClient.Instance.GetEntityPrefab(packet.TargetId);
        if(entityPrefab == null) throw new Exception($"EntityPrefab with id {packet.TargetId} not found.");
        
        ViewModelFactory.Entity.NotifyUpdate(entity);
        
        GameManagerClient.Instance.SendChatMessage($"<color=#FF0000>{entity.Race.Name}</color> perd <color=#00FF00>{packet.Value}</color> pv");
        InteractionManager.ShowInfo($"{packet.Value}", entityPrefab.transform.position + Vector3.up * 1f, Color.red);
        
        return Task.CompletedTask;
    }
}