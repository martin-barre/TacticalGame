using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Race", menuName = "ScriptableObjects/Race")]
public class Race : ScriptableObject
{
    [Header("GLOBAL")]
    public int Id;
    public string Name;
    public Sprite IconSprite;
    public EntityPrefabController Prefab;
    public int Hp;
    public int Pa;
    public int Pm;
    public List<Spell> Spells;
    public AiEnum AiEnum;
}
