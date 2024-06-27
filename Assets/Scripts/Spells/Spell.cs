using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "ScriptableObjects/Spell")]
public class Spell : ScriptableObject
{
    public int id;
    public Sprite iconSprite;
    public bool isProjectile;
    public string spellName;
    public FovMode fovMode;
    public bool xRay;
    public bool canLaunchOnEntity;
    public int poMin;
    public int poMax;
    public int paCost;
    [SerializeField] private SpellZone zone;

    [Header("ALL VFXs")]
    [SerializeField] private GameObject launcherVfx;
    [SerializeField] private GameObject projectileVfx;
    [SerializeField] private GameObject targetVfx;

    [Header("ALL EFFECTS")]
    [SerializeField] private List<EffectDamage> damage;
    [SerializeField] private List<EffectPush> push;
    [SerializeField] private List<EffectAttract> attract;
    [SerializeField] private List<EffectTeleport> teleport;
    [SerializeField] private List<EffectInvocation> invocation;

    public List<Node> GetZoneNodes(Entity launcher, Node targetNode)
    {
        return zone.GetPositions(launcher, targetNode)
            .Select(position => MapManager.Instance.GetNode(targetNode.gridPosition + position))
            .Where(node => node.type == NodeType.GROUND)
            .ToList();
    }

    public void Launch(Entity launcher, Spell spell, Vector2Int targetPos)
    {
        LaunchVfx(launcher, targetPos);

        SortedDictionary<int, Effect> effects = new();
        foreach (Effect effect in damage) effects.Add(effect.order, effect);
        foreach (Effect effect in push) effects.Add(effect.order, effect);
        foreach (Effect effect in attract) effects.Add(effect.order, effect);
        foreach (Effect effect in teleport) effects.Add(effect.order, effect);
        foreach (Effect effect in invocation) effects.Add(effect.order, effect);

        List<Entity> entities = GetZoneNodes(launcher, MapManager.Instance.GetNode(targetPos))
            .Where(node => node.entity != null)
            .Select(node => node.entity)
            .ToList();

        foreach (KeyValuePair<int, Effect> entry in effects)
        {
            entry.Value.Apply(launcher, spell, entities, targetPos);
        }
    }

    private void LaunchVfx(Entity launcher, Vector2Int targetPos)
    {
        if (launcherVfx != null)
        {
            Instantiate(launcherVfx, launcher.transform.position + new Vector3(0, 0, 200), Quaternion.identity);
        }

        if (projectileVfx != null)
        {
            Vector3 start = launcher.transform.position + new Vector3(0, 0, 200);
            Vector3 end = MapManager.Instance.GetNode(targetPos).worldPosition + new Vector3(0, 0, 200);
            var vfx = Instantiate(projectileVfx, start, Quaternion.identity);
            vfx.transform
                .DOMove(end, 0.4f)
                .OnComplete(() =>
                {
                    if (targetVfx != null)
                    {
                        Instantiate(targetVfx, MapManager.Instance.GetNode(targetPos).worldPosition, Quaternion.identity);
                    }
                    Destroy(vfx);
                });
        }

        if (targetVfx != null && projectileVfx == null)
        {
            Instantiate(targetVfx, MapManager.Instance.GetNode(targetPos).worldPosition + new Vector3(0, 0, 200), Quaternion.identity);
        }
    }
}
