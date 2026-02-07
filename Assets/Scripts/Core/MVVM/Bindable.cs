using System;

[Serializable]
public class Bindable<T>
{
    private T _value;

    /// <summary>
    /// Événement déclenché lorsque la valeur change.
    /// </summary>
    public event Action<T> OnValueChanged;

    /// <summary>
    /// Constructeur vide.
    /// </summary>
    public Bindable()
    {
        _value = default;
    }
    
    /// <summary>
    /// Constructeur qui initialise la valeur.
    /// </summary>
    /// <param name="initialValue">Valeur initiale.</param>
    public Bindable(T initialValue)
    {
        _value = initialValue;
    }
    
    /// <summary>
    /// Constructeur qui initialise la valeur.
    /// </summary>
    /// <param name="initialValue">Valeur initiale.</param>
    public Bindable(Bindable<T> initialValue)
    {
        _value = initialValue.Value;
    }

    /// <summary>
    /// La valeur actuelle de la variable.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }
}