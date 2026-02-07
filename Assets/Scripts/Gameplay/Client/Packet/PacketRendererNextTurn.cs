using System.Threading.Tasks;

public sealed class PacketRendererNextTurn : IPacketRenderer<PacketNextTurn>
{
    public Task RenderAsync(PacketNextTurn packet)
    {
        Entity entity = GameManagerClient.Instance.GameState.CurrentEntity;
        entity.Buffs.ForEach(ViewModelFactory.ActiveBuff.NotifyUpdate);
        
        ViewModelFactory.Entity.NotifyUpdate(entity);
        ViewModelFactory.Game.NotifyUpdate(GameManagerClient.Instance.GameState);
        
        return Task.CompletedTask;
    }
}