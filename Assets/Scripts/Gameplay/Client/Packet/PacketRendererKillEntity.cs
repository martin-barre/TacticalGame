using System.Threading.Tasks;

public sealed class PacketRendererKillEntity : IPacketRenderer<PacketKillEntity>
{
    public async Task RenderAsync(PacketKillEntity packet)
    {
        Entity entity = GameManagerClient.Instance.GameState.GetEntityById(packet.TargetId);
        EntityPrefabController entityPrefabController = GameManagerClient.Instance.GetEntityPrefab(entity.Id);
        GameManagerClient.Instance.SendChatMessage($"<color=#FF0000>{entity.Race.Name}</color> est mort");
        await entityPrefabController.TriggerAnimAndWaitAsync("Dead");
    }
}