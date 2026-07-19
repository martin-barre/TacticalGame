using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoWindow : WindowUI
{
    [SerializeField] private GameObject panelBuffsContent;
    [SerializeField] private Image raceSprite;
    [SerializeField] private TMP_Text textEntityName;
    [SerializeField] private TMP_Text textHp;
    [SerializeField] private TMP_Text textPa;
    [SerializeField] private TMP_Text textPm;
    [SerializeField] private BuffUI buffsUI;

    private EntityViewModel _entityViewModel;

    protected override string WindowIdSuffix => _entityViewModel?.Model.Id.ToString() ?? string.Empty; 
    
    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(Entity entity)
    {
        Unbind();
        
        _entityViewModel = ViewModelFactory.Entity.GetOrCreate(entity);
        _entityViewModel.Buffs.OnListChanged += RefreshBuffsUI;
        _entityViewModel.Hp.OnValueChanged += RefreshHpUI;
        _entityViewModel.Pa.OnValueChanged += RefreshPaUI;
        _entityViewModel.Pm.OnValueChanged += RefreshPmUI;

        raceSprite.sprite = _entityViewModel.Model.Race.IconSprite;
        textEntityName.text = _entityViewModel.Model.Race.Name;
        RefreshBuffsUI();
        RefreshHpUI(_entityViewModel.Hp.Value);
        RefreshPaUI(_entityViewModel.Pa.Value);
        RefreshPmUI(_entityViewModel.Pm.Value);
    }

    private void Unbind()
    {
        if (_entityViewModel == null) return;
        _entityViewModel.Buffs.OnListChanged -= RefreshBuffsUI;
        _entityViewModel.Hp.OnValueChanged -= RefreshHpUI;
        _entityViewModel.Pa.OnValueChanged -= RefreshPaUI;
        _entityViewModel.Pm.OnValueChanged -= RefreshPmUI;
        _entityViewModel = null;
    }

    private void RefreshBuffsUI()
    {
        foreach (Transform child in panelBuffsContent.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (ActiveBuff activeBuff in _entityViewModel.Buffs)
        {
            BuffUI buffUI = Instantiate(buffsUI, panelBuffsContent.transform);
            buffUI.Bind(activeBuff);
        }
    }

    private void RefreshHpUI(int hp) => textHp.text = $"{hp} / {_entityViewModel.Model.Race.Hp}";
    private void RefreshPaUI(int pa) => textPa.text = $"{pa} / {_entityViewModel.Model.Race.Pa}";
    private void RefreshPmUI(int pm) => textPm.text = $"{pm} / {_entityViewModel.Model.Race.Pm}";
}