namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The timing of the Crystal Flash body's colour pulse: how many steps it holds and how long each shows. Which colours
/// each step shows is the import's (#1512).
/// </summary>
internal static class CrystalFlashPaletteTimingDefinitions
{
    /// <summary>The steps of the body's pulse, after which it returns to its first.</summary>
    public const int RecordCount = 10;

    /// <summary>Every step of the body's pulse shows for ten colouring updates.</summary>
    public const int AuthoredDuration = 10;
}
