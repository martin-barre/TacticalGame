using System.Threading.Tasks;

public sealed class PacketRendererSummonEntity : IPacketRenderer<PacketSummonEntity>
{
    public Task RenderAsync(PacketSummonEntity packet)
    {
        Entity summoner = GameManagerClient.Instance.GameState.GetEntityById(packet.SummonerId);
        Entity invocation = GameManagerClient.Instance.GameState.GetEntityById(packet.EntityId);

        if (summoner != null)
        {
            GameManagerClient.Instance.SendChatMessage($"<color=#FF0000>{summoner.Race.Name}</color> invoque <color=#00FF00>{invocation.Race.Name}</color>");
        }
        
        GameManagerClient.Instance.SpawnEntity(packet.EntityId, packet.RaceId, packet.GridPosition);
        return Task.CompletedTask;
    }
}