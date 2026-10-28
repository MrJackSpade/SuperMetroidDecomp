using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>Compiled Maridia quicksand dispatch and physical definition data.</summary>
public static class QuicksandDefinitions
{
    /// <summary>
    /// Named setup dispatch for the eight supported sand headers at $84:B713..B73F. All select the shared delete list
    /// (<see cref="QuicksandReactions"/>); no other header, including the unused surface clones between the named
    /// headers, is a sand reaction.
    /// </summary>
    internal static readonly IReadOnlyDictionary<PlmHeaderId, SpecialAirReactionSetup> Reactions =
        new Dictionary<PlmHeaderId, SpecialAirReactionSetup>
        {
            [PlmHeaderId.InsideReactionQuicksandSurface] = SpecialAirReactionSetup.QuicksandSurface,
            [PlmHeaderId.InsideReactionSubmergingQuicksand] = SpecialAirReactionSetup.SubmergingQuicksand,
            [PlmHeaderId.InsideReactionSandFallsSlow] = SpecialAirReactionSetup.SandFallsSlow,
            [PlmHeaderId.InsideReactionSandFallsFast] = SpecialAirReactionSetup.SandFallsFast,
            [PlmHeaderId.CollisionReactionQuicksandSurface] = SpecialAirReactionSetup.QuicksandSurfaceCollision,
            [PlmHeaderId.CollisionReactionSubmergingQuicksand] = SpecialAirReactionSetup.SubmergingQuicksandCollision,
            [PlmHeaderId.CollisionReactionSandFallsSlow] = SpecialAirReactionSetup.SandFallsCollision,
            [PlmHeaderId.CollisionReactionSandFallsFast] = SpecialAirReactionSetup.SandFallsCollision,
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
