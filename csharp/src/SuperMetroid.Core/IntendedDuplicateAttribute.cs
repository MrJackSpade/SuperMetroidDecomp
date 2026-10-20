namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one site, a method body or record that another type repeats but which must stay a separate
/// copy (SME6419, SME6420). The reason names what keeps the two from being one shared implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class IntendedDuplicateAttribute : Attribute
{
    /// <summary>Marks a deliberate copy; SME6419 and SME6420 read the reason from the attribute argument.</summary>
    /// <param name="reason">Why this copy cannot be the shared implementation.</param>
    public IntendedDuplicateAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
