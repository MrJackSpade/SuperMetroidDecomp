using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectMapWindowCompositor"/> members production does not use.</summary>
internal static class FileSelectMapWindowCompositorAccess
{
    /// <summary>Supplies a fresh full-screen buffer for focused window composition checks.</summary>
    extension(FileSelectMapWindowCompositor)
    {
        /// <summary>
        /// Keeps the area scene outside the window and the empty room frame inside it.
        /// Inputs must already have their own color math applied; this does not scale pixels.
        /// </summary>
        internal static RgbaPixel[] Composite(ReadOnlySpan<RgbaPixel> area, ReadOnlySpan<RgbaPixel> roomFrame,
            FileSelectMapWindow window) =>
            FileSelectMapWindowCompositor.Composite(area, roomFrame, window, new RgbaPixel[FrontendFrame.Width * FrontendFrame.Height]);
    }
}
