namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Fixed health-band policy for Botwoon's palette handler. The eight authored color images are independently
/// extractable visual assets; where they sit in the cartridge is the import's knowledge (#1413).
/// </summary>
internal static class BotwoonHealthPaletteDefinitions
{
    /// <summary>Eight complete sixteen-color palette images, one per health band.</summary>
    public const int PaletteCount = 8;
    /// <summary>Each Botwoon health-band image occupies one sixteen-color sprite palette.</summary>
    public const int ColorsPerPalette = 16;

    /// <summary>Terminal native byte offset after all eight threshold words are consumed.</summary>
    public const int CompletePhase = 8;

    /// <summary>The health between successive thresholds: threshold(i) = 375 * (8 - i).</summary>
    public const int HealthPerThreshold = 375;
}
