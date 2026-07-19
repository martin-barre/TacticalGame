using System.Collections.Generic;

public static class RaceDatabase
{
    private static readonly ScriptableObjectDatabase<Race> _db = new(r => r.Id);

    public static Race GetById(int id) => _db.GetById(id);

    public static List<Race> GetAll() => _db.GetAll();

    public static void LoadAllItems() => _db.LoadAllItems();
}
