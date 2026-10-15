using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Cartridge values and helpers of <see cref="TourianAccessPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class TourianAccessPlmDrawDefinitionsConstants
{
    /// <summary>Maps the four visible crumble stages to three tile rows followed by the empty row.</summary>
    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => (ushort)TourianAccessDraw.CrumbleFirst,
        1 => (ushort)TourianAccessDraw.CrumbleSecond,
        2 => (ushort)TourianAccessDraw.CrumbleThird,
        3 => (ushort)TourianAccessDraw.EmptyRow,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
