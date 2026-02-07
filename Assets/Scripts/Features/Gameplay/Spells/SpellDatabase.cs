using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SpellDatabase
{
    private static List<Spell> _items;
    
    public static Spell GetById(int id)
    {
        LoadAllItems();
        return _items.FirstOrDefault(item => item.Id == id);
    }

    public static void LoadAllItems()
    {
        _items ??= Resources.LoadAll<Spell>("").ToList();
    }
}