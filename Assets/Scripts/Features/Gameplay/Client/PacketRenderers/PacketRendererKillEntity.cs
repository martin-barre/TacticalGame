using System.Threading.Tasks;
using VContainer;

public sealed class PacketRendererKillEntity : IPacketRenderer<PacketKillEntity>
{
    [Inject] private GameplayClientState _clientState;

    public async Task RenderAsync(PacketKillEntity packet)
    {
        Entity entity = _clientState.GameState.GetEntityById(packet.TargetId);
        EntityPrefabController entityPrefabController = _clientState.GetEntityPrefab(entity.Id);
        _clientState.SendChatMessage($"<color=#FF0000>{entity.Race.Name}</color> est mort");
        await entityPrefabController.TriggerAnimAndWaitAsync("Dead");
    }
}
