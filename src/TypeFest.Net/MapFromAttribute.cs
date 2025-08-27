namespace TypeFest.Net;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class MapFromAttribute<T> : Attribute
{
    /// <summary>
    /// A list of members to ignore when mapping.
    /// </summary>
    public string[]? Ignore { get; set; }
}