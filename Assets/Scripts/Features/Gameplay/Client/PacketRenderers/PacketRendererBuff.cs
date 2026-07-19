using System;
using System.Threading.Tasks;
using VContainer;

public sealed class PacketRendererBuff : IPacketRenderer<PacketBuff>
{
    [Inject] private GameplayClientState _clientState;

    public Task RenderAsync(PacketBuff packet)
    {
        Entity target = _clientState.GameState.GetEntityById(packet.TargetId);
        if(target == null) throw new Exception($"Entity with id {packet.TargetId} not found.");
        
        Entity launcher = _clientState.GameState.GetEntityById(packet.LauncherId);
        if(launcher == null) throw new Exception($"Entity with id {packet.LauncherId} not found.");
        
        Buff buff = BuffDatabase.GetById(packet.BuffId);
        if(buff == null) throw new Exception($"Buff with id {packet.BuffId} not found.");
        
        ViewModelFactory.Entity.NotifyUpdate(target);
        _clientState.SendChatMessage($"<color=#FF0000>{target.Race.Name}</color> gagne <color=#00FF00>{buff.Name}");
        
        return Task.CompletedTask;
    }
}
