namespace TypeFest.Net;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false)]
public sealed class OmitAttribute<T> : Attribute
{
    public OmitAttribute(string omitMemberOne, params string[] omitMembers)
    {
        OmitMembers = [omitMemberOne, .. omitMembers];
    }

    public string[] OmitMembers { get; }
}