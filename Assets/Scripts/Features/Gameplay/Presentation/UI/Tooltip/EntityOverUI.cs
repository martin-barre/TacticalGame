using TMPro;
using UnityEngine;

public class EntityOverUI : ContextTooltip
{
    [SerializeField] private TMP_Text infoText;

    private EntityViewModel _entityViewModel;
    
    public void SetUI(Entity entity)
    {
        if (_entityViewModel != null)
        {
            _entityViewModel.Hp.OnValueChanged -= OnHpChanged;
        }
        _entityViewModel = ViewModelFactory.Entity.GetOrCreate(entity);
        _entityViewModel.Hp.OnValueChanged += OnHpChanged;

        OnHpChanged(_entityViewModel.Hp.Value);
    }
    
    private void OnHpChanged(int hp) => infoText.text = $"{_entityViewModel.Model.Race.Name}  /  {hp}";
}
