using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Cartridge values and helpers of <see cref="SporeSpawnCeilingObjectDrawDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnCeilingObjectDrawDefinitionsConstants
{
    /// <summary>Maps zero-based crumble stage zero through two to its bank-$84 draw pointer.</summary>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => RoomObjectDrawAddressCodec.Of(SporeSpawnCeilingDraws.Family.Of(SporeSpawnCeilingDraw.CeilingCracked)),
        1 => RoomObjectDrawAddressCodec.Of(SporeSpawnCeilingDraws.Family.Of(SporeSpawnCeilingDraw.CeilingCrumbling)),
        2 => RoomObjectDrawAddressCodec.Of(SporeSpawnCeilingDraws.Family.Of(SporeSpawnCeilingDraw.CeilingCrumbled)),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
