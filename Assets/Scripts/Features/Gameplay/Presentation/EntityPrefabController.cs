using System;
using System.Threading.Tasks;
using UnityEngine;

public class EntityPrefabController : MonoBehaviour
{
    public event Action OnTriggerAnimation;

    public GameObject overHeadPosition;
    
    private bool _pendingAnimation;

    public void TriggerAnimationEvent()
    {
        OnTriggerAnimation?.Invoke();
    }

    public async Task TriggerAnimAndWaitAsync(string triggerName)
    {
        if (_pendingAnimation) return;
        _pendingAnimation = true;
        OnTriggerAnimation += OnAnimationEnded;
        GetComponentInChildren<Animator>()?.SetTrigger(triggerName);
        while(_pendingAnimation) await Task.Delay(10);
        OnTriggerAnimation -= OnAnimationEnded;
    }
    
    private void OnAnimationEnded()
    {
        _pendingAnimation = false;
    }
}
