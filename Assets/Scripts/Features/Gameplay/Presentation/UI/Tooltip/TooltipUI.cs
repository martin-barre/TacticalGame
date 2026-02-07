using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TooltipPosition
{
    Top,
    Bottom,
    Left,
    Right
}

public class ContextTooltip : MonoBehaviour {}

public class TooltipUI : MonoSingleton<TooltipUI>
{
    private Dictionary<Type, ContextTooltip> _tooltips = new();
    private RectTransform _rectTransform;
    private Canvas _canvas;

    private void Awake()
    {
        _tooltips = GetComponentsInChildren<ContextTooltip>(true).ToDictionary(o => o.GetType(), o => o);
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void Show<T>(Vector2 position, TooltipPosition tooltipPosition, Action<T> setup = null) where T : ContextTooltip
    {
        if(_tooltips.TryGetValue(typeof(T), out ContextTooltip content))
        {
            if (setup != null && content is T typedContent)
            {
                setup.Invoke(typedContent); // setup personnalisé ici
            }

            if (!content.gameObject.activeSelf)
            {
                transform.SetAsLastSibling();
            }
            
            content.gameObject.SetActive(true);
            
            SetPivotFromPosition(tooltipPosition);
        
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                position,
                _canvas.worldCamera,
                out pos
            );
            _rectTransform.anchoredPosition = ClampToScreen(pos);
        }
        else
        {
            Debug.LogWarning($"Tooltip of type {typeof(T).Name} not found in tooltip registry.");
        }
    }

    public void Hide<T>() where T : ContextTooltip
    {
        if (_tooltips.TryGetValue(typeof(T), out ContextTooltip content))
        {
            content.gameObject.SetActive(false);
        }
    }
    
    private void SetPivotFromPosition(TooltipPosition position)
    {
        _rectTransform.pivot = position switch
        {
            TooltipPosition.Top => new Vector2(0.5f, 0f),
            TooltipPosition.Bottom => new Vector2(0.5f, 1f),
            TooltipPosition.Left => new Vector2(1f, 0.5f),
            TooltipPosition.Right => new Vector2(0f, 0.5f),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };
    }

    private Vector2 ClampToScreen(Vector2 pos)
    {
        Vector2 size = _rectTransform.sizeDelta;
        Vector2 canvasSize = (_canvas.transform as RectTransform).sizeDelta;

        pos.x = Mathf.Clamp(pos.x, -canvasSize.x / 2 + size.x / 2, canvasSize.x / 2 - size.x / 2);
        pos.y = Mathf.Clamp(pos.y, -canvasSize.y / 2 + size.y / 2, canvasSize.y / 2 - size.y / 2);

        return pos;
    }
}
