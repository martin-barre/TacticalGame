using VContainer;
using UnityEngine;

public class SpellbarUI : ClientStateBoundBehaviour
{
    [SerializeField] private GameObject panelSpellBar;
    [SerializeField] private BtnSpellUI btnSpellUI;

    private GameStateViewModel _gameStateViewModel;

    [Inject] private InjectedPrefabFactory _prefabFactory;

    protected override void BindClientState()
    {
        _gameStateViewModel = ViewModelFactory.Game.GetOrCreate(ClientState.GameState);
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged += SetUI;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_gameStateViewModel != null)
        {
            _gameStateViewModel.CurrentEntityIndex.OnValueChanged -= SetUI;
        }
    }

    private void SetUI(int playerId)
    {
        for (int i = 0; i < panelSpellBar.transform.childCount; i++)
        {
            Destroy(panelSpellBar.transform.GetChild(i).gameObject);
        }

        Entity entity = GetNextOwnEntity();
        if (entity == null) return;

        foreach (Spell spell in entity.Race.Spells)
        {
            BtnSpellUI instance = _prefabFactory.Instantiate(btnSpellUI, panelSpellBar.transform);
            instance.Bind(entity, spell);
        }
    }

    private Entity GetNextOwnEntity()
    {
        int currentPlayerIndex = _gameStateViewModel.CurrentEntityIndex.Value;
        for (int i = 0; i < ClientState.GameState.Entities.Count; i++)
        {
            Entity entity = ClientState.GameState.GetEntityByIndex(currentPlayerIndex);
            if (entity.Team == ClientState.Team && entity.IsPlayer)
            {
                return entity;
            }

            currentPlayerIndex++;
            if (currentPlayerIndex >= ClientState.GameState.Entities.Count)
            {
                currentPlayerIndex = 0;
            }
        }
        return null;
    }
}
