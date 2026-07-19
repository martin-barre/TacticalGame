public class GameStateViewModel: IViewModel<GameState>
{
    public readonly Bindable<bool> IsStarted;
    public readonly Bindable<int> CurrentEntityIndex;
    public readonly BindableList<Entity> Entities;

    public GameState Model { get; }

    public GameStateViewModel(GameState model)
    {
        Model = model;
        IsStarted = new Bindable<bool>(model.IsStarted);
        CurrentEntityIndex = new Bindable<int>(model.CurrentEntityIndex);
        Entities = new BindableList<Entity>(model.Entities);
    }
    
    public void UpdateFromModel()
    {
        IsStarted.Value = Model.IsStarted;
        CurrentEntityIndex.Value = Model.CurrentEntityIndex;
        Entities.ReplaceAll(Model.Entities);
    }
}
