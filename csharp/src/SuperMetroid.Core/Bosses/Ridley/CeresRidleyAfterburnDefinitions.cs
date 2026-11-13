namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Launch velocities of the directional afterburns a Ridley fireball's impact centre spawns
/// (two horizontal and two vertical): each
/// moves fourteen pixels per frame along one axis and keeps it for its continuation chain.
/// </summary>
internal static class CeresRidleyAfterburnDefinitions
{
    /// <summary>The speed of every directional afterburn, fourteen pixels per frame.</summary>
    private static readonly SignedEightEight Speed = SignedEightEight.FromPixels(14);

    /// <summary>The rightward horizontal afterburn's velocity.</summary>
    internal static EightEightVector<RoomPixels> Right => new(Speed, SignedEightEight.Zero);

    /// <summary>The leftward horizontal afterburn's velocity.</summary>
    internal static EightEightVector<RoomPixels> Left => new(-Speed, SignedEightEight.Zero);

    /// <summary>The upward vertical afterburn's velocity.</summary>
    internal static EightEightVector<RoomPixels> Up => new(SignedEightEight.Zero, -Speed);

    /// <summary>The downward vertical afterburn's velocity.</summary>
    internal static EightEightVector<RoomPixels> Down => new(SignedEightEight.Zero, Speed);
}
