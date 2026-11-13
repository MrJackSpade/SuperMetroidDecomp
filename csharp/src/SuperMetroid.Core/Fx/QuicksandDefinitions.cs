using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>Maridia quicksand's reaction objects and physical definition data.</summary>
public static class QuicksandDefinitions
{
    /// <summary>
    /// The identity of the room object each inside sand reaction places at its block (<see cref="QuicksandObjects"/>);
    /// every one leaves the room on its first update. No other inside reaction places a sand reaction.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpecialAirInsideReaction, RoomObjectKind> InsideObjects =
        new Dictionary<SpecialAirInsideReaction, RoomObjectKind>
        {
            [SpecialAirInsideReaction.QuicksandSurface] = RoomObjectKind.InsideReactionQuicksandSurface,
            [SpecialAirInsideReaction.SubmergingQuicksand] = RoomObjectKind.InsideReactionSubmergingQuicksand,
            [SpecialAirInsideReaction.SlowSandfall] = RoomObjectKind.InsideReactionSandFallsSlow,
            [SpecialAirInsideReaction.FastSandfall] = RoomObjectKind.InsideReactionSandFallsFast,
        };

    /// <summary>
    /// The identity of the room object each collision sand reaction places at its block (<see cref="QuicksandObjects"/>);
    /// every one leaves the room on its first update. No other collision reaction places a sand reaction.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpecialAirCollisionReaction, RoomObjectKind> CollisionObjects =
        new Dictionary<SpecialAirCollisionReaction, RoomObjectKind>
        {
            [SpecialAirCollisionReaction.QuicksandSurface] = RoomObjectKind.CollisionReactionQuicksandSurface,
            [SpecialAirCollisionReaction.SubmergingQuicksand] = RoomObjectKind.CollisionReactionSubmergingQuicksand,
            [SpecialAirCollisionReaction.SlowSandfall] = RoomObjectKind.CollisionReactionSandFallsSlow,
            [SpecialAirCollisionReaction.FastSandfall] = RoomObjectKind.CollisionReactionSandFallsFast,
        };

    /// <summary>Without Gravity: 2-pixel moving and 1.125-pixel stationary displacement, 2.5-pixel upward speed cap.</summary>
    internal static readonly QuicksandSurfacePhysics WithoutGravitySuit =
        new(SignedSixteenSixteen.FromSixteenthsOfPixel(32),
            SignedSixteenSixteen.FromSixteenthsOfPixel(18),
            SignedSixteenSixteen.FromSixteenthsOfPixel(40));

    /// <summary>With Gravity: 2-pixel moving and 1-pixel stationary displacement, 3.5-pixel upward speed cap.</summary>
    internal static readonly QuicksandSurfacePhysics WithGravitySuit =
        new(SignedSixteenSixteen.FromSixteenthsOfPixel(32),
            SignedSixteenSixteen.FromSixteenthsOfPixel(16),
            SignedSixteenSixteen.FromSixteenthsOfPixel(56));

    /// <summary>How far submerging quicksand pulls Samus down each frame she is inside it: 1.125 pixels.</summary>
    internal static readonly SignedSixteenSixteen SubmergingSinkDisplacement = SignedSixteenSixteen.FromSixteenthsOfPixel(18);

    /// <summary>How far a slow sandfall carries Samus down each frame she is inside it: 1.25 pixels.</summary>
    internal static readonly SignedSixteenSixteen SlowSandfallDisplacement = SignedSixteenSixteen.FromSixteenthsOfPixel(20);

    /// <summary>How far a fast sandfall carries Samus down each frame she is inside it: 1.75 pixels.</summary>
    internal static readonly SignedSixteenSixteen FastSandfallDisplacement = SignedSixteenSixteen.FromSixteenthsOfPixel(28);

    /// <summary>
    /// The most a stationary Samus's downward probe may sink into a quicksand surface: 0.1875 pixels. The surface
    /// compares and replaces only the probe's middle part (its whole pixels and the upper half of its fraction).
    /// </summary>
    internal static readonly SignedSixteenSixteen StationarySurfaceProbeLimit = SignedSixteenSixteen.FromSixteenthsOfPixel(3);
}
