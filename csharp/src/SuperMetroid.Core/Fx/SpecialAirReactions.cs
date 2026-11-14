using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// What each special-air reaction does (#1742, #1805): what the block does while Samus is inside it, and what it does when
/// her movement reaches it. The import has already resolved what the room's area makes of the area's own reactions, so a
/// reaction means the same wherever it is found. A reaction that is the same in every area (passing through, a conveyor,
/// the scroll trigger) does nothing here; no reaction is rejected. These are the game's rules for its special-air blocks,
/// not editable room or presentation assets.
/// </summary>
public static class SpecialAirReactions
{
    /// <summary>What the special-air block with reaction <paramref name="reaction"/> does to Samus inside it.</summary>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block has no reaction.</exception>
    public static SpecialAirInsideReaction InsideOf(SpecialAirReaction? reaction) => Validate(reaction, "inside-body") switch
    {
        SpecialAirReaction.Ice => SpecialAirInsideReaction.IcePhysics,
        SpecialAirReaction.FloorPlant => SpecialAirInsideReaction.BrinstarFloorPlant,
        SpecialAirReaction.CeilingPlant => SpecialAirInsideReaction.BrinstarCeilingPlant,
        SpecialAirReaction.QuicksandSurface => SpecialAirInsideReaction.QuicksandSurface,
        SpecialAirReaction.SubmergingQuicksand => SpecialAirInsideReaction.SubmergingQuicksand,
        SpecialAirReaction.SlowSandfall => SpecialAirInsideReaction.SlowSandfall,
        SpecialAirReaction.FastSandfall => SpecialAirInsideReaction.FastSandfall,
        SpecialAirReaction.PassesThrough or SpecialAirReaction.RightwardConveyorOnFoot or SpecialAirReaction.LeftwardConveyorOnFoot or
            SpecialAirReaction.RightwardConveyor or SpecialAirReaction.LeftwardConveyor or SpecialAirReaction.ScrollTrigger or
            SpecialAirReaction.SlowRespawningSpeedBlock or SpecialAirReaction.SlowPermanentSpeedBlock or
            SpecialAirReaction.DachoraRespawningSpeedBlock or SpecialAirReaction.PermanentSpeedBlock or
            SpecialAirReaction.WreckedShipChozoHand or SpecialAirReaction.LowerNorfairChozoHand => SpecialAirInsideReaction.Nothing,
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SpecialAirReaction)} {undefined}."),
    };

    /// <summary>What the special-air block with reaction <paramref name="reaction"/> does when Samus's movement reaches it.</summary>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block has no reaction.</exception>
    public static SpecialAirCollisionReaction CollisionOf(SpecialAirReaction? reaction) => Validate(reaction, "movement-collision") switch
    {
        SpecialAirReaction.FloorPlant or SpecialAirReaction.CeilingPlant => SpecialAirCollisionReaction.PassesThrough,
        SpecialAirReaction.SlowRespawningSpeedBlock => SpecialAirCollisionReaction.BrinstarSlowRespawningSpeedBlock,
        SpecialAirReaction.SlowPermanentSpeedBlock => SpecialAirCollisionReaction.BrinstarSlowPermanentSpeedBlock,
        SpecialAirReaction.DachoraRespawningSpeedBlock => SpecialAirCollisionReaction.DachoraRespawningSpeedBlock,
        SpecialAirReaction.PermanentSpeedBlock => SpecialAirCollisionReaction.PermanentSpeedBlock,
        SpecialAirReaction.LowerNorfairChozoHand => SpecialAirCollisionReaction.LowerNorfairChozoHand,
        SpecialAirReaction.WreckedShipChozoHand => SpecialAirCollisionReaction.WreckedShipChozoHand,
        SpecialAirReaction.QuicksandSurface => SpecialAirCollisionReaction.QuicksandSurface,
        SpecialAirReaction.SubmergingQuicksand => SpecialAirCollisionReaction.SubmergingQuicksand,
        SpecialAirReaction.SlowSandfall => SpecialAirCollisionReaction.SlowSandfall,
        SpecialAirReaction.FastSandfall => SpecialAirCollisionReaction.FastSandfall,
        SpecialAirReaction.PassesThrough or SpecialAirReaction.RightwardConveyorOnFoot or SpecialAirReaction.LeftwardConveyorOnFoot or
            SpecialAirReaction.RightwardConveyor or SpecialAirReaction.LeftwardConveyor or SpecialAirReaction.ScrollTrigger or
            SpecialAirReaction.Ice => SpecialAirCollisionReaction.Nothing,
        var undefined => throw new InvalidOperationException($"Undefined {nameof(SpecialAirReaction)} {undefined}."),
    };

    /// <summary>Requires the block to have a reaction.</summary>
    /// <returns>The block's reaction.</returns>
    private static SpecialAirReaction Validate(SpecialAirReaction? reaction, string context) =>
        reaction ?? throw new ArgumentOutOfRangeException(
            nameof(reaction),
            reaction,
            $"A special-air block's {context} reaction needs a reaction the block has.");
}
