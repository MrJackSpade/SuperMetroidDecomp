namespace SuperMetroid.Core.Game;

/// <summary>Cartridge-authored displacement values for posture transition commands.</summary>
public static class SamusPostureDefinitions
{
    /// <summary>
    /// $91:ED3A/$ED3C, word_91ED36 entries for poses $37/$38. Command seven
    /// requests nine pixels downward with the new radius, irrespective of the
    /// source radius, and clips that request through $94:96AB block collision.
    /// </summary>
    public const int MorphEntryDownwardPixels = 9;
}
