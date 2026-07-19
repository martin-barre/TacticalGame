public static class SpellDatabase
{
    private static readonly ScriptableObjectDatabase<Spell> _db = new(s => s.Id);

    public static Spell GetById(int id) => _db.GetById(id);

    public static void LoadAllItems() => _db.LoadAllItems();
}
