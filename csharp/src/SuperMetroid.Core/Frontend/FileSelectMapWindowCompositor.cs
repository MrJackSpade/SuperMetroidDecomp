using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Opposing BG1/OBJ and BG2 window-one masks installed by $81:AAAC/$81:AFF6.</summary>
public static class FileSelectMapWindowCompositor
{
    /// <summary>
    /// $81:A5B3 temporarily restores the initial centered window during return setup:
    /// area graphics are inside it, while the previously installed BG2 frame stays outside.
    /// </summary>
    public static Rgba32[] CompositeInitialEntryWindow(ReadOnlySpan<Rgba32> area, ReadOnlySpan<Rgba32> roomFrame)
    {
        int length = FrontendFrame.Width * FrontendFrame.Height;
        if (area.Length != length || roomFrame.Length != length)
            throw new ArgumentException("File-select entry windows require two complete scenes.");
        var pixels = roomFrame.ToArray();
        for (int y = FileSelectMapRomData.EntryWindowTop; y < FrontendFrame.Height - FileSelectMapRomData.EntryWindowTop; y++)
        {
            int offset = y * FrontendFrame.Width + FileSelectMapRomData.EntryWindowLeft;
            area.Slice(offset, FileSelectMapRomData.EntryWindowRight - FileSelectMapRomData.EntryWindowLeft + 1)
                .CopyTo(pixels.AsSpan(offset));
        }
        return pixels;
    }

    /// <summary>
    /// Keeps the area scene outside the window and the empty room frame inside it.
    /// Inputs must already have their own color math applied; this does not scale pixels.
    /// </summary>
    public static Rgba32[] Composite(ReadOnlySpan<Rgba32> area, ReadOnlySpan<Rgba32> roomFrame,
        FileSelectMapWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        int length = FrontendFrame.Width * FrontendFrame.Height;
        if (area.Length != length || roomFrame.Length != length)
            throw new ArgumentException("File-select map windows require two complete 256x224 scenes.");
        if (window.IsComplete)
            return window.IsReturning ? area.ToArray() : roomFrame.ToArray();
        Rgba32[] pixels = area.ToArray();
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
