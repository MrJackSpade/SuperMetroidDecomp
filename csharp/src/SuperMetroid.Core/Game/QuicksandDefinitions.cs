using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>One suit-dependent row of the fixed quicksand-surface physics table.</summary>
public readonly record struct QuicksandSurfacePhysics(
    ushort MovingDisplacement,
    ushort StationaryDisplacement,
    ushort UpwardSpeedLimit);

/// <summary>One quicksand PLM header paired with its setup routine and initial list.</summary>
public readonly record struct QuicksandReactionDefinition(
    ushort HeaderPointer,
    ushort SetupPointer,
    ushort InstructionListPointer);

/// <summary>Compiled Maridia quicksand dispatch and physical definition data.</summary>
public static class QuicksandDefinitions
{
    /// <summary>
    /// `$84:B48B-$84:B495`, suit-indexed moving displacement, stationary
    /// displacement, and upward-speed-limit words used by the surface reaction.
    /// Gravity equipment selects one of two named scalar cases. Words are unsigned 8.8:
    /// airborne displacement is 2 for either suit, grounded displacement is 1.125/1,
    /// and the NTSC upward cap is 2.5/3.5 without/with Gravity. Consumers shift by eight
    /// for 16.16 displacement and compare the middle word of upward velocity.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? withGravitySuit : withoutGravitySuit;

    /// <summary>
    /// Named setup dispatch for the eight supported sand headers at $84:B713..B73F.
    /// All select the shared delete list; unsupported pointers return false/default,
    /// including the unused surface clones between the named headers. No table is stored.
    /// </summary>
    public static bool TryGetReaction(
        ushort header,
        out QuicksandReactionDefinition definition)
    {
        // Native headers select behavior; the gaps contain unused clones outside this API.
        ushort setup = header switch
        {
            QuicksandRomData.SurfaceInsideHeader => QuicksandRomData.SurfaceSetup,
            QuicksandRomData.SubmergingInsideHeader => QuicksandRomData.SubmergingSetup,
            QuicksandRomData.SlowFallsInsideHeader => QuicksandRomData.SlowFallsSetup,
            QuicksandRomData.FastFallsInsideHeader => QuicksandRomData.FastFallsSetup,
            QuicksandRomData.SurfaceCollisionHeader => QuicksandRomData.SurfaceCollision,
            QuicksandRomData.SubmergingCollisionHeader => QuicksandRomData.SubmergingCollision,
            QuicksandRomData.SlowFallsCollisionHeader or QuicksandRomData.FastFallsCollisionHeader =>
                QuicksandRomData.SandFallsCollision,
            _ => 0,
        };
        definition = setup == 0 ? default : new(header, setup, RoomPlmInstructionLists.Delete);
        return setup != 0;
    }

    /// <summary>Resolves one supported sand header or rejects an invalid allocator request.</summary>
    public static QuicksandReactionDefinition ResolveReaction(ushort header) =>
        TryGetReaction(header, out QuicksandReactionDefinition definition)
            ? definition
            : throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Quicksand reaction header must be one of the eight bank-$84 sand entries.");

    private static readonly QuicksandSurfacePhysics withoutGravitySuit =
        new(0x0200, 0x0120, 0x0280);

    private static readonly QuicksandSurfacePhysics withGravitySuit =
        new(0x0200, 0x0100, 0x0380);
}
