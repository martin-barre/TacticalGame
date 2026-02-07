using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellTooltipUI : ContextTooltip
{
    [SerializeField] private Image imageIcon;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textPa;
    [SerializeField] private TMP_Text textPo;
    
    public void SetUI(Spell spell)
    {
        imageIcon.sprite = spell.iconSprite;
        textName.text = spell.spellName;
        textPa.text = $"PA : {spell.paCost}";
        textPo.text = $"PO : {spell.poMin}-{spell.poMax}";
    }
}
