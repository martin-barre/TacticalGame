using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptableObjectDatabase<T> where T : ScriptableObject
{
    private readonly Func<T, int> _idSelector;
    private List<T> _items;

    public ScriptableObjectDatabase(Func<T, int> idSelector)
    {
        _idSelector = idSelector;
    }

    public T GetById(int id)
    {
        LoadAllItems();
        return _items.FirstOrDefault(item => _idSelector(item) == id);
    }

    public List<T> GetAll()
    {
        LoadAllItems();
        return _items;
    }

    public void LoadAllItems()
    {
        _items ??= Resources.LoadAll<T>("").ToList();
    }
}
