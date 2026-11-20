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

    /// <summary>Every reaction of <paramref name="family"/>, in the family's written-out order (its keys' listed order, #2567).</summary>
    /// <exception cref="InvalidOperationException"><paramref name="family"/> is undefined.</exception>
    public static IReadOnlyList<BlockReaction> Reactions(BlockReactionFamily family) => family switch
    {
        BlockReactionFamily.Shootable => [.. Ensure.EveryMemberOnce(ShootableReactionKeys.ListedOrder).Select(reaction => BlockReaction.Of(reaction))],
        BlockReactionFamily.Bombable => [.. Ensure.EveryMemberOnce(BombableReactionKeys.ListedOrder).Select(reaction => BlockReaction.Of(reaction))],
        BlockReactionFamily.Special => [.. Ensure.EveryMemberOnce(SpecialBlockReactionKeys.ListedOrder).Select(reaction => BlockReaction.Of(reaction))],
        BlockReactionFamily.SpecialAir => [.. Ensure.EveryMemberOnce(SpecialAirReactionKeys.ListedOrder).Select(reaction => BlockReaction.Of(reaction))],
        BlockReactionFamily.Spike => [.. Ensure.EveryMemberOnce(SpikeReactionKeys.ListedOrder).Select(reaction => BlockReaction.Of(reaction))],
        BlockReactionFamily.Grapple => [.. Ensure.EveryMemberOnce(GrappleReactionKeys.ListedOrder).Select(reaction => BlockReaction.Of(reaction))],
        _ => throw new InvalidOperationException($"Undefined reaction family {family}."),
    };

    /// <summary>
    /// The reactions <paramref name="kind"/> distinguishes: every reaction of its family, or none for a kind that reads no
    /// reaction (#2558), a case those reading it state themselves.
    /// </summary>
    public static IReadOnlyList<BlockReaction> ReadingsOf(RoomCollisionType kind) =>
        Of(kind) is { } family ? [.. Reactions(family)] : [];
}
