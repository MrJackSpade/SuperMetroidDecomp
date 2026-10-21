using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Cartridge values and helpers of <see cref="SporeSpawnCeilingPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnCeilingPlmDrawDefinitionsConstants
{
    /// <summary>Maps zero-based crumble stage zero through two to its bank-$84 draw pointer.</summary>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => PlmDrawAddressCodec.Of(PlmDraw.Of(SporeSpawnCeilingDraw.CrumbleFirst)),
        1 => PlmDrawAddressCodec.Of(PlmDraw.Of(SporeSpawnCeilingDraw.CrumbleSecond)),
        2 => PlmDrawAddressCodec.Of(PlmDraw.Of(SporeSpawnCeilingDraw.CrumbleThird)),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
