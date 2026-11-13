using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The special-air reaction each retail area gives each of its first <see cref="EntriesPerArea"/> area reactions: what the
/// block does while Samus is inside it, and what it does when her movement reaches it. Every entry an area leaves unused
/// does nothing; an area outside the retail game, a common reaction and an area reaction past the sixteenth are rejected.
/// These are the game's rules for its special-air blocks, not editable room or presentation assets.
/// </summary>
public static class SpecialAirReactions
{
    /// <summary>How many area reactions each retail area gives special-air blocks, in both kinds of reaction.</summary>
    public const int EntriesPerArea = 16;

    /// <summary>What the special-air block with area reaction <paramref name="reaction"/> does to Samus inside it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="reaction">The block's reaction, one of its area's first <see cref="EntriesPerArea"/> reactions.</param>
    public static SpecialAirInsideReaction InsideOf(AreaId area, BlockReaction reaction)
    {
        Validate(area, reaction, "inside-body");
        return new AreaTableEntry(area, reaction.Entry) switch
        {
            (AreaId.Crateria, 0) => SpecialAirInsideReaction.IcePhysics,
            (AreaId.Brinstar, 0) => SpecialAirInsideReaction.BrinstarFloorPlant,
            (AreaId.Brinstar, 1) => SpecialAirInsideReaction.BrinstarCeilingPlant,
            (AreaId.Maridia, 0) => SpecialAirInsideReaction.QuicksandSurface,
            (AreaId.Maridia, 1) => SpecialAirInsideReaction.QuicksandSurface,
            (AreaId.Maridia, 2) => SpecialAirInsideReaction.QuicksandSurface,
            (AreaId.Maridia, 3) => SpecialAirInsideReaction.SubmergingQuicksand,
            (AreaId.Maridia, 4) => SpecialAirInsideReaction.SlowSandfall,
            (AreaId.Maridia, 5) => SpecialAirInsideReaction.FastSandfall,
            _ => SpecialAirInsideReaction.Nothing,
        };
    }

    /// <summary>What the special-air block with area reaction <paramref name="reaction"/> does when Samus's movement reaches it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="reaction">The block's reaction, one of its area's first <see cref="EntriesPerArea"/> reactions.</param>
    public static SpecialAirCollisionReaction CollisionOf(AreaId area, BlockReaction reaction)
    {
        Validate(area, reaction, "movement-collision");
        return new AreaTableEntry(area, reaction.Entry) switch
        {
            (AreaId.Brinstar, 0) => SpecialAirCollisionReaction.PassesThrough,
            (AreaId.Brinstar, 1) => SpecialAirCollisionReaction.PassesThrough,
            (AreaId.Brinstar, 2) => SpecialAirCollisionReaction.BrinstarSlowRespawningSpeedBlock,
            (AreaId.Brinstar, 3) => SpecialAirCollisionReaction.BrinstarSlowPermanentSpeedBlock,
            (AreaId.Brinstar, 4) => SpecialAirCollisionReaction.DachoraRespawningSpeedBlock,
            (AreaId.Brinstar, 5) => SpecialAirCollisionReaction.PermanentSpeedBlock,
            (AreaId.Norfair, 3) => SpecialAirCollisionReaction.LowerNorfairChozoHand,
            (AreaId.WreckedShip, 0) => SpecialAirCollisionReaction.WreckedShipChozoHand,
            (AreaId.Maridia, 0) => SpecialAirCollisionReaction.QuicksandSurface,
            (AreaId.Maridia, 1) => SpecialAirCollisionReaction.QuicksandSurface,
            (AreaId.Maridia, 2) => SpecialAirCollisionReaction.QuicksandSurface,
            (AreaId.Maridia, 3) => SpecialAirCollisionReaction.SubmergingQuicksand,
            (AreaId.Maridia, 4) => SpecialAirCollisionReaction.SlowSandfall,
            (AreaId.Maridia, 5) => SpecialAirCollisionReaction.FastSandfall,
            _ => SpecialAirCollisionReaction.Nothing,
        };
    }

    /// <summary>Restricts the special-air reactions to the retail areas and their first sixteen area reactions.</summary>
    private static void Validate(AreaId area, BlockReaction reaction, string context)
    {
        _ = AreaIds.ToIndex(area);
        if (!reaction.IsAreaEntryBelow(EntriesPerArea))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reaction),
                reaction,
                $"Special-air {context} reactions are only an area's first sixteen reactions.");
        }
    }
}
