using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>Maridia quicksand's reaction objects and physical definition data.</summary>
public static class QuicksandDefinitions
{
    /// <summary>
    /// The identity of the room object each inside sand reaction places at its block (<see cref="QuicksandObjects"/>);
    /// every one leaves the room on its first update. No other inside reaction places a sand reaction.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpecialAirInsideReaction, RoomObjectHeaderId> InsideObjects =
        new Dictionary<SpecialAirInsideReaction, RoomObjectHeaderId>
        {
            [SpecialAirInsideReaction.QuicksandSurface] = RoomObjectHeaderId.InsideReactionQuicksandSurface,
            [SpecialAirInsideReaction.SubmergingQuicksand] = RoomObjectHeaderId.InsideReactionSubmergingQuicksand,
            [SpecialAirInsideReaction.SlowSandfall] = RoomObjectHeaderId.InsideReactionSandFallsSlow,
            [SpecialAirInsideReaction.FastSandfall] = RoomObjectHeaderId.InsideReactionSandFallsFast,
        };

    /// <summary>
    /// The identity of the room object each collision sand reaction places at its block (<see cref="QuicksandObjects"/>);
    /// every one leaves the room on its first update. No other collision reaction places a sand reaction.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpecialAirCollisionReaction, RoomObjectHeaderId> CollisionObjects =
        new Dictionary<SpecialAirCollisionReaction, RoomObjectHeaderId>
        {
            [SpecialAirCollisionReaction.QuicksandSurface] = RoomObjectHeaderId.CollisionReactionQuicksandSurface,
            [SpecialAirCollisionReaction.SubmergingQuicksand] = RoomObjectHeaderId.CollisionReactionSubmergingQuicksand,
            [SpecialAirCollisionReaction.SlowSandfall] = RoomObjectHeaderId.CollisionReactionSandFallsSlow,
            [SpecialAirCollisionReaction.FastSandfall] = RoomObjectHeaderId.CollisionReactionSandFallsFast,
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
    /// compares and replaces only the probe's middle word (whole and upper fraction byte).
    /// </summary>
    internal static readonly SignedSixteenSixteen StationarySurfaceProbeLimit = SignedSixteenSixteen.FromSixteenthsOfPixel(3);
}
