namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Every reading of one behaviour a block can keep (#1722): what each collision kind that reads a behaviour makes of it.
/// The kept behaviours are the catalog's (<see cref="RoomBlockReadingDefinitions"/>); a block holds one of them as its
/// <see cref="RoomBlockBehavior"/>. A reacting kind's reading is null when the kind has no reaction for the behaviour: no
/// block of that kind holds it, the room's level refuses one as it loads and refuses a write that brings one (#1742,
/// #2554). A behaviour whose meaning to special blocks and special air the room's area gives is resolved by its area: it
/// holds neither reading until a room's area resolves it (<see cref="AreaBlockReadings"/>, #1805).
/// </summary>
/// <param name="Shootable">
/// The reaction shootable blocks and shootable air read: every behaviour gives them one, as every behaviour a block can
/// keep has a shootable reaction in the game (#2553).
/// </param>
/// <param name="Bombable">The reaction bombable blocks and bombable air read, or null for none.</param>
/// <param name="Special">The reaction special blocks read, or null for none.</param>
/// <param name="SpecialAir">The reaction special-air blocks read, or null for none.</param>
/// <param name="Spike">The reaction spike blocks and spike air read, or null for none.</param>
/// <param name="Grapple">The reaction grapple blocks read, or null for none.</param>
/// <param name="Slope">The shape and mirroring slopes read.</param>
/// <param name="ExtensionOffset">The signed offset extensions read, in blocks (or rows).</param>
/// <param name="ResolvedByArea">
/// Whether the room's area gives the behaviour its meaning to special blocks and special air, so it holds neither reading
/// until an area resolves it; a behaviour that is not holds those kinds' own readings, null only for no reaction.
/// </param>
/// <remarks>Where a door block leads is its room's own (<see cref="RoomDoorBlock"/>, #2090), not a reading.</remarks>
public sealed record RoomBlockReadings(
    ShootableReaction Shootable,
    BombableReaction? Bombable,
    SpecialBlockReaction? Special,
    SpecialAirReaction? SpecialAir,
    SpikeReaction? Spike,
    GrappleReaction? Grapple,
    SlopeGeometry Slope,
    int ExtensionOffset,
    bool ResolvedByArea = false);
