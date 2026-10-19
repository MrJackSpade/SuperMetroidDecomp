using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>Compiled Maridia quicksand dispatch and physical definition data.</summary>
public static class QuicksandDefinitions
{
    /// <summary>
    /// `$84:B48B-$84:B495`, suit-indexed moving displacement, stationary
    /// displacement, and upward-speed-limit values used by the surface reaction.
    /// Gravity equipment selects one of two named scalar cases. Moving displacement is
    /// 2 pixels per frame for either suit, stationary displacement is 1.125/1, and the
    /// NTSC upward cap is 2.5/3.5 pixels per frame without/with Gravity. The cartridge
    /// stores them as unsigned 8.8 words; they are held here as the 16.16 values its
    /// consumers widen them to.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? withGravitySuit : withoutGravitySuit;

    /// <summary>
    /// Named setup dispatch for the eight supported sand headers at $84:B713..B73F.
    /// All select the shared delete list; unsupported pointers return false/default,
    /// including the unused surface clones between the named headers. No table is stored.
    /// </summary>
    internal static bool TryGetReaction(
        PlmHeaderId header,
        out QuicksandReactionDefinition definition)
    {
        // Native headers select behavior; the gaps contain unused clones outside this API.
        if (header is not (PlmHeaderId.InsideReactionQuicksandSurface or PlmHeaderId.InsideReactionSubmergingQuicksand or
            PlmHeaderId.InsideReactionSandFallsSlow or PlmHeaderId.InsideReactionSandFallsFast or
            PlmHeaderId.CollisionReactionQuicksandSurface or PlmHeaderId.CollisionReactionSubmergingQuicksand or
            PlmHeaderId.CollisionReactionSandFallsSlow or PlmHeaderId.CollisionReactionSandFallsFast))
        {
            definition = default;
            return false;
        }
        SpecialAirReactionSetup setup = header switch
        {
            PlmHeaderId.InsideReactionQuicksandSurface => SpecialAirReactionSetup.QuicksandSurface,
            PlmHeaderId.InsideReactionSubmergingQuicksand => SpecialAirReactionSetup.SubmergingQuicksand,
            PlmHeaderId.InsideReactionSandFallsSlow => SpecialAirReactionSetup.SandFallsSlow,
            PlmHeaderId.InsideReactionSandFallsFast => SpecialAirReactionSetup.SandFallsFast,
            PlmHeaderId.CollisionReactionQuicksandSurface => SpecialAirReactionSetup.QuicksandSurfaceCollision,
            PlmHeaderId.CollisionReactionSubmergingQuicksand => SpecialAirReactionSetup.SubmergingQuicksandCollision,
            PlmHeaderId.CollisionReactionSandFallsSlow or PlmHeaderId.CollisionReactionSandFallsFast =>
                SpecialAirReactionSetup.SandFallsCollision,
            _ => throw new InvalidOperationException($"Undefined quicksand header {header}."),
        };
        definition = new(setup, PlmProgramStart.Delete);
        return true;
    }

    /// <summary>Resolves one supported sand header or rejects an invalid allocator request.</summary>
    internal static QuicksandReactionDefinition ResolveReaction(PlmHeaderId header) =>
        TryGetReaction(header, out QuicksandReactionDefinition definition)
            ? definition
            : throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Quicksand reaction header must be one of the eight bank-$84 sand entries.");

    /// <summary>Without Gravity: 2-pixel moving and 1.125-pixel stationary displacement, 2.5-pixel upward speed cap (fractions in 1/65536 pixel).</summary>
    private static readonly QuicksandSurfacePhysics withoutGravitySuit =
        new(SnesSignedSixteenSixteen.FromParts(wholePixels: 2, fraction: 0),
            SnesSignedSixteenSixteen.FromParts(wholePixels: 1, fraction: 8192),
            SnesSignedSixteenSixteen.FromParts(wholePixels: 2, fraction: 32768));

    /// <summary>With Gravity: 2-pixel moving and 1-pixel stationary displacement, 3.5-pixel upward speed cap (fractions in 1/65536 pixel).</summary>
    private static readonly QuicksandSurfacePhysics withGravitySuit =
        new(SnesSignedSixteenSixteen.FromParts(wholePixels: 2, fraction: 0),
            SnesSignedSixteenSixteen.FromParts(wholePixels: 1, fraction: 0),
            SnesSignedSixteenSixteen.FromParts(wholePixels: 3, fraction: 32768));
}
