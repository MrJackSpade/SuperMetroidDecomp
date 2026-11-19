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

    /// <summary>The installed key of <paramref name="reaction"/>, a reaction of one of the families (#2520).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is no family's reaction.</exception>
    [InstalledKey]
    public static string KeyOf(Enum reaction) => reaction switch
    {
        ShootableReaction shootable => ShootableReactionKeys.Installed.Of(shootable),
        BombableReaction bombable => BombableReactionKeys.Installed.Of(bombable),
        SpecialBlockReaction special => SpecialBlockReactionKeys.Installed.Of(special),
        SpecialAirReaction specialAir => SpecialAirReactionKeys.Installed.Of(specialAir),
        SpikeReaction spike => SpikeReactionKeys.Installed.Of(spike),
        GrappleReaction grapple => GrappleReactionKeys.Installed.Of(grapple),
        _ => throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The value is no block reaction."),
    };

    /// <summary>Every reaction of <paramref name="family"/>, in the family's written-out order (its keys' listed order, #2567).</summary>
    /// <exception cref="InvalidOperationException"><paramref name="family"/> is undefined.</exception>
    public static IReadOnlyList<Enum> Reactions(BlockReactionFamily family) => family switch
    {
        BlockReactionFamily.Shootable => [.. Ensure.EveryMemberOnce(ShootableReactionKeys.ListedOrder).Cast<Enum>()],
        BlockReactionFamily.Bombable => [.. Ensure.EveryMemberOnce(BombableReactionKeys.ListedOrder).Cast<Enum>()],
        BlockReactionFamily.Special => [.. Ensure.EveryMemberOnce(SpecialBlockReactionKeys.ListedOrder).Cast<Enum>()],
        BlockReactionFamily.SpecialAir => [.. Ensure.EveryMemberOnce(SpecialAirReactionKeys.ListedOrder).Cast<Enum>()],
        BlockReactionFamily.Spike => [.. Ensure.EveryMemberOnce(SpikeReactionKeys.ListedOrder).Cast<Enum>()],
        BlockReactionFamily.Grapple => [.. Ensure.EveryMemberOnce(GrappleReactionKeys.ListedOrder).Cast<Enum>()],
        _ => throw new InvalidOperationException($"Undefined reaction family {family}."),
    };

    /// <summary>
    /// The reactions <paramref name="kind"/> distinguishes: every reaction of its family, or a single null for a kind that
    /// reads no reaction.
    /// </summary>
    public static IReadOnlyList<Enum?> ReadingsOf(RoomCollisionType kind) =>
        Of(kind) is { } family ? [.. Reactions(family)] : [null];
}
