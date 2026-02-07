using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BuffDatabase
{
    private static List<Buff> _items;

    public static Buff GetById(int id)
    {
        LoadAllItems();
        return _items.FirstOrDefault(item => item.Id == id);
    }

    public static void LoadAllItems()
    {
        _items ??= Resources.LoadAll<Buff>("").ToList();
    }
}