namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one site, a gap-free numbered member family that is a set of distinct identities rather than a
/// sequence (SME6415, SME6416): each member has its own role, so no caller indexes or iterates them. On a type it covers
/// that type's families and hand-written list shape (SME6417). The reason names what makes each member distinct.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Field |
    AttributeTargets.Property, Inherited = false)]
public sealed class NumberedIdentitiesAttribute : Attribute
{
    /// <summary>Marks numbered identities; SME6415, SME6416 and SME6417 read the reason from the attribute argument.</summary>
    /// <param name="reason">What makes each numbered member a distinct identity.</param>
    public NumberedIdentitiesAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
