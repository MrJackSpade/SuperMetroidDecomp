using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Cartridge values and helpers of <see cref="RoomDoorBackground"/> that only verification reads.</summary>
internal static class RoomDoorBackgroundConstants
{
    /// <summary>The source screen's extent: the gameplay viewport below the HUD.</summary>
    public static Extent16<BgPlanePixels> WindowSize { get; } = new(SnesPpuLayout.ScreenWidthPixels,
        SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels);
}
