using System.Threading.Tasks;
using VContainer;

public sealed class PacketRendererSummonEntity : IPacketRenderer<PacketSummonEntity>
{
    [Inject] private GameplayClientState _clientState;

    public Task RenderAsync(PacketSummonEntity packet)
    {
        Entity summoner = _clientState.GameState.GetEntityById(packet.SummonerId);
        Entity invocation = _clientState.GameState.GetEntityById(packet.EntityId);

        if (summoner != null)
        {
            _clientState.SendChatMessage($"<color=#FF0000>{summoner.Race.Name}</color> invoque <color=#00FF00>{invocation.Race.Name}</color>");
        }
        
        _clientState.SpawnEntity(packet.EntityId, packet.RaceId, packet.GridPosition);
        return Task.CompletedTask;
    }
}
