using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Cartridge values and helpers of <see cref="SporeSpawnCeilingObjectDrawDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnCeilingObjectDrawDefinitionsConstants
{
    /// <summary>Maps zero-based crumble stage zero through two to its bank-$84 draw pointer.</summary>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(SporeSpawnCeilingDraw.CrumbleFirst)),
        1 => RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(SporeSpawnCeilingDraw.CrumbleSecond)),
        2 => RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(SporeSpawnCeilingDraw.CrumbleThird)),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
