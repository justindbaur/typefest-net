namespace TypeFest.Net;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class MapToAttribute<T> : Attribute
{
    /// <summary>
    /// A list of members to ignore when mapping.
    /// </summary>
    public IReadOnlyList<string>? Ignore { get; set; }
}