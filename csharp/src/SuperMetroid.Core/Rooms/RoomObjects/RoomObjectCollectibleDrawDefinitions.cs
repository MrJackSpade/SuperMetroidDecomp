
namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The collectible draws' groups and frames. Each frame writes one block; what a frame shows is its installed picture
/// (#1557), except a block pair's frame, which writes its own block number under the item's picture.
/// </summary>
internal static class RoomObjectCollectibleDrawDefinitions
{
    /// <summary>
    /// The block of the first block pair's first frame, which an item that shows its own picture writes under the picture;
    /// it is a special block, and each later frame writes the next block. The number is gameplay terrain, not a picture:
    /// a Wave shot's straddled read past the room's ceiling takes its collision family from the block number a block
    /// pair's collectible wrote (<see cref="Compatibility.CollectibleBlockPairs"/>), so the game writes exactly it.
    /// </summary>
    internal const int OwnPictureFirstBlock = 142;

    /// <summary>The six collectible draw groups, in the order their frames are published.</summary>
    internal enum DrawGroup
    {
        /// <summary>Air block after an item is removed.</summary>
        Empty,
        /// <summary>Three shootable Chozo orb animation frames.</summary>
        Orb,
        /// <summary>Solid burst frame when a Chozo orb opens.</summary>
        OrbBurst,
        /// <summary>Two visible frames for each of four tank pickup kinds.</summary>
        Tank,
        /// <summary>Two frames for each of the block pairs items that show their own picture take in turn.</summary>
        OwnPicture,
        /// <summary>Three solid frames of a shot-block item reveal.</summary>
        ShotReveal,
    }

    /// <summary>One collectible draw: a group and the frame within it.</summary>
    /// <param name="Group">The group of frames the draw belongs to.</param>
    /// <param name="Index">The frame within the group, counted from zero.</param>
    internal readonly record struct Draw(DrawGroup Group, [property: IntegerQuantity("frame within the collectible draw group")] int Index);

    /// <summary>The air block left after an item is removed.</summary>
    internal static RoomObjectFrameDraw<Draw> Empty { get; } = RoomObjectDraw.Of(new Draw(DrawGroup.Empty, 0));

    /// <summary>The solid burst frame drawn when a Chozo orb opens.</summary>
    internal static RoomObjectFrameDraw<Draw> OrbBurst { get; } = RoomObjectDraw.Of(new Draw(DrawGroup.OrbBurst, 0));

    /// <summary>The tank kinds in the order of their draws.</summary>
    internal static readonly InWorldCollectibleKind[] TankKinds =
    [
        InWorldCollectibleKind.EnergyTank, InWorldCollectibleKind.MissileTank,
        InWorldCollectibleKind.SuperMissileTank, InWorldCollectibleKind.PowerBombTank,
    ];
}
