namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Authored displacement values for posture transition commands.</summary>
public static class SamusPostureDefinitions
{
    /// <summary>
    /// Word_91ED36 entries for poses $37/$38. Command seven
    /// requests nine pixels downward with the new radius, irrespective of the
    /// source radius, and clips that request through downward block collision.
    /// </summary>
    public const int MorphEntryDownwardPixels = 9;

    /// <summary>
    /// For poses $35/$36, and the literal five for aimed
    /// crouching transitions $F1-$F6. Command seven requests five pixels downward with the
    /// new radius 16 and clips that request through downward block collision.
    /// </summary>
    public const int CrouchEntryDownwardPixels = 5;
}
