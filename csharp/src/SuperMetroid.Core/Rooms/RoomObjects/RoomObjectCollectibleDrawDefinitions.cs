using D = SuperMetroid.Core.Rooms.RoomObjects.CollectibleDraw;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The collectible draws' groups and frames (#1762). Each frame writes one block; what a frame shows is its installed picture
/// (#1557), except a block pair's frame, which writes its pair's block under the item's picture
/// (<see cref="Rooms.LevelData.CollectiblePairBlock"/>, #2354).
/// </summary>
internal static partial class RoomObjectCollectibleDrawDefinitions
{
    /// <summary>The six kinds of collectible draw; each kind's blocks collide alike.</summary>
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
    internal static RoomObjectFrameDraw<D> Empty { get; } = CollectibleObjectDraws.Family.Of(D.Empty);

    /// <summary>The solid burst frame drawn when a Chozo orb opens.</summary>
    internal static RoomObjectFrameDraw<D> OrbBurst { get; } = CollectibleObjectDraws.Family.Of(D.ChozoOrbBurst);

    /// <summary>A concealed item's shot block whole, as its reveal begins and its hiding ends (<see cref="CollectibleSequenceAnimations"/>).</summary>
    internal static RoomObjectFrameDraw<D> ShotBlockIntact { get; } = CollectibleObjectDraws.Family.Of(D.ShotBlockIntact);

    /// <summary>Each tank kind's draw in each of its two looks (#1994).</summary>
    internal static readonly IReadOnlyDictionary<InWorldCollectibleKind, CollectibleLookDraws> TankLooks = new Dictionary<InWorldCollectibleKind, CollectibleLookDraws>
    {
        [InWorldCollectibleKind.EnergyTank] = new(Primary: D.EnergyTankBright, Alternate: D.EnergyTankDim),
        [InWorldCollectibleKind.MissileTank] = new(Primary: D.MissileTankDim, Alternate: D.MissileTankBright),
        [InWorldCollectibleKind.SuperMissileTank] = new(Primary: D.SuperMissileTankDim, Alternate: D.SuperMissileTankBright),
        [InWorldCollectibleKind.PowerBombTank] = new(Primary: D.PowerBombTankBright, Alternate: D.PowerBombTankDim),
    };

    /// <summary>Each block pair's draw under an item's own picture in each of the picture's two looks (#1994).</summary>
    internal static readonly IdentityTable<CollectibleBlockPairs.Pair, CollectibleLookDraws> BlockPairLooks = new(new Dictionary<CollectibleBlockPairs.Pair, CollectibleLookDraws>
        {
            [CollectibleBlockPairs.Pair.Solid] = new(Primary: D.SolidPairPrimaryLook, Alternate: D.SolidPairAlternateLook),
            [CollectibleBlockPairs.Pair.FirstDoor] = new(Primary: D.FirstDoorPairPrimaryLook, Alternate: D.FirstDoorPairAlternateLook),
            [CollectibleBlockPairs.Pair.SecondDoor] = new(Primary: D.SecondDoorPairPrimaryLook, Alternate: D.SecondDoorPairAlternateLook),
            [CollectibleBlockPairs.Pair.ThirdDoor] = new(Primary: D.ThirdDoorPairPrimaryLook, Alternate: D.ThirdDoorPairAlternateLook),
        });
}
