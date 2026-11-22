namespace SuperMetroid.Core.Hud;

/// <summary>
/// The installed key of each <see cref="HudReserveIndicator"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class HudReserveIndicatorKeys
{
    /// <summary>Each HudReserveIndicator member's installed key.</summary>
    internal static InstalledKeys<HudReserveIndicator> Installed { get; } = new(new Dictionary<HudReserveIndicator, string>
    {
        [HudReserveIndicator.Filled] = "Filled",
        [HudReserveIndicator.Empty] = "Empty",
        [HudReserveIndicator.Cleared] = "Cleared",
    });
}
