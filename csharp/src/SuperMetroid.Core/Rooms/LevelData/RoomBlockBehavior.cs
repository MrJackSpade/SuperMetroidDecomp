using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a room block does beyond how it collides (#1551). The block's collision kind decides which of four readings
/// applies: a slope reads its <see cref="Slope"/>, an extension its <see cref="ExtensionOffset"/>, a door its
/// <see cref="DoorLink"/>, and every reacting kind (shootable, bombable, special, spike and grapple blocks, and their air
/// siblings) its <see cref="Reaction"/>.
/// </summary>
/// <remarks>
/// A block whose collision kind changes keeps its behaviour, and the new kind reads it through its own reading, so every
/// behaviour holds all four. The installation decodes them once from the game data; a behaviour a room object writes
/// is written as the reading it is meant for, and carries the others as <see cref="BlockParameterCarryOver"/> states.
/// </remarks>
public readonly record struct RoomBlockBehavior
{
    /// <summary>Holds a behaviour's four readings, which the import's codec decodes together.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The offset or door link is out of range.</exception>
    internal RoomBlockBehavior(BlockReaction reaction, SlopeGeometry slope, int extensionOffset, int doorLink)
    {
        Reaction = reaction;
        Slope = slope;
        ExtensionOffset = Ensure.BetweenInclusive(MostBackwardExtension, MostForwardExtension, extensionOffset);
        DoorLink = Ensure.BetweenInclusive(0, BlockReaction.LastEntry, doorLink);
    }

    /// <summary>The farthest an extension reaches backward: 128 blocks (or rows).</summary>
    public const int MostBackwardExtension = -BlockParameterCarryOver.EntriesPerReactionList;

    /// <summary>The farthest an extension reaches forward: 127 blocks (or rows).</summary>
    public const int MostForwardExtension = BlockParameterCarryOver.EntriesPerReactionList - 1;

    /// <summary>
    /// The empty behaviour: the first common reaction, a flat unmirrored slope, an extension that ends its chain, and the
    /// room's first door. Every block past a room's grid starts with it.
    /// </summary>
    public static RoomBlockBehavior None { get; } = Reacting(BlockReaction.Common(0));

    /// <summary>A reacting block's behaviour.</summary>
    /// <param name="reaction">The reaction.</param>
    public static RoomBlockBehavior Reacting(BlockReaction reaction)
    {
        BlockMirroring mirroring =
            (BlockParameterCarryOver.ReflectsSlopeHorizontally(reaction.Entry) ? BlockMirroring.Horizontal : BlockMirroring.None) |
            (reaction.IsAreaSpecific ? BlockMirroring.Vertical : BlockMirroring.None);
        return new RoomBlockBehavior(
            reaction,
            new SlopeGeometry(BlockParameterCarryOver.SlopeShapeOf(reaction.Entry), mirroring),
            BlockParameterCarryOver.ExtensionOffsetOf(reaction.IsAreaSpecific, reaction.Entry),
            reaction.Entry);
    }

    /// <summary>An extension's behaviour.</summary>
    /// <param name="offset">The signed displacement, <see cref="MostBackwardExtension"/> through <see cref="MostForwardExtension"/>.</param>
    public static RoomBlockBehavior Extending(int offset)
    {
        Ensure.BetweenInclusive(MostBackwardExtension, MostForwardExtension, offset);
        return Reacting(new BlockReaction(
            BlockParameterCarryOver.IsAreaSpecificReaction(offset) ? BlockReactionScope.AreaSpecific : BlockReactionScope.Common,
            BlockParameterCarryOver.ReactionEntryOf(offset)));
    }

    /// <summary>A slope's behaviour.</summary>
    /// <param name="slope">The slope's shape and mirroring.</param>
    public static RoomBlockBehavior Sloping(SlopeGeometry slope) =>
        Reacting(new BlockReaction(
            slope.FlipsVertically ? BlockReactionScope.AreaSpecific : BlockReactionScope.Common,
            BlockParameterCarryOver.ReactionEntryOf(slope.Shape, slope.FlipsHorizontally)));

    /// <summary>A door block's behaviour.</summary>
    /// <param name="doorLink">The entry of the room's door list the door selects.</param>
    public static RoomBlockBehavior LinkingDoor(int doorLink) => Reacting(BlockReaction.Common(doorLink));

    /// <summary>The reaction a shootable, bombable, special, spike or grapple block (or its air sibling) selects.</summary>
    public BlockReaction Reaction { get; }

    /// <summary>A slope block's shape and mirroring.</summary>
    public SlopeGeometry Slope { get; }

    /// <summary>
    /// The signed number of blocks (horizontal) or rows (vertical) from an extension to the block it collides as; zero
    /// ends the chain, and the extension then behaves as its kind's terminator.
    /// </summary>
    public int ExtensionOffset { get; }

    /// <summary>Whether an extension with this behaviour ends its chain instead of leading to another block.</summary>
    public bool EndsExtension => ExtensionOffset == 0;

    /// <summary>
    /// The entry of the room's door list a door block selects. Elevator and special-door blocks select through the same
    /// list.
    /// </summary>
    public int DoorLink { get; }

    /// <summary>The blue-door cap a shootable block's reaction selects, if it selects one.</summary>
    public bool TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation) =>
        BlockReactionDefinitions.BlueDoorCaps.TryGetMeaning(Reaction, out orientation);

    /// <summary>
    /// The crumble block a special block's reaction selects, if it selects one. The same reactions mean unrelated things to
    /// other collision kinds, so the caller first establishes that the block is special.
    /// </summary>
    public bool TryGetContactCrumble(out ContactCrumbleBlock crumble) =>
        BlockReactionDefinitions.ContactCrumbles.TryGetMeaning(Reaction, out crumble);

    /// <summary>The map, recharge or save-station access a special block's reaction selects, if it selects one.</summary>
    public bool TryGetStationAccess(out StationAccessBehavior access) =>
        BlockReactionDefinitions.StationAccesses.TryGetMeaning(Reaction, out access);

    /// <summary>
    /// The downward-gate trigger a shootable block's reaction selects, if it selects one. The same reactions mean
    /// unrelated things to other collision kinds, so the caller first establishes that the block is shootable.
    /// </summary>
    public bool TryGetDownwardGateTrigger(out DownwardGateTriggerBehavior trigger) =>
        BlockReactionDefinitions.DownwardGateTriggers.TryGetMeaning(Reaction, out trigger);

    /// <summary>Names the behaviour by its reaction, which every reading follows from.</summary>
    public override string ToString() => $"reaction {Reaction}";
}
