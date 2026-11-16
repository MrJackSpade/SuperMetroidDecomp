namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The state of one fragment of Norfair Ridley's breakup.</summary>
internal sealed class RidleyExplosionFragmentState
{
    /// <summary>Signed 1/256-pixel velocity per update: X, which drag slows toward zero, and Y, which gravity increases.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The updates the fragment lives, counted down past zero.</summary>
    public int Lifetime { get; internal set; }

    /// <summary>
    /// The piece of Ridley the fragment shows, chosen when it spawns (#2235): a still picture, shown for good on the
    /// fragment's first animation update.
    /// </summary>
    internal required RidleyBreakupFrame Picture { get; init; }
}
