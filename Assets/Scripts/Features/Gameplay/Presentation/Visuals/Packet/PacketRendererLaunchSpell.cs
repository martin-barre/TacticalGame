using System.Threading.Tasks;
using UnityEngine;

public sealed class PacketRendererLaunchSpell : IPacketRenderer<PacketLaunchSpell>
{
    public async Task RenderAsync(PacketLaunchSpell packet)
    {
        Spell spell = SpellDatabase.GetById(packet.SpellId);
        Entity entity = GameManagerClient.Instance.GameState.GetEntityById(packet.LauncherId);
        EntityPrefabController entityPrefabController = GameManagerClient.Instance.GetEntityPrefab(entity.Id);
        
        GameManagerClient.Instance.SendChatMessage($"<color=#FF0000>{entity.Race.Name}</color> lance <color=#00FF00>{spell.spellName}</color>");
        
        await entityPrefabController.TriggerAnimAndWaitAsync("Attack");
        
        InteractionManager.ShowInfo(spell.paCost.ToString(), entityPrefabController.transform.position + Vector3.up * 1f, Color.yellow);
        ViewModelFactory.Entity.NotifyUpdate(entity);
        foreach (SFX effect in spell.sfx)
        {
            Vector3 position = effect.target == TargetEnum.Launcher
                ? MapManager.Instance.GridPositionToWorlPosition(entity.GridPosition)
                : MapManager.Instance.GridPositionToWorlPosition(packet.TargetPos);
            GameManagerClient.Instance.InstantiateObject(effect.prefab, position, Quaternion.identity);
        }
    }
}