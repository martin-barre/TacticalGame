using System.Threading.Tasks;
using VContainer;
using UnityEngine;

public sealed class PacketRendererLaunchSpell : IPacketRenderer<PacketLaunchSpell>
{
    [Inject] private MapManager _mapManager;
    [Inject] private GameplayClientState _clientState;
    
    public async Task RenderAsync(PacketLaunchSpell packet)
    {
        Spell spell = SpellDatabase.GetById(packet.SpellId);
        Entity entity = _clientState.GameState.GetEntityById(packet.LauncherId);
        EntityPrefabController entityPrefabController = _clientState.GetEntityPrefab(entity.Id);
        
        _clientState.SendChatMessage($"<color=#FF0000>{entity.Race.Name}</color> lance <color=#00FF00>{spell.spellName}</color>");
        
        await entityPrefabController.TriggerAnimAndWaitAsync("Attack");
        
        InteractionManager.ShowInfo(spell.paCost.ToString(), entityPrefabController.transform.position + Vector3.up * 1f, Color.yellow);
        ViewModelFactory.Entity.NotifyUpdate(entity);
        foreach (SFX effect in spell.sfx)
        {
            Vector3 position = effect.target == TargetEnum.Launcher
                ? _mapManager.GridToWorld(entity.GridPosition)
                : _mapManager.GridToWorld(packet.TargetPos);
            _clientState.InstantiateObject(effect.prefab, position, Quaternion.identity);
        }
    }
}
