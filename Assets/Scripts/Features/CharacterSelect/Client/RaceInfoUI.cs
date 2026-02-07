using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaceInfoUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<int> OnCharacterSelected;
    
    [SerializeField] private Image imgRace;

    private RectTransform _rectTransform;
    private Race _race;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetRace(Race race)
    {
        _race = race;
        imgRace.sprite = race.IconSprite;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 position = _rectTransform.GetScreenRectTransformPosition(RectTransformPosition.Top);
        TooltipUI.Instance.Show<BasicTooltipUI>(position, TooltipPosition.Top, tooltip => tooltip.SetUI(_race.Name));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide<BasicTooltipUI>();
    }

    public void OnClick()
    {
        OnCharacterSelected?.Invoke(_race.Id);
    }
}
