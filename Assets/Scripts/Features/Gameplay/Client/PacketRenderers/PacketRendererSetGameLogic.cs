using System.Threading.Tasks;
using VContainer;

public sealed class PacketRendererSetGameLogic : IPacketRenderer<PacketSetGameLogic>
{
    [Inject] private MapManager _mapManager;
    [Inject] private GameplayClientState _clientState;
    
    public Task RenderAsync(PacketSetGameLogic packet)
    {
        ViewModelFactory.Game.NotifyUpdate(_clientState.GameState);
        _mapManager.SetSpawnMarkersVisible(!packet.IsStarted);
        
        return Task.CompletedTask;
    }
}
