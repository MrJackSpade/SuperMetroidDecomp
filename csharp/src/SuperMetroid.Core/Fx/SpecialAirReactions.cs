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
    /// <summary>What each special-air reaction does to Samus inside the block and when her movement reaches it.</summary>
    private static readonly Dictionary<SpecialAirReaction, (SpecialAirInsideReaction Inside, SpecialAirCollisionReaction Collision)> Reactions = new()
    {
        [SpecialAirReaction.FloorPlant] = new(SpecialAirInsideReaction.BrinstarFloorPlant, SpecialAirCollisionReaction.PassesThrough),
        [SpecialAirReaction.CeilingPlant] = new(SpecialAirInsideReaction.BrinstarCeilingPlant, SpecialAirCollisionReaction.PassesThrough),
        [SpecialAirReaction.QuicksandSurface] = new(SpecialAirInsideReaction.QuicksandSurface, SpecialAirCollisionReaction.QuicksandSurface),
        [SpecialAirReaction.SubmergingQuicksand] = new(SpecialAirInsideReaction.SubmergingQuicksand, SpecialAirCollisionReaction.SubmergingQuicksand),
        [SpecialAirReaction.SlowSandfall] = new(SpecialAirInsideReaction.SlowSandfall, SpecialAirCollisionReaction.SlowSandfall),
        [SpecialAirReaction.FastSandfall] = new(SpecialAirInsideReaction.FastSandfall, SpecialAirCollisionReaction.FastSandfall),
        [SpecialAirReaction.PassesThrough] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.Nothing),
        [SpecialAirReaction.RightwardConveyorOnFoot] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.Nothing),
        [SpecialAirReaction.LeftwardConveyorOnFoot] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.Nothing),
        [SpecialAirReaction.RightwardConveyor] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.Nothing),
        [SpecialAirReaction.LeftwardConveyor] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.Nothing),
        [SpecialAirReaction.ScrollTrigger] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.Nothing),
        [SpecialAirReaction.SlowRespawningSpeedBlock] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.BrinstarSlowRespawningSpeedBlock),
        [SpecialAirReaction.SlowPermanentSpeedBlock] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.BrinstarSlowPermanentSpeedBlock),
        [SpecialAirReaction.DachoraRespawningSpeedBlock] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.DachoraRespawningSpeedBlock),
        [SpecialAirReaction.PermanentSpeedBlock] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.PermanentSpeedBlock),
        [SpecialAirReaction.WreckedShipChozoHand] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.WreckedShipChozoHand),
        [SpecialAirReaction.LowerNorfairChozoHand] = new(SpecialAirInsideReaction.Nothing, SpecialAirCollisionReaction.LowerNorfairChozoHand),
    };

    /// <summary>What the special-air block with reaction <paramref name="reaction"/> does to Samus inside it.</summary>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block has no reaction.</exception>
    public static SpecialAirInsideReaction InsideOf(SpecialAirReaction? reaction) => Of(reaction, "inside-body").Inside;

    /// <summary>What the special-air block with reaction <paramref name="reaction"/> does when Samus's movement reaches it.</summary>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block has no reaction.</exception>
    public static SpecialAirCollisionReaction CollisionOf(SpecialAirReaction? reaction) => Of(reaction, "movement-collision").Collision;

    /// <summary>What the block's reaction does.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block has no reaction.</exception>
    /// <exception cref="InvalidOperationException">The reaction is undefined.</exception>
    private static (SpecialAirInsideReaction Inside, SpecialAirCollisionReaction Collision) Of(SpecialAirReaction? reaction, string context)
    {
        SpecialAirReaction validated = Validate(reaction, context);
        return Reactions.TryGetValue(validated, out var does)
            ? does
            : throw new InvalidOperationException($"Undefined {nameof(SpecialAirReaction)} {validated}.");
    }

    /// <summary>Requires the block to have a reaction.</summary>
    /// <returns>The block's reaction.</returns>
    private static SpecialAirReaction Validate(SpecialAirReaction? reaction, string context) =>
        reaction ?? throw new ArgumentOutOfRangeException(
            nameof(reaction),
            reaction,
            $"A special-air block's {context} reaction needs a reaction the block has.");
}
