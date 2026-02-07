public static class ViewModelFactory {
    public static readonly ViewModelRegistry<Entity, EntityViewModel> Entity = new(e => new EntityViewModel(e));
    public static readonly ViewModelRegistry<GameState, GameStateViewModel> Game = new(e => new GameStateViewModel(e));
    public static readonly ViewModelRegistry<ActiveBuff, ActiveBuffViewModel> ActiveBuff = new(e => new ActiveBuffViewModel(e));
}