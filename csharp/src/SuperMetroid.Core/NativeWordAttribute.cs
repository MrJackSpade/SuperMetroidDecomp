namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one declaration, an integer field or property that holds a packed cartridge word whose bit fields
/// a typed view decodes, rather than one coded category (SME6421, SME6422). Its zero test asks whether the word holds
/// anything at all. The reason names the word and its typed view.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class NativeWordAttribute : Attribute
{
    /// <summary>Marks a packed native word; SME6421 and SME6422 read the reason from the attribute argument.</summary>
    /// <param name="reason">The native word and the typed view that decodes it.</param>
    public NativeWordAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
