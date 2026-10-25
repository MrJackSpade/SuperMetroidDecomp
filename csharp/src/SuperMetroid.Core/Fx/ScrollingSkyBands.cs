using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Fx;

/// <summary>Each band of the Landing Site's scrolling sky: where it starts, how fast it drifts and which accumulator it adds to (#1415).</summary>
public static class ScrollingSkyBands
{
    /// <summary>Selects a sky band's top world row, its drift a frame and its horizontal-scroll slot.</summary>
    /// <remarks>The bands have fixed geometry, so their tops are explicit cases. Bands 0..16 alternate drifting half
    /// and three quarters of a pixel a frame; band 17 drifts seven sixteenths, bands 18/19 three quarters then a half,
    /// and the three bottom bands are stationary. Slots follow band indices except band 11 targets slot 8 again, giving
    /// it two additions per frame. All four fields are independently verified against supported NTSC J/U v1.0 and
    /// pinned bank_88.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// Reject outside 0..22 before computing fields; no row array/cache remains.</remarks>
    public static SkyScrollSection GetSection(int index)
    {
        ushort top = index switch
        {
            0 => 0, 1 => 16, 2 => 56, 3 => 208,
            4 => 224, 5 => 288, 6 => 416, 7 => 472,
            8 => 568, 9 => 616, 10 => 672, 11 => 736,
            12 => 768, 13 => 800, 14 => 848, 15 => 888,
            16 => 968, 17 => 1088, 18 => 1120, 19 => 1152,
            20 => 1168, 21 => 1192, 22 => 1208,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        int sixteenths = index < 17 ? 8 + (index & 1) * 4 :
            index == 17 ? 7 : index < 20 ? 12 - (index - 18) * 4 : 0;
        return new(top, SnesSignedSixteenSixteen.FromSixteenthsOfPixel(sixteenths), index == 11 ? 8 : index);
    }
}
