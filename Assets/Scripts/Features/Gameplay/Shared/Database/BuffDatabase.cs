public static class BuffDatabase
{
    private static readonly ScriptableObjectDatabase<Buff> _db = new(b => b.Id);

    public static Buff GetById(int id) => _db.GetById(id);

    public static void LoadAllItems() => _db.LoadAllItems();
}
