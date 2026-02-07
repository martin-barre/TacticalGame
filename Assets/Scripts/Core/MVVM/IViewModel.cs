public interface IViewModel<T>
{
    public T Model { get; }
    public void UpdateFromModel();
}
