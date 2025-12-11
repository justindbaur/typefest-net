namespace TypeFest.Net;

public interface IFromable<TTarget, TSource>
{
    /// <summary>
    /// Creates a new instance of <see cref="TTarget" /> using the properties from <paramref name="value"/>.
    /// </summary>
    static abstract TTarget From(TSource value);
}