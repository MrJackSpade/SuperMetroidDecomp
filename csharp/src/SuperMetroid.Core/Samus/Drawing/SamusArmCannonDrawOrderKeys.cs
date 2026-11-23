namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The installed key of each <see cref="SamusArmCannonDrawOrder"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SamusArmCannonDrawOrderKeys
{
    /// <summary>Each SamusArmCannonDrawOrder member's installed key.</summary>
    internal static InstalledKeys<SamusArmCannonDrawOrder> Installed { get; } = new(new Dictionary<SamusArmCannonDrawOrder, string>
    {
        [SamusArmCannonDrawOrder.Hidden] = "Hidden",
        [SamusArmCannonDrawOrder.BeforeBody] = "BeforeBody",
        [SamusArmCannonDrawOrder.AfterBody] = "AfterBody",
    });
}
