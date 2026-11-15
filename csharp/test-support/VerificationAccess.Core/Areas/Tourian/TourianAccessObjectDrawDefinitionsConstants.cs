using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Cartridge values and helpers of <see cref="TourianAccessObjectDrawDefinitions"/> that only verification reads.</summary>
internal static class TourianAccessObjectDrawDefinitionsConstants
{
    /// <summary>Maps the four visible crumble stages to three tile rows followed by the empty row.</summary>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => RoomObjectDrawAddressCodec.Of(TourianAccessDraws.Family.Of(TourianAccessDraw.CrumbleFirst)),
        1 => RoomObjectDrawAddressCodec.Of(TourianAccessDraws.Family.Of(TourianAccessDraw.CrumbleSecond)),
        2 => RoomObjectDrawAddressCodec.Of(TourianAccessDraws.Family.Of(TourianAccessDraw.CrumbleThird)),
        3 => RoomObjectDrawAddressCodec.Of(TourianAccessDraws.Family.Of(TourianAccessDraw.EmptyRow)),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>The rows the block shape of <paramref name="request"/> covers when it is a Tourian access draw.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out int rows)
    {
        bool found = RoomObjectDrawIdentity.TryShape(TourianAccessDraws.Family, request, out RoomBlockShape? shape);
        rows = found && shape is not null ? shape.Cells.Select(cell => cell.Cell.Y).Distinct().Count() : 0;
        return found;
    }

    /// <summary>The four-air-cell runs of <paramref name="request"/> when it is a Tourian access draw, including all six rows of the final clear.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        bool found = RoomObjectDrawIdentity.TryFrame(request, out TourianAccessDraw frame);
        list = found ? RoomObjectTourianAccessVisualFiles.Compile(frame) : default;
        return found;
    }
}
