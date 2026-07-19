using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InjectedPrefabFactory
{
    private readonly IObjectResolver _container;

    public InjectedPrefabFactory(IObjectResolver container)
    {
        _container = container;
    }

    public T Instantiate<T>(T prefab, Transform parent = null) where T : Component
    {
        T instance = Object.Instantiate(prefab, parent);
        _container.InjectGameObject(instance.gameObject);
        return instance;
    }

    public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        T instance = Object.Instantiate(prefab, position, rotation);
        _container.InjectGameObject(instance.gameObject);
        return instance;
    }

    public GameObject Instantiate(GameObject prefab)
    {
        GameObject instance = Object.Instantiate(prefab);
        _container.InjectGameObject(instance);
        return instance;
    }

    public GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = Object.Instantiate(prefab, position, rotation);
        _container.InjectGameObject(instance);
        return instance;
    }
}
