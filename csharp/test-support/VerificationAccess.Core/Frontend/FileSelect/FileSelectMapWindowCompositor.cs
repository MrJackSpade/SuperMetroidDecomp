using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Opposing BG1/OBJ and BG2 window-one masks installed by $81:AAAC/$81:AFF6.</summary>
public static class FileSelectMapWindowCompositor
{
    /// <summary>As above, writing into <paramref name="pixels"/>, which must not alias either input.</summary>
    public static RgbaPixel[] CompositeInitialEntryWindow(ReadOnlySpan<RgbaPixel> area, ReadOnlySpan<RgbaPixel> roomFrame, RgbaPixel[] pixels)
    {
        int length = FrontendFrame.Width * FrontendFrame.Height;
        if (area.Length != length || roomFrame.Length != length || pixels.Length != length)
            throw new ArgumentException("File-select entry windows require two complete scenes and a complete output.");
        roomFrame.CopyTo(pixels);
        for (int y = FileSelectMapDefinitions.EntryWindowTop; y < FrontendFrame.Height - FileSelectMapDefinitions.EntryWindowTop; y++)
        {
            int offset = y * FrontendFrame.Width + FileSelectMapDefinitions.EntryWindowLeft;
            area.Slice(offset, FileSelectMapDefinitions.EntryWindowRight - FileSelectMapDefinitions.EntryWindowLeft + 1)
                .CopyTo(pixels.AsSpan(offset));
        }
        return pixels;
    }

    /// <summary>As above, writing into <paramref name="pixels"/>, which must not alias either input.</summary>
    public static RgbaPixel[] Composite(ReadOnlySpan<RgbaPixel> area, ReadOnlySpan<RgbaPixel> roomFrame,
        FileSelectMapWindow window, RgbaPixel[] pixels)
    {
        Ensure.NotNull(window);
        Ensure.NotNull(pixels);
        int length = FrontendFrame.Width * FrontendFrame.Height;
        if (area.Length != length || roomFrame.Length != length || pixels.Length != length)
            throw new ArgumentException("File-select map windows require two complete 256x224 scenes and a complete output.");
        if (window.IsComplete)
        {
            (window.IsReturning ? area : roomFrame).CopyTo(pixels);
            return pixels;
        }
        area.CopyTo(pixels);
        // HDMA encodes the top rows, then max(1,bottom-top) rows using the window.
        // WH0/WH1 include both horizontal endpoints, even for a zero-width window.
        Position16<ScreenPixels> topLeft = window.TopLeft, bottomRight = window.BottomRight;
        int top = (byte)topLeft.Y;
        int bottom = Math.Min(FrontendFrame.Height,
            top + Math.Max(1, (int)unchecked((byte)(bottomRight.Y - topLeft.Y))));
        int left = (byte)topLeft.X;
        int right = (byte)bottomRight.X;
        if (left > right) return pixels;
        for (int y = top; y < bottom; y++)
        {
            int offset = y * FrontendFrame.Width + left;
            roomFrame.Slice(offset, right - left + 1).CopyTo(pixels.AsSpan(offset));
        }
        return pixels;
    }
}
