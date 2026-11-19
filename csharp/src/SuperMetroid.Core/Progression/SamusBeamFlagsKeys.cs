namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="SamusBeamFlags"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SamusBeamFlagsKeys
{
    /// <summary>Each SamusBeamFlags member's installed key.</summary>
    internal static InstalledKeys<SamusBeamFlags> Installed { get; } = new(new Dictionary<SamusBeamFlags, string>
    {
        [SamusBeamFlags.None] = "None",
        [SamusBeamFlags.Wave] = "Wave",
        [SamusBeamFlags.Ice] = "Ice",
        [SamusBeamFlags.Spazer] = "Spazer",
        [SamusBeamFlags.Plasma] = "Plasma",
        [SamusBeamFlags.Charge] = "Charge",
    });
}
