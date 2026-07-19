using Unity.Netcode;
using UnityEngine;

public abstract class NetworkClientBehaviour : MonoBehaviour
{
    protected virtual void Start()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            enabled = false;
        }
    }
}
