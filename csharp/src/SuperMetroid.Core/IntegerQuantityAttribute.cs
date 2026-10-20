namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one declaration, an integer field or property that is a genuine count or quantity rather than a
/// coded category, though it is compared to literals (SME6421, SME6422). The reason names what it counts.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class IntegerQuantityAttribute : Attribute
{
    /// <summary>Marks a quantity; SME6421 and SME6422 read the reason from the attribute argument.</summary>
    /// <param name="reason">What the member counts or measures.</param>
    public IntegerQuantityAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
