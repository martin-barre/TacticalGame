using UnityEngine;

public class SpellbarUI : MonoBehaviour
{
    [SerializeField] private GameObject panelSpellBar;
    [SerializeField] private BtnSpellUI btnSpellUI;
    
    private GameStateViewModel _gameStateViewModel;
    
    private void Start()
    {
        _gameStateViewModel = ViewModelFactory.Game.GetOrCreate(GameManagerClient.Instance.GameState);
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged += SetUI;
    }
    
    private void OnDestroy()
    {
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged -= SetUI;
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
            BtnSpellUI instance = Instantiate(btnSpellUI, panelSpellBar.transform);
            instance.Bind(entity, spell);
        }
    }

    private Entity GetNextOwnEntity()
    {
        int currentPlayerIndex = _gameStateViewModel.CurrentEntityIndex.Value;
        for (int i = 0; i < GameManagerClient.Instance.GameState.Entities.Count; i++)
        {
            Entity entity = GameManagerClient.Instance.GameState.GetEntityByIndex(currentPlayerIndex);
            if (entity.Team == GameManagerClient.Instance.Team && entity.IsPlayer)
            {
                return entity;
            }
            
            currentPlayerIndex++;
            if(currentPlayerIndex >= GameManagerClient.Instance.GameState.Entities.Count) currentPlayerIndex = 0;
        }
        return null;
    }
}
