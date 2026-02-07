using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RaceDatabase
{
    private static List<Race> _items;

    public static Race GetById(int id)
    {
        LoadAllItems();
        return _items.FirstOrDefault(item => item.Id == id);
    }
    
    public static List<Race> GetAll()
    {
        LoadAllItems();
        return _items;
    }

    public static void LoadAllItems()
    {
        _items ??= Resources.LoadAll<Race>("").ToList();
    }
}