namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Every reading of one behaviour a block can keep (#1722): what each collision kind that reads a behaviour makes of it.
/// The kept behaviours are the catalog's (<see cref="RoomBlockReadingDefinitions"/>); a block holds one of them as its
/// <see cref="RoomBlockBehavior"/>. A reacting kind's reading is null when the kind has no reaction for the behaviour: no
/// retail block of that kind holds it, the import refuses one, and the kind's dispatch rejects one a kind change brings
/// (#1742).
/// </summary>
/// <param name="Shootable">The reaction shootable blocks and shootable air read, or null for none.</param>
/// <param name="Bombable">The reaction bombable blocks and bombable air read, or null for none.</param>
/// <param name="Special">The reaction special blocks read, or null for none.</param>
/// <param name="SpecialAir">The reaction special-air blocks read, or null for none.</param>
/// <param name="Spike">The reaction spike blocks and spike air read, or null for none.</param>
/// <param name="Grapple">The reaction grapple blocks read, or null for none.</param>
/// <param name="Slope">The shape and mirroring slopes read.</param>
/// <param name="ExtensionOffset">The signed offset extensions read, <see cref="RoomBlockBehavior.MostBackwardExtension"/> through <see cref="RoomBlockBehavior.MostForwardExtension"/>.</param>
/// <param name="DoorLink">The entry of the room's door list door blocks read, 0 through <see cref="RoomBlockBehavior.LastDoorLink"/>.</param>
public sealed record RoomBlockReadings(
    ShootableReaction? Shootable,
    BombableReaction? Bombable,
    SpecialBlockReaction? Special,
    SpecialAirReaction? SpecialAir,
    SpikeReaction? Spike,
    GrappleReaction? Grapple,
    SlopeGeometry Slope,
    int ExtensionOffset,
    int DoorLink)
{
    /// <summary>The signed offset extensions read.</summary>
    public int ExtensionOffset { get; } =
        Ensure.BetweenInclusive(RoomBlockBehavior.MostBackwardExtension, RoomBlockBehavior.MostForwardExtension, ExtensionOffset);

    /// <summary>The entry of the room's door list door blocks read.</summary>
    public int DoorLink { get; } = Ensure.BetweenInclusive(0, RoomBlockBehavior.LastDoorLink, DoorLink);
}
