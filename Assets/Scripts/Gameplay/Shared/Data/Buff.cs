using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "ScriptableObjects/Buff")]
public class Buff : ScriptableObject
{
    public int Id;
    public Sprite Icon;
    public string Name;
    [TextArea] public string Description;
    public int TurnDuration;
    public int MaxStack;
    
    [SerializeReference] public List<BuffEffect> Effects;
    [SerializeReference] public List<ServerEffectBase> StartTurnEffects;
}
