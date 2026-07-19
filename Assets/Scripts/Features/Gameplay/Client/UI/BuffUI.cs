using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuffUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image imageSprite;
    [SerializeField] private GameObject panelTurnDuration;
    [SerializeField] private TMP_Text textTurnDuration;

    private RectTransform _rectTransform;
    private ActiveBuffViewModel _activeBuffViewModel;

    public void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 position = _rectTransform.GetScreenRectTransformPosition(RectTransformPosition.Bottom);
        string text = $"<font-weight=\"700\">{_activeBuffViewModel.Buff.Value.Name}</font-weight><br>{_activeBuffViewModel.Buff.Value.Description}";
        TooltipUI.Instance.Show<BasicTooltipUI>(position, TooltipPosition.Bottom, tooltip => tooltip.SetUI(text));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide<BasicTooltipUI>();
    }
    
    private void OnDestroy()
    {
        Unbind();
    }
    
    public void Bind(ActiveBuff activeBuff)
    {
        Unbind();
        
        _activeBuffViewModel = ViewModelFactory.ActiveBuff.GetOrCreate(activeBuff);
        _activeBuffViewModel.TurnDuration.OnValueChanged += UpdateTurnDuration;
        
        imageSprite.sprite = activeBuff.Buff.Icon;
        UpdateTurnDuration(_activeBuffViewModel.TurnDuration.Value);
    }
    
    private void Unbind()
    {
        if (_activeBuffViewModel == null) return;
        _activeBuffViewModel.TurnDuration.OnValueChanged -= UpdateTurnDuration;
        _activeBuffViewModel = null;
    }
    
    private void UpdateTurnDuration(int turnDuration)
    {
        panelTurnDuration.SetActive(turnDuration > 0);
        textTurnDuration.text = $"{turnDuration}";
    }
}
