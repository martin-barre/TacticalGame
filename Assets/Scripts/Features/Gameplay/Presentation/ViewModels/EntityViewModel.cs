public class EntityViewModel: IViewModel<Entity>
{
    public readonly Bindable<int> Hp;
    public readonly Bindable<int> Pa;
    public readonly Bindable<int> Pm;
    public readonly BindableList<ActiveBuff> Buffs;

    public Entity Model { get; }

    public EntityViewModel(Entity model)
    {
        Model = model;
        Hp = new Bindable<int>(model.Hp);
        Pa = new Bindable<int>(model.Pa);
        Pm = new Bindable<int>(model.Pm);
        Buffs = new BindableList<ActiveBuff>(model.Buffs);
    }
    
    public void UpdateFromModel()
    {
        Hp.Value = Model.Hp;
        Pa.Value = Model.Pa;
        Pm.Value = Model.Pm;
        Buffs.ReplaceAll(Model.Buffs);
    }
}
