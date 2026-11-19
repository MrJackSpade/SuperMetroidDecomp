namespace SuperMetroid.Core;

/// <summary>
/// Marks text that is an installed key (#2511): a name the import writes into the installation and the game looks its
/// content up by, such as a shade's effect or an artwork frame's name. The parameter, the property or field, or the method's
/// result is the key or a part of one. No enum member's name is written into it (SME6460); each key is written out, as
/// a literal or in a table of the identity's keys, so renaming an identity never changes a name on disk.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method,
    Inherited = false)]
public sealed class InstalledKeyAttribute : Attribute;
