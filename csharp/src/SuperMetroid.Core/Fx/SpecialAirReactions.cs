using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The special-air reaction each retail area gives each of the area reactions of special air: what the block does while
/// Samus is inside it, and what it does when her movement reaches it. Every area reaction an area leaves unused does
/// nothing; an area outside the retail game, a common reaction and an area reaction past the areas' tables are rejected.
/// These are the game's rules for its special-air blocks, not editable room or presentation assets.
/// </summary>
public static class SpecialAirReactions
{
    /// <summary>What the special-air block with area reaction <paramref name="reaction"/> does to Samus inside it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="reaction">The block's reaction, one of the area reactions within the areas' tables.</param>
    public static SpecialAirInsideReaction InsideOf(AreaId area, SpecialAirReaction reaction)
    {
        Validate(area, reaction, "inside-body");
        return new AreaReaction(area, reaction) switch
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

    /// <summary>What the special-air block with area reaction <paramref name="reaction"/> does when Samus's movement reaches it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="reaction">The block's reaction, one of the area reactions within the areas' tables.</param>
    public static SpecialAirCollisionReaction CollisionOf(AreaId area, SpecialAirReaction reaction)
    {
        Validate(area, reaction, "movement-collision");
        return new AreaReaction(area, reaction) switch
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

    /// <summary>Whether <paramref name="reaction"/> is one of the area reactions within the areas' tables, which the area tables read.</summary>
    public static bool IsAreaTableReaction(SpecialAirReaction reaction) => reaction is
        SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface or SpecialAirReaction.CeilingPlantOrQuicksandSurface or
        SpecialAirReaction.SlowRespawningSpeedBlockOrQuicksandSurface or
        SpecialAirReaction.SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand or
        SpecialAirReaction.RespawningSpeedBlockOrSlowSandfall or SpecialAirReaction.PermanentSpeedBlockOrFastSandfall or
        SpecialAirReaction.AreaNothing;

    /// <summary>Whether <paramref name="reaction"/> is one the room's area gives special air, within its tables or past them.</summary>
    public static bool IsArea(SpecialAirReaction reaction) =>
        IsAreaTableReaction(reaction) || reaction == SpecialAirReaction.PastAreaTable;

    /// <summary>An area and one of the area reactions its special-air table gives meaning to.</summary>
    /// <param name="Area">The room's area.</param>
    /// <param name="Reaction">The block's area reaction.</param>
    private readonly record struct AreaReaction(AreaId Area, SpecialAirReaction Reaction);

    /// <summary>Restricts the special-air reactions to the retail areas and the area reactions within their tables.</summary>
    private static void Validate(AreaId area, SpecialAirReaction reaction, string context)
    {
        _ = AreaIds.Require(area);
        if (!IsAreaTableReaction(reaction))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reaction),
                reaction,
                $"Special-air {context} reactions are only an area's first sixteen reactions.");
        }
    }
}
