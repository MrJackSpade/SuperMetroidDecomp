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
    /// <summary>Complete eight-header sand-reaction domain from bank $84.</summary>
    public static ReadOnlySpan<QuicksandReactionDefinition> Reactions => reactions;

    /// <summary>
    /// `$84:B48B-$84:B495`, suit-indexed moving displacement, stationary
    /// displacement, and upward-speed-limit words used by the surface reaction.
    /// </summary>
    public static QuicksandSurfacePhysics SurfacePhysics(bool gravitySuit) =>
        gravitySuit ? withGravitySuit : withoutGravitySuit;

    /// <summary>Resolves one of the eight authored sand headers without reading executable metadata.</summary>
    public static bool TryGetReaction(
        ushort header,
        out QuicksandReactionDefinition definition)
    {
        foreach (QuicksandReactionDefinition candidate in reactions)
        {
            if (candidate.HeaderPointer != header)
                continue;
            definition = candidate;
            return true;
        }

        definition = default;
        return false;
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

    private static readonly QuicksandReactionDefinition[] reactions =
    [
        new(QuicksandRomData.SurfaceInsideHeader, QuicksandRomData.SurfaceSetup,
            RoomPlmInstructionLists.Delete),
        new(QuicksandRomData.SubmergingInsideHeader, QuicksandRomData.SubmergingSetup,
            RoomPlmInstructionLists.Delete),
        new(QuicksandRomData.SlowFallsInsideHeader, QuicksandRomData.SlowFallsSetup,
            RoomPlmInstructionLists.Delete),
        new(QuicksandRomData.FastFallsInsideHeader, QuicksandRomData.FastFallsSetup,
            RoomPlmInstructionLists.Delete),
        new(QuicksandRomData.SurfaceCollisionHeader, QuicksandRomData.SurfaceCollision,
            RoomPlmInstructionLists.Delete),
        new(QuicksandRomData.SubmergingCollisionHeader, QuicksandRomData.SubmergingCollision,
            RoomPlmInstructionLists.Delete),
        new(QuicksandRomData.SlowFallsCollisionHeader, QuicksandRomData.SandFallsCollision,
            RoomPlmInstructionLists.Delete),
        new(QuicksandRomData.FastFallsCollisionHeader, QuicksandRomData.SandFallsCollision,
            RoomPlmInstructionLists.Delete),
    ];
}
