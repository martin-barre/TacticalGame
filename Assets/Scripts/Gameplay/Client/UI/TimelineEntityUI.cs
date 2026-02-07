using System;
using UnityEngine;
using UnityEngine.UI;

public class TimelineEntityUI : MonoBehaviour
{
    [SerializeField] private Image imgEntity;
    [SerializeField] private Slider sliderHealth;
    [SerializeField] private Image imgSlider;
    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;

    private EntityViewModel _entityViewModel;

    private void OnDisable()
    {
        Unbind();
    }

    public void Bind(Entity entity)
    {
        Unbind();
        
        // BIND
        _entityViewModel = ViewModelFactory.Entity.GetOrCreate(entity);
        _entityViewModel.Hp.OnValueChanged += UpdateHp;
        
        // UPDATE UI
        imgEntity.sprite = entity.Race.IconSprite;
        imgEntity.enabled = entity.Race != null;
        sliderHealth.maxValue = entity.Race.Hp;
        imgSlider.color = entity.Team == Team.Red ? Color.red : Color.blue;
        UpdateHp(entity.Hp);
    }
    
    private void Unbind()
    {
        if (_entityViewModel == null) return;
        
        _entityViewModel.Hp.OnValueChanged -= UpdateHp;
        _entityViewModel = null;
    }
    
    private void UpdateHp(int hp) => sliderHealth.value = hp;
}
