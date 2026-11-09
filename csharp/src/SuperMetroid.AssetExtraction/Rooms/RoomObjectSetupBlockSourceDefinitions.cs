using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Where the cartridge stores each block a room-object setup or block reaction writes of its own accord (#1580): the level
/// word each setup substitutes or writes, the import pipeline's private knowledge. A block written by several setups lists
/// each of their words, which the import requires to agree. Core identifies the blocks only semantically.
/// </summary>
[EncodingOwner("bank-$84 addresses of the room-object setup blocks")]
internal static class RoomObjectSetupBlockSourceDefinitions
{
    /// <summary>Bank $84, where every room-object setup is stored.</summary>
    private const byte Bank = 0x84;

    /// <summary>The level words that hold each setup block, in the setups' order.</summary>
    internal static IReadOnlyDictionary<RoomObjectSetupBlock, IReadOnlyList<SnesAddress>> Words { get; } = new Dictionary<RoomObjectSetupBlock, IReadOnlyList<SnesAddress>>
    {
        // The collision bomb block's, the Power Bomb's and the normal bomb's bomb-block setups.
        [RoomObjectSetupBlock.BombBlockParent] = [new(Bank, 0xcecc), new(Bank, 0xceff), new(Bank, 0xcf21)],
        [RoomObjectSetupBlock.CrumbleBlockParent] = [new(Bank, 0xce4d)],
        [RoomObjectSetupBlock.SpeedBoosterBlockParent] = [new(Bank, 0xce29)],
        [RoomObjectSetupBlock.ShotBlockParent] = [new(Bank, 0xce76)],
        [RoomObjectSetupBlock.PowerBombBlockParent] = [new(Bank, 0xcf53)],
        [RoomObjectSetupBlock.SuperMissileBlockParent] = [new(Bank, 0xcf8c)],
        [RoomObjectSetupBlock.CritterWallStruck] = [new(Bank, 0xb995)],
        [RoomObjectSetupBlock.TreadmillIdle] = [new(Bank, 0xb04e)],
        // The rightwards and the leftwards belt's row writes.
        [RoomObjectSetupBlock.TreadmillRunning] = [new(Bank, 0xad46), new(Bank, 0xad5b)],
    };
}
