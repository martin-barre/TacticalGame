using System.Threading.Tasks;

public sealed class PacketRendererSetGameLogic : IPacketRenderer<PacketSetGameLogic>
{
    public Task RenderAsync(PacketSetGameLogic packet)
    {
        ViewModelFactory.Game.NotifyUpdate(GameManagerClient.Instance.GameState);
        MapManager.Instance.ActiveTilemapSpawns(!packet.IsStarted);
        
        return Task.CompletedTask;
    }
}