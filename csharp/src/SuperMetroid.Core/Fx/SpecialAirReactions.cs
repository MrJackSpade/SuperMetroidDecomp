using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// What each retail area makes of each special-air reaction (#1742): what the block does while Samus is inside it, and
/// what it does when her movement reaches it. A reaction whose meaning does not depend on the area (passing through, a
/// conveyor, the scroll trigger), and every reaction an area leaves unused, does nothing here; an area outside the retail
/// game and no reaction are rejected. These are the game's rules for its special-air blocks, not editable room or
/// presentation assets.
/// </summary>
public static class SpecialAirReactions
{
    /// <summary>What the special-air block with reaction <paramref name="reaction"/> does to Samus inside it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not a retail area, or the block has no reaction.</exception>
    public static SpecialAirInsideReaction InsideOf(AreaId area, SpecialAirReaction? reaction)
    {
        SpecialAirReaction read = Validate(area, reaction, "inside-body");
        return new AreaReaction(area, read) switch
        {
            (AreaId.Crateria, SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface) => SpecialAirInsideReaction.IcePhysics,
            (AreaId.Brinstar, SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface) => SpecialAirInsideReaction.BrinstarFloorPlant,
            (AreaId.Brinstar, SpecialAirReaction.CeilingPlantOrQuicksandSurface) => SpecialAirInsideReaction.BrinstarCeilingPlant,
            (AreaId.Maridia, SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface) => SpecialAirInsideReaction.QuicksandSurface,
            (AreaId.Maridia, SpecialAirReaction.CeilingPlantOrQuicksandSurface) => SpecialAirInsideReaction.QuicksandSurface,
            (AreaId.Maridia, SpecialAirReaction.SlowRespawningSpeedBlockOrQuicksandSurface) => SpecialAirInsideReaction.QuicksandSurface,
            (AreaId.Maridia, SpecialAirReaction.SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand) => SpecialAirInsideReaction.SubmergingQuicksand,
            (AreaId.Maridia, SpecialAirReaction.RespawningSpeedBlockOrSlowSandfall) => SpecialAirInsideReaction.SlowSandfall,
            (AreaId.Maridia, SpecialAirReaction.PermanentSpeedBlockOrFastSandfall) => SpecialAirInsideReaction.FastSandfall,
            _ => SpecialAirInsideReaction.Nothing,
        };
    }

    /// <summary>What the special-air block with reaction <paramref name="reaction"/> does when Samus's movement reaches it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not a retail area, or the block has no reaction.</exception>
    public static SpecialAirCollisionReaction CollisionOf(AreaId area, SpecialAirReaction? reaction)
    {
        SpecialAirReaction read = Validate(area, reaction, "movement-collision");
        return new AreaReaction(area, read) switch
        {
            (AreaId.Brinstar, SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface) => SpecialAirCollisionReaction.PassesThrough,
            (AreaId.Brinstar, SpecialAirReaction.CeilingPlantOrQuicksandSurface) => SpecialAirCollisionReaction.PassesThrough,
            (AreaId.Brinstar, SpecialAirReaction.SlowRespawningSpeedBlockOrQuicksandSurface) => SpecialAirCollisionReaction.BrinstarSlowRespawningSpeedBlock,
            (AreaId.Brinstar, SpecialAirReaction.SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand) => SpecialAirCollisionReaction.BrinstarSlowPermanentSpeedBlock,
            (AreaId.Brinstar, SpecialAirReaction.RespawningSpeedBlockOrSlowSandfall) => SpecialAirCollisionReaction.DachoraRespawningSpeedBlock,
            (AreaId.Brinstar, SpecialAirReaction.PermanentSpeedBlockOrFastSandfall) => SpecialAirCollisionReaction.PermanentSpeedBlock,
            (AreaId.Norfair, SpecialAirReaction.SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand) => SpecialAirCollisionReaction.LowerNorfairChozoHand,
            (AreaId.WreckedShip, SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface) => SpecialAirCollisionReaction.WreckedShipChozoHand,
            (AreaId.Maridia, SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface) => SpecialAirCollisionReaction.QuicksandSurface,
            (AreaId.Maridia, SpecialAirReaction.CeilingPlantOrQuicksandSurface) => SpecialAirCollisionReaction.QuicksandSurface,
            (AreaId.Maridia, SpecialAirReaction.SlowRespawningSpeedBlockOrQuicksandSurface) => SpecialAirCollisionReaction.QuicksandSurface,
            (AreaId.Maridia, SpecialAirReaction.SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand) => SpecialAirCollisionReaction.SubmergingQuicksand,
            (AreaId.Maridia, SpecialAirReaction.RespawningSpeedBlockOrSlowSandfall) => SpecialAirCollisionReaction.SlowSandfall,
            (AreaId.Maridia, SpecialAirReaction.PermanentSpeedBlockOrFastSandfall) => SpecialAirCollisionReaction.FastSandfall,
            _ => SpecialAirCollisionReaction.Nothing,
        };
    }

    /// <summary>An area and a special-air reaction, which the area gives its meaning.</summary>
    /// <param name="Area">The room's area.</param>
    /// <param name="Reaction">The block's reaction.</param>
    private readonly record struct AreaReaction(AreaId Area, SpecialAirReaction Reaction);

    /// <summary>Restricts the special-air reactions to the retail areas and the blocks that have a reaction.</summary>
    /// <returns>The block's reaction.</returns>
    private static SpecialAirReaction Validate(AreaId area, SpecialAirReaction? reaction, string context)
    {
        _ = AreaIds.Require(area);
        return reaction ?? throw new ArgumentOutOfRangeException(
            nameof(reaction),
            reaction,
            $"A special-air block's {context} reaction needs a reaction the block has.");
    }
}
