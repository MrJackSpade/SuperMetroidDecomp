namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed key of each <see cref="PauseWireframeKind"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class PauseWireframeKindKeys
{
    /// <summary>Each PauseWireframeKind member's installed key.</summary>
    internal static InstalledKeys<PauseWireframeKind> Installed { get; } = new(new Dictionary<PauseWireframeKind, string>
    {
        [PauseWireframeKind.PowerSuit] = "PowerSuit",
        [PauseWireframeKind.PowerSuitHiJump] = "PowerSuitHiJump",
        [PauseWireframeKind.VariaSuit] = "VariaSuit",
        [PauseWireframeKind.VariaSuitHiJump] = "VariaSuitHiJump",
    });
}
