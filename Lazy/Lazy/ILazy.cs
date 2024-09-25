namespace Lazy;

public interface ILazy<T>
{
    T Get();
}