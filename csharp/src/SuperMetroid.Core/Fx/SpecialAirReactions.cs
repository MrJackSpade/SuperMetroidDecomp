using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Fx.SpecialAirReactionDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Resolves the special-air reaction a retail area's BTS index selects (<see cref="SpecialAirReactionDefinitions"/>), for a body
/// inside the block and for a movement collision with it.
/// </summary>
public static class SpecialAirReactions
{
    /// <summary>Resolves the native inside-body header/setup for retail areas and indexes 0..15.</summary>
    public static SpecialAirReactionDefinition ResolveInside(AreaId area, int areaReactionIndex)
    {
        Validate(area, areaReactionIndex, "inside-body");
        return new AreaTableEntry(area, areaReactionIndex) switch
        {
            (AreaId.Crateria, 0) => CrateriaIcePhysics,
            (AreaId.Brinstar, 0) => BrinstarFloorPlant,
            (AreaId.Brinstar, 1) => BrinstarCeilingPlant,
            (AreaId.Norfair, 0) => NorfairInsideNothing80,
            (AreaId.Norfair, 1) => NorfairInsideNothing81,
            (AreaId.Norfair, 2) => NorfairInsideNothing82,
            (AreaId.Maridia, 0) => QuicksandSurfaceInside,
            (AreaId.Maridia, 1) => QuicksandSurfaceInside,
            (AreaId.Maridia, 2) => QuicksandSurfaceInside,
            (AreaId.Maridia, 3) => QuicksandSubmergingInside,
            (AreaId.Maridia, 4) => QuicksandSlowFallInside,
            (AreaId.Maridia, 5) => QuicksandFastFallInside,
            _ => Nothing,
        };
    }

    /// <summary>Resolves the native movement-collision header/setup for retail areas and indexes 0..15.</summary>
    public static SpecialAirReactionDefinition ResolveCollision(AreaId area, int areaReactionIndex)
    {
        Validate(area, areaReactionIndex, "movement-collision");
        return new AreaTableEntry(area, areaReactionIndex) switch
        {
            (AreaId.Brinstar, 0) => ClearCarry,
            (AreaId.Brinstar, 1) => ClearCarry,
            (AreaId.Brinstar, 2) => BrinstarSlowSpeedBlockRespawning,
            (AreaId.Brinstar, 3) => BrinstarSlowSpeedBlockPermanent,
            (AreaId.Brinstar, 4) => BrinstarDachoraSpeedBlock,
            (AreaId.Brinstar, 5) => BrinstarSpeedBlockPermanent,
            (AreaId.Norfair, 3) => LowerNorfairChozoHand,
            (AreaId.WreckedShip, 0) => WreckedShipChozoHand,
            (AreaId.Maridia, 0) => QuicksandSurfaceCollision,
            (AreaId.Maridia, 1) => QuicksandSurfaceCollision,
            (AreaId.Maridia, 2) => QuicksandSurfaceCollision,
            (AreaId.Maridia, 3) => QuicksandSubmergingCollision,
            (AreaId.Maridia, 4) => QuicksandSlowFallCollision,
            (AreaId.Maridia, 5) => QuicksandFastFallCollision,
            _ => Nothing,
        };
    }

    /// <summary>Restricts special-air lookup to retail areas and their sixteen authored BTS entries.</summary>
    private static void Validate(AreaId area, int areaReactionIndex, string context)
    {
        _ = AreaIds.ToIndex(area);
        if (areaReactionIndex >= EntriesPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(areaReactionIndex),
                areaReactionIndex,
                $"Special-air {context} BTS indexes are authored only from zero through fifteen.");
        }
    }
}
