namespace SuperMetroid.Core;

/// <summary>
/// Marks text that is an installed key (#2511, #2520): a name the import writes into the installation and the game looks
/// its content up by, such as a shade's effect, an artwork frame's name or a sheet's picture name, or a name an installed
/// document or a save spells an identity by. The parameter, the property or field, or the method's result is the key or a
/// part of one; a member overriding a marked one is marked with it. No enum member's name is written into it (SME6460);
/// each key is written out, as a literal or in a table of the identity's keys, so renaming an identity never changes a
/// name on disk.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method,
    Inherited = false)]
public sealed partial class InstalledKeyAttribute : Attribute;
