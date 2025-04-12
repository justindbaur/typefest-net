namespace TypeFest.Net;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false)]
public sealed class PickAttribute<T> : Attribute
{
    public PickAttribute(string pickMemberOne, params string[] pickMembers)
    {
        PickMembers = [pickMemberOne, .. pickMembers];
    }

    public IReadOnlyList<string> PickMembers { get; }
}