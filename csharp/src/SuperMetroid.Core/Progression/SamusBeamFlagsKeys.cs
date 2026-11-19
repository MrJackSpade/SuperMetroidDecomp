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

    /// <summary>Each SamusBeamFlags flag on its own in the order a save and an installed document list a set of them (#2567), written out beside the keys so reordering the members never changes the bytes written; the empty set is no flag.</summary>
    internal static IReadOnlyList<SamusBeamFlags> ListedOrder { get; } =
    [
        SamusBeamFlags.Wave,
        SamusBeamFlags.Ice,
        SamusBeamFlags.Spazer,
        SamusBeamFlags.Plasma,
        SamusBeamFlags.Charge,
    ];
}
