namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The runs of images a collectible shows at its block (<see cref="CollectibleSequenceAnimations"/>, #1994, #2480).</summary>
internal enum CollectibleSequence
{
    /// <summary>A Chozo orb's shell glowing brighter and dimmer again.</summary>
    OrbPulsing,
    /// <summary>A shot Chozo orb bursting open around its item.</summary>
    OrbBurst,
    /// <summary>The item's two looks, one after the other, four updates each.</summary>
    Looks,
    /// <summary>A revealed item's primary look, the first half of its last pair.</summary>
    RevealedLastPrimary,
    /// <summary>A revealed item's alternate look, the last of its pairs, as its shot block starts to form again over it.</summary>
    RevealedLastAlternate,
    /// <summary>A shot block crumbling away from the item it conceals.</summary>
    ShotBlockReveal,
    /// <summary>A shot block forming again over an item nobody took.</summary>
    ShotBlockConceal,
    /// <summary>A collected item's shot block standing empty before it grows back.</summary>
    CollectedShotBlockEmpty,
    /// <summary>A collected item's shot block growing back.</summary>
    ShotBlockRespawn,
    /// <summary>A picked-up item's empty block, before the item leaves the room.</summary>
    PickedUpEmpty,
}
