using System.Collections.Generic;

public static class BindableExtensions
{
    public static BindableList<T> ToObservableList<T>(this IEnumerable<T> source)
    {
        return new BindableList<T>(source);
    }
}