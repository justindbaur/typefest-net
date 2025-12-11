namespace TypeFest.Net;

public interface IIntoable<T>
{
    /// <summary>
    /// Maps the current object into a new instance of <see cref="T" />.
    /// </summary>
    T Into();
}