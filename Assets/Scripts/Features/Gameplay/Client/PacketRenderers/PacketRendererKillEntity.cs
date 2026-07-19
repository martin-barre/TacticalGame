using System;
using System.Threading.Tasks;
using VContainer;

public sealed class PacketRendererKillEntity : IPacketRenderer<PacketKillEntity>
{
    [Inject] private GameplayClientState _clientState;

    public async Task RenderAsync(PacketKillEntity packet)
    {
        Entity entity = packet.RemovedEntity;
        if (entity == null) throw new Exception($"Entity with id {packet.TargetId} not found.");

        EntityPrefabController entityPrefabController = _clientState.GetEntityPrefab(packet.TargetId);
        if (entityPrefabController == null) throw new Exception($"EntityPrefab with id {packet.TargetId} not found.");

        _clientState.SendChatMessage($"<color=#FF0000>{entity.Race.Name}</color> est mort");
        await entityPrefabController.TriggerAnimAndWaitAsync("Dead");
        entityPrefabController.Destroy();
    }
}
