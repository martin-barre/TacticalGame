using System.Threading.Tasks;
using VContainer;

public sealed class PacketRendererNextTurn : IPacketRenderer<PacketNextTurn>
{
    [Inject] private GameplayClientState _clientState;

    public Task RenderAsync(PacketNextTurn packet)
    {
        Entity entity = _clientState.GameState.CurrentEntity;
        entity.Buffs.ForEach(ViewModelFactory.ActiveBuff.NotifyUpdate);
        
        ViewModelFactory.Entity.NotifyUpdate(entity);
        ViewModelFactory.Game.NotifyUpdate(_clientState.GameState);
        
        return Task.CompletedTask;
    }
}
