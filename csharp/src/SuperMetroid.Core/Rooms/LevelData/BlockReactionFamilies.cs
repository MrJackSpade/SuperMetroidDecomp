namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Which reaction family each collision kind reads, and every reaction of each family (#1722).</summary>
public static class BlockReactionFamilies
{
    /// <summary>The family <paramref name="kind"/> reads its block's reaction from, or null for a kind that reads none.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="kind"/> is undefined.</exception>
    public static BlockReactionFamily? Of(RoomCollisionType kind) => kind switch
    {
        RoomCollisionType.ShootableBlock or RoomCollisionType.ShootableAir => BlockReactionFamily.Shootable,
        RoomCollisionType.BombableBlock or RoomCollisionType.BombableAir => BlockReactionFamily.Bombable,
        RoomCollisionType.SpecialBlock => BlockReactionFamily.Special,
        RoomCollisionType.SpecialAir => BlockReactionFamily.SpecialAir,
        RoomCollisionType.SpikeBlock or RoomCollisionType.SpikeAir => BlockReactionFamily.Spike,
        RoomCollisionType.GrappleBlock => BlockReactionFamily.Grapple,
        RoomCollisionType.Air or RoomCollisionType.Slope or RoomCollisionType.HorizontalExtension or
            RoomCollisionType.UnrevealedAir or RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock or
            RoomCollisionType.VerticalExtension => null,
        _ => throw new InvalidOperationException($"Undefined collision kind {kind}."),
    };

    /// <summary>Every reaction of <paramref name="family"/>, in its declaration order.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="family"/> is undefined.</exception>
    public static IReadOnlyList<Enum> Reactions(BlockReactionFamily family) => family switch
    {
        BlockReactionFamily.Shootable => [.. Enum.GetValues<ShootableReaction>().Cast<Enum>()],
        BlockReactionFamily.Bombable => [.. Enum.GetValues<BombableReaction>().Cast<Enum>()],
        BlockReactionFamily.Special => [.. Enum.GetValues<SpecialBlockReaction>().Cast<Enum>()],
        BlockReactionFamily.SpecialAir => [.. Enum.GetValues<SpecialAirReaction>().Cast<Enum>()],
        BlockReactionFamily.Spike => [.. Enum.GetValues<SpikeReaction>().Cast<Enum>()],
        BlockReactionFamily.Grapple => [.. Enum.GetValues<GrappleReaction>().Cast<Enum>()],
        _ => throw new InvalidOperationException($"Undefined reaction family {family}."),
    };

    /// <summary>
    /// The reactions <paramref name="kind"/> distinguishes: every reaction of its family, or a single null for a kind that
    /// reads no reaction.
    /// </summary>
    public static IReadOnlyList<Enum?> ReadingsOf(RoomCollisionType kind) =>
        Of(kind) is { } family ? [.. Reactions(family)] : [null];
}
