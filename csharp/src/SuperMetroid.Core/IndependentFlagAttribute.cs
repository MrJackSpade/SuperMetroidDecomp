namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one site, a bool that is a genuine independent fact supplied by data rather than a choice between
/// behaviours (SME6423, SME6424, SME6425). The reason names the fact and where it comes from.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class |
    AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class IndependentFlagAttribute : Attribute
{
    /// <summary>Marks an independent flag; the analyzers read the reason from the attribute argument.</summary>
    /// <param name="reason">The fact the bool states and where it comes from.</param>
    public IndependentFlagAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
