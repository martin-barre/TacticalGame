using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
    [SerializeField] private List<GameObject> gameObjectClient;
    [SerializeField] private List<GameObject> gameObjectServer;

    private void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            gameObjectServer.ForEach(g => Instantiate(g).GetComponent<NetworkObject>()?.Spawn());
        }
        if (NetworkManager.Singleton.IsClient)
        {
            gameObjectClient.ForEach(g => Instantiate(g).GetComponent<NetworkObject>()?.Spawn());
        }
    }
}
