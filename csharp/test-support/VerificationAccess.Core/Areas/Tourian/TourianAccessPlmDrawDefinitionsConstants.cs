using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Cartridge values and helpers of <see cref="TourianAccessPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class TourianAccessPlmDrawDefinitionsConstants
{
    /// <summary>Maps the four visible crumble stages to three tile rows followed by the empty row.</summary>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => PlmDrawAddressCodec.Of(PlmDraw.Of(TourianAccessDraw.CrumbleFirst)),
        1 => PlmDrawAddressCodec.Of(PlmDraw.Of(TourianAccessDraw.CrumbleSecond)),
        2 => PlmDrawAddressCodec.Of(PlmDraw.Of(TourianAccessDraw.CrumbleThird)),
        3 => PlmDrawAddressCodec.Of(PlmDraw.Of(TourianAccessDraw.EmptyRow)),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>The rows and air block of <paramref name="request"/> when it is a Tourian access draw.</summary>
    internal static bool TryDescribe(PlmDraw request, out int rows, out RoomTerrainBlock block)
    {
        bool found = PlmDraw.TryFrame(request, out TourianAccessDraw frame);
        rows = 0;
        block = default;
        if (found)
            TourianAccessDraws.Describe(frame, out rows, out block);
        return found;
    }

    /// <summary>The four-air-cell runs of <paramref name="request"/> when it is a Tourian access draw, including all six rows of the final clear.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = PlmDraw.TryFrame(request, out TourianAccessDraw frame);
        list = found ? TourianAccessDraws.Compile(frame) : default;
        return found;
    }
}
