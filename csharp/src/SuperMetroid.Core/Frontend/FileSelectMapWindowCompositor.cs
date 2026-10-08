using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Opposing BG1/OBJ and BG2 window-one masks installed by $81:AAAC/$81:AFF6.</summary>
public static class FileSelectMapWindowCompositor
{
    /// <summary>As above, writing into <paramref name="pixels"/>, which must not alias either input.</summary>
    public static Rgba32[] CompositeInitialEntryWindow(ReadOnlySpan<Rgba32> area, ReadOnlySpan<Rgba32> roomFrame, Rgba32[] pixels)
    {
        int length = FrontendFrame.Width * FrontendFrame.Height;
        if (area.Length != length || roomFrame.Length != length || pixels.Length != length)
            throw new ArgumentException("File-select entry windows require two complete scenes and a complete output.");
        roomFrame.CopyTo(pixels);
        for (int y = FileSelectMapRomData.EntryWindowTop; y < FrontendFrame.Height - FileSelectMapRomData.EntryWindowTop; y++)
        {
            int offset = y * FrontendFrame.Width + FileSelectMapRomData.EntryWindowLeft;
            area.Slice(offset, FileSelectMapRomData.EntryWindowRight - FileSelectMapRomData.EntryWindowLeft + 1)
                .CopyTo(pixels.AsSpan(offset));
        }
        return pixels;
    }

    /// <summary>As above, writing into <paramref name="pixels"/>, which must not alias either input.</summary>
    public static Rgba32[] Composite(ReadOnlySpan<Rgba32> area, ReadOnlySpan<Rgba32> roomFrame,
        FileSelectMapWindow window, Rgba32[] pixels)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(pixels);
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
        int top = (byte)window.Top;
        int bottom = Math.Min(FrontendFrame.Height,
            top + Math.Max(1, (int)unchecked((byte)(window.Bottom - window.Top))));
        int left = (byte)window.Left;
        int right = (byte)window.Right;
        if (left > right) return pixels;
        for (int y = top; y < bottom; y++)
        {
            int offset = y * FrontendFrame.Width + left;
            roomFrame.Slice(offset, right - left + 1).CopyTo(pixels.AsSpan(offset));
        }
        return pixels;
    }
}
