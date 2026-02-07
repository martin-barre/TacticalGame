using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BindableList<T> : IList<T>
{
    private readonly List<T> _list;

    public event Action<T> OnItemAdded;
    public event Action<T> OnItemRemoved;
    public event Action OnListChanged;

    public BindableList() => _list = new List<T>();
    public BindableList(int capacity) => _list = new List<T>(capacity);
    public BindableList(IEnumerable<T> collection) => _list = new List<T>(collection);
    
    public T this[int index]
    {
        get => _list[index];
        set
        {
            _list[index] = value;
            OnListChanged?.Invoke();
        }
    }

    public void Add(T item)
    {
        _list.Add(item);
        OnItemAdded?.Invoke(item);
        OnListChanged?.Invoke();
    }

    public bool Remove(T item)
    {
        if (!_list.Remove(item)) return false;
        OnItemRemoved?.Invoke(item);
        OnListChanged?.Invoke();
        return true;
    }

    public void Clear()
    {
        _list.Clear();
        OnListChanged?.Invoke();
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;
    public bool Contains(T item) => _list.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    public BindableList<T> ToObservableList() => new (_list);
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int IndexOf(T item) => _list.IndexOf(item);
    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        OnItemAdded?.Invoke(item);
        OnListChanged?.Invoke();
    }

    public void RemoveAt(int index)
    {
        T item = _list[index];
        _list.RemoveAt(index);
        OnItemRemoved?.Invoke(item);
        OnListChanged?.Invoke();
    }
    
    public int RemoveAll(Predicate<T> match)
    {
        List<T> removedItems = _list.FindAll(match);
        int count = _list.RemoveAll(match);

        foreach (T item in removedItems)
            OnItemRemoved?.Invoke(item);

        if (count > 0)
            OnListChanged?.Invoke();

        return count;
    }
    
    public void ReplaceAll(IEnumerable<T> items)
    {
        if (_list.SequenceEqual(items)) return;
        
        _list.Clear();
        foreach (T item in items)
        {
            _list.Add(item);
            OnItemAdded?.Invoke(item);
        }
        OnListChanged?.Invoke();
    }
    
    public void ForEach(Action<T> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        foreach (T item in _list)
            action(item);
    }
}