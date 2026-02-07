using System;
using System.Threading.Tasks;

public sealed class PacketRendererBuff : IPacketRenderer<PacketBuff>
{
    public Task RenderAsync(PacketBuff packet)
    {
        Entity target = GameManagerClient.Instance.GameState.GetEntityById(packet.TargetId);
        if(target == null) throw new Exception($"Entity with id {packet.TargetId} not found.");
        
        Entity launcher = GameManagerClient.Instance.GameState.GetEntityById(packet.LauncherId);
        if(launcher == null) throw new Exception($"Entity with id {packet.LauncherId} not found.");
        
        Buff buff = BuffDatabase.GetById(packet.BuffId);
        if(buff == null) throw new Exception($"Buff with id {packet.BuffId} not found.");
        
        ViewModelFactory.Entity.NotifyUpdate(target);
        GameManagerClient.Instance.SendChatMessage($"<color=#FF0000>{target.Race.Name}</color> gagne <color=#00FF00>{buff.Name}");
        
        return Task.CompletedTask;
    }
}