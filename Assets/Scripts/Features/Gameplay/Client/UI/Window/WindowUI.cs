using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private string windowId;
    [SerializeField] private RectTransform header;

    protected virtual string WindowIdSuffix { get; } = string.Empty;
    
    public string WindowId => windowId + WindowIdSuffix;

    private Canvas _canvas;
    private RectTransform _rectTransform;

    private Vector2 _dragOffset;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();

        if (header != null)
        {
            // Ajoute un EventTrigger automatiquement si pas déjà présent
            var trigger = header.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = header.gameObject.AddComponent<EventTrigger>();

            AddEventTrigger(trigger, EventTriggerType.BeginDrag, OnBeginDrag);
            AddEventTrigger(trigger, EventTriggerType.Drag, OnDrag);
        }
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, Action<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => callback(data));
        trigger.triggers.Add(entry);
    }

    private void OnBeginDrag(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform,
            pointerData.position,
            pointerData.pressEventCamera,
            out _dragOffset
        );
    }

    private void OnDrag(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)_rectTransform.parent,
            pointerData.position,
            pointerData.pressEventCamera,
            out localMousePos
        );

        _rectTransform.localPosition = localMousePos - _dragOffset;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        WindowManager.Instance.BringToFront(this);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        WindowManager.Instance.BringToFront(this);
    }

    public void SetSortingOrder(int order)
    {
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = order;
    }
}
