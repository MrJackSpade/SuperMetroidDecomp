namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one declaration, an integer field or property that packs several fields into one value whose bit
/// fields a typed view decodes, rather than one coded category (SME6421, SME6422, #1420). Its zero test asks whether
/// the value holds anything at all. The reason names the value and its typed view.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class PackedFieldsAttribute : Attribute
{
    /// <summary>Marks a packed value; SME6421 and SME6422 read the reason from the attribute argument.</summary>
    /// <param name="reason">The packed value and the typed view that decodes it.</param>
    public PackedFieldsAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
