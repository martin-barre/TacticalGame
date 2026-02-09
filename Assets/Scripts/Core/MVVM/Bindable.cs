using System;
using UnityEngine;

[Serializable]
public class Bindable<T>
{
    [SerializeField] private T value;

    /// <summary>
    /// Événement déclenché lorsque la valeur change.
    /// </summary>
    public event Action<T> OnValueChanged;

    /// <summary>
    /// Constructeur vide.
    /// </summary>
    public Bindable()
    {
        value = default;
    }
    
    /// <summary>
    /// Constructeur qui initialise la valeur.
    /// </summary>
    /// <param name="initialValue">Valeur initiale.</param>
    public Bindable(T initialValue)
    {
        value = initialValue;
    }
    
    /// <summary>
    /// Constructeur qui initialise la valeur.
    /// </summary>
    /// <param name="initialValue">Valeur initiale.</param>
    public Bindable(Bindable<T> initialValue)
    {
        value = initialValue.Value;
    }

    /// <summary>
    /// La valeur actuelle de la variable.
    /// </summary>
    public T Value
    {
        get => value;
        set
        {
            if (!Equals(this.value, value))
            {
                this.value = value;
                OnValueChanged?.Invoke(this.value);
            }
        }
    }
}
