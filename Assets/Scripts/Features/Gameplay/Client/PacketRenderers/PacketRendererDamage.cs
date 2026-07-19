using System;
using System.Threading.Tasks;
using MessagePack;
using VContainer;
using UnityEngine;

public sealed class PacketRendererDamage : IPacketRenderer<PacketDamage>
{
    [Inject] private GameplayClientState _clientState;

    public Task RenderAsync(PacketDamage packet)
    {
        Entity entity = _clientState.GameState.GetEntityById(packet.TargetId);
        if(entity == null) throw new Exception($"Entity with id {packet.TargetId} not found.");
        
        EntityPrefabController entityPrefab = _clientState.GetEntityPrefab(packet.TargetId);
        if(entityPrefab == null) throw new Exception($"EntityPrefab with id {packet.TargetId} not found.");
        
        ViewModelFactory.Entity.NotifyUpdate(entity);
        
        _clientState.SendChatMessage($"<color=#FF0000>{entity.Race.Name}</color> perd <color=#00FF00>{packet.Value}</color> pv");
        InteractionManager.ShowInfo($"{packet.Value}", entityPrefab.transform.position + Vector3.up * 1f, Color.red);
        
        return Task.CompletedTask;
    }
}
