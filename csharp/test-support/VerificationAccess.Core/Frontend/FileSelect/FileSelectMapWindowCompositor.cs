using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The file-select map's reveals composed from whole scenes for focused checks: the area scene inside the entry's centered
/// rectangle, and the room frame inside a map reveal's rectangle over the area scene (#1555).
/// </summary>
public static class FileSelectMapWindowCompositor
{
    /// <summary>As above, writing into <paramref name="pixels"/>, which must not alias either input.</summary>
    public static RgbaPixel[] CompositeInitialEntryWindow(ReadOnlySpan<RgbaPixel> area, ReadOnlySpan<RgbaPixel> roomFrame, RgbaPixel[] pixels)
    {
        int length = SuperMetroid.Core.Spatial.ScreenGeometry.ScreenWidthPixels * FrontendFrame.Height;
        if (area.Length != length || roomFrame.Length != length || pixels.Length != length)
            throw new ArgumentException("File-select entry windows require two complete scenes and a complete output.");
        roomFrame.CopyTo(pixels);
        for (int y = FileSelectMapDefinitions.EntryWindowTop; y < FrontendFrame.Height - FileSelectMapDefinitions.EntryWindowTop; y++)
        {
            int offset = y * SuperMetroid.Core.Spatial.ScreenGeometry.ScreenWidthPixels + FileSelectMapDefinitions.EntryWindowLeft;
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
        int length = SuperMetroid.Core.Spatial.ScreenGeometry.ScreenWidthPixels * FrontendFrame.Height;
        if (area.Length != length || roomFrame.Length != length || pixels.Length != length)
            throw new ArgumentException("File-select map windows require two complete 256x224 scenes and a complete output.");
        if (window.IsComplete)
        {
            (window.IsReturning ? area : roomFrame).CopyTo(pixels);
            return pixels;
        }
        area.CopyTo(pixels);
        // The room frame shows inside the reveal's rectangle, its right and bottom edges the first column and line past it.
        if (window.IsEmpty) return pixels;
        for (int y = window.Top; y < window.Bottom; y++)
        {
            int offset = y * SuperMetroid.Core.Spatial.ScreenGeometry.ScreenWidthPixels + window.Left;
            roomFrame.Slice(offset, window.Right - window.Left).CopyTo(pixels.AsSpan(offset));
        }
        return pixels;
    }
}
