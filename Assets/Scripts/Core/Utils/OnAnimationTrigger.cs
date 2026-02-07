using UnityEngine;
using UnityEngine.Events;

public class OnAnimationTrigger : MonoBehaviour
{
    public UnityEvent OnTrigger;
    
    public void Apply()
    {
        OnTrigger.Invoke();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
