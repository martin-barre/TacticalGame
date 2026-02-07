using TMPro;
using UnityEngine;

public class BasicTooltipUI : ContextTooltip
{
    [SerializeField] private TMP_Text infoText;

    private EntityViewModel _entityViewModel;
    
    public void SetUI(string text)
    {
        infoText.text = text;
    }
}
