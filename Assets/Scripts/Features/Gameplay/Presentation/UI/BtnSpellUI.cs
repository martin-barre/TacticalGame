using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnSpellUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button btnSpell;
    [SerializeField] private TMP_Text txtCooldown;
    
    private RectTransform _rectTransform;
    private EntityViewModel _entityViewModel;
    private Spell _spell;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        Unbind();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 position = _rectTransform.GetScreenRectTransformPosition(RectTransformPosition.Top);
        TooltipUI.Instance.Show<SpellTooltipUI>(position, TooltipPosition.Top, tooltip => tooltip.SetUI(_spell));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide<SpellTooltipUI>();
    }

    public void Bind(Entity entity, Spell spell)
    {
        Unbind();
        
        _spell = spell;
        _entityViewModel = ViewModelFactory.Entity.GetOrCreate(entity);
        
        btnSpell.onClick.AddListener(OnButtonClick);
        btnSpell.image.sprite = _spell.iconSprite;
        
        // BIND
        _entityViewModel.Pa.OnValueChanged += OnPaChanged;
        
        // UPDATE UI
        RefreshUI();
    }
    
    private void Unbind()
    {
        if (_entityViewModel == null) return;
        btnSpell.onClick.RemoveAllListeners();
        _entityViewModel.Pa.OnValueChanged -= OnPaChanged;
        _entityViewModel = null;
    }
    
    private void RefreshUI()
    {
        btnSpell.interactable = _entityViewModel.Pa.Value >= _spell.paCost;
        txtCooldown.text = "";
    }

    private void OnButtonClick() => InteractionManager.Instance.DisplaySpellNode(_spell.Id);
    private void OnPaChanged(int pa) => RefreshUI();
}
