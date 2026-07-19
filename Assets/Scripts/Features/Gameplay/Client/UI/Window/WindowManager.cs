using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoSingleton<WindowManager>
{
    private List<WindowUI> _windows = new List<WindowUI>();
    private int _sortingOrderCounter = 0;

    private void Start()
    {
        // Récupère toutes les fenêtres présentes dans la hiérarchie
        _windows.AddRange(GetComponentsInChildren<WindowUI>(true));
    }

    public T CreateWindow<T>(T windowUI) where T : WindowUI
    {
        T instance = Instantiate(windowUI, gameObject.transform);
        
        if (!_windows.Contains(instance))
            _windows.Add(instance);
        
        return instance;
    }

    public void BringToFront(WindowUI window)
    {
        _sortingOrderCounter++;
        window.SetSortingOrder(_sortingOrderCounter);
    }

    public WindowUI GetWindow(string id)
    {
        return _windows.Find(w => w.WindowId == id);
    }

    public void OpenWindow(string id)
    {
        var window = GetWindow(id);
        if (window != null)
            window.Open();
    }

    public void CloseWindow(string id)
    {
        var window = GetWindow(id);
        if (window != null)
            window.Close();
    }
}