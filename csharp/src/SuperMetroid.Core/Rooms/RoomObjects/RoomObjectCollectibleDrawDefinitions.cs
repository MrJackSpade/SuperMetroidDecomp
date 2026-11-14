using SuperMetroid.Core.Compatibility;
using D = SuperMetroid.Core.Rooms.RoomObjects.CollectibleDraw;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The collectible draws' groups and frames (#1762). Each frame writes one block; what a frame shows is its installed picture
/// (#1557), except a block pair's frame, which writes its own block number under the item's picture.
/// </summary>
internal static class RoomObjectCollectibleDrawDefinitions
{
    /// <summary>
    /// The block of the first block pair's first frame, which an item that shows its own picture writes under the picture;
    /// it is a special block, and each later frame writes the next block. The number is gameplay terrain, not a picture:
    /// a Wave shot's straddled read past the room's ceiling takes its collision family from the block number a block
    /// pair's collectible wrote (<see cref="CollectibleBlockPairs"/>), so the game writes exactly it.
    /// </summary>
    internal const int OwnPictureFirstBlock = 142;

    /// <summary>The six kinds of collectible draw, in the order their frames are published; each kind's blocks collide alike.</summary>
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

    /// <summary>The air block left after an item is removed.</summary>
    internal static RoomObjectFrameDraw<D> Empty { get; } = RoomObjectDraw.Of(D.Empty);

    /// <summary>The solid burst frame drawn when a Chozo orb opens.</summary>
    internal static RoomObjectFrameDraw<D> OrbBurst { get; } = RoomObjectDraw.Of(D.ChozoOrbBurst);

    /// <summary>A Chozo orb's three frames, dimmest first; its animation runs forward through them and back through the middle one.</summary>
    internal static readonly D[] OrbFrames = [D.ChozoOrbDim, D.ChozoOrbGlowing, D.ChozoOrbBright];

    /// <summary>A shot block's frames as a concealed item's reveal shows them; its hiding shows them in reverse.</summary>
    internal static readonly D[] ShotRevealFrames = [D.ShotBlockIntact, D.ShotBlockCracking, D.ShotBlockDust];

    /// <summary>The tank kinds in the order of their draws.</summary>
    internal static readonly InWorldCollectibleKind[] TankKinds =
    [
        InWorldCollectibleKind.EnergyTank, InWorldCollectibleKind.MissileTank,
        InWorldCollectibleKind.SuperMissileTank, InWorldCollectibleKind.PowerBombTank,
    ];

    /// <summary>Each tank kind's two frames, in the order its animation shows them.</summary>
    internal static readonly IReadOnlyDictionary<InWorldCollectibleKind, D[]> TankFrames = new Dictionary<InWorldCollectibleKind, D[]>
    {
        [InWorldCollectibleKind.EnergyTank] = [D.EnergyTankBright, D.EnergyTankDim],
        [InWorldCollectibleKind.MissileTank] = [D.MissileTankDim, D.MissileTankBright],
        [InWorldCollectibleKind.SuperMissileTank] = [D.SuperMissileTankDim, D.SuperMissileTankBright],
        [InWorldCollectibleKind.PowerBombTank] = [D.PowerBombTankBright, D.PowerBombTankDim],
    };

    /// <summary>Each block pair's two frames under an item's own picture, in the order the picture's animation shows them.</summary>
    internal static readonly IReadOnlyDictionary<CollectibleBlockPairs.Pair, D[]> BlockPairFrames =
        new Dictionary<CollectibleBlockPairs.Pair, D[]>
        {
            [CollectibleBlockPairs.Pair.First] = [D.FirstBlockPairFirstFrame, D.FirstBlockPairSecondFrame],
            [CollectibleBlockPairs.Pair.Second] = [D.SecondBlockPairFirstFrame, D.SecondBlockPairSecondFrame],
            [CollectibleBlockPairs.Pair.Third] = [D.ThirdBlockPairFirstFrame, D.ThirdBlockPairSecondFrame],
            [CollectibleBlockPairs.Pair.Fourth] = [D.FourthBlockPairFirstFrame, D.FourthBlockPairSecondFrame],
        };

    /// <summary>
    /// The block each block pair's frame writes under the item's own picture: from <see cref="OwnPictureFirstBlock"/>, each
    /// pair's two frames after the previous pair's, in the turn the room's collectibles take the pairs in.
    /// </summary>
    internal static readonly IReadOnlyDictionary<D, int> BlockPairBlocks = new Dictionary<D, int>
    {
        [D.FirstBlockPairFirstFrame] = OwnPictureFirstBlock,
        [D.FirstBlockPairSecondFrame] = OwnPictureFirstBlock + 1,
        [D.SecondBlockPairFirstFrame] = OwnPictureFirstBlock + 2,
        [D.SecondBlockPairSecondFrame] = OwnPictureFirstBlock + 3,
        [D.ThirdBlockPairFirstFrame] = OwnPictureFirstBlock + 4,
        [D.ThirdBlockPairSecondFrame] = OwnPictureFirstBlock + 5,
        [D.FourthBlockPairFirstFrame] = OwnPictureFirstBlock + 6,
        [D.FourthBlockPairSecondFrame] = OwnPictureFirstBlock + 7,
    };
}
