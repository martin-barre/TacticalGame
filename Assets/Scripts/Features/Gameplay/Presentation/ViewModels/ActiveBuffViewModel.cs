public class ActiveBuffViewModel: IViewModel<ActiveBuff>
{
    public readonly Bindable<Buff> Buff;
    public readonly Bindable<int> TurnDuration;
    public readonly Bindable<Entity> Launcher;

    public ActiveBuff Model { get; }

    public ActiveBuffViewModel(ActiveBuff model)
    {
        Model = model;
        Buff = new Bindable<Buff>(model.Buff);
        TurnDuration = new Bindable<int>(model.TurnDuration);
        Launcher = new Bindable<Entity>(model.Launcher);
    }
    
    public void UpdateFromModel()
    {
        Buff.Value = Model.Buff;
        TurnDuration.Value = Model.TurnDuration;
        Launcher.Value = Model.Launcher;
    }
}
