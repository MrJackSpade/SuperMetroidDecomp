namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One group of four body-relative positions Mother Brain's death explosions burst at (#2315): a mixed burst explodes at
/// all four, a smoky burst at the first two.
/// </summary>
/// <param name="First">The first position.</param>
/// <param name="Second">The second position.</param>
/// <param name="Third">The third position.</param>
/// <param name="Fourth">The fourth position.</param>
internal sealed record MotherBrainDeathExplosionGroup(
    Offset16<RoomPixels> First,
    Offset16<RoomPixels> Second,
    Offset16<RoomPixels> Third,
    Offset16<RoomPixels> Fourth)
{
    /// <summary>The positions a smoky burst explodes at, in the order it spawns them.</summary>
    internal IReadOnlyList<Offset16<RoomPixels>> Smoky => [First, Second];

    /// <summary>The positions a mixed burst explodes at, in the order it spawns them.</summary>
    internal IReadOnlyList<Offset16<RoomPixels>> Mixed => [First, Second, Third, Fourth];
}
