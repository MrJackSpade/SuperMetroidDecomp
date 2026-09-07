using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

public sealed partial class FileSelectMapMenuState
{
    /// <summary>Captures both map-memory owners and resolved window edges at the display boundary.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        LayeredRenderSnapshot Area(bool backdrop = true) => areaGraphics.CaptureRenderSnapshot(usedStations, backdrop);
        LayeredRenderSnapshot Frame() => roomGraphics.CaptureRenderSnapshot(frameOnly: true);
        LayeredRenderSnapshot Room(bool icons = true) => roomGraphics.CaptureRenderSnapshot(scroll.Horizontal, scroll.Vertical,
            icons ? marker : null, drawArrows ? animations : null);
        if (!entry.IsComplete)
        {
            LayeredRenderSnapshot areaScene = Area();
            ushort[] palette = areaScene.Memory.Cgram.ToArray(); palette[0] = 0;
            var black = new LayeredRenderSnapshot(new PpuMemorySnapshot(areaScene.Memory.Vram, palette,
                areaScene.Memory.Oam, 0), [], areaScene.ObjectSelection, SnesPpuLayout.MaximumMasterBrightness);
            return entry.Phase == FileSelectMapEntryPhase.Revealing
                ? Insert(black, areaScene, entry.Left, entry.Top, entry.Right + 1, entry.Bottom) : black;
        }
        LayeredRenderSnapshot scene = Phase switch
        {
            FileSelectMapNavigationPhase.ExpandingWindow or FileSelectMapNavigationPhase.InitializingRoom =>
                Window(Area(false), Frame(), navigation.Window!),
            FileSelectMapNavigationPhase.Room when !markerDrawn => Room(false),
            FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested => Room(),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == 0 => Room(),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == FileSelectMapRomData.ReturnSetupFrames - 1 =>
                Insert(Frame(), Area(), FileSelectMapRomData.EntryWindowLeft, FileSelectMapRomData.EntryWindowTop,
                    FileSelectMapRomData.EntryWindowRight + 1, SnesPpuLayout.ScreenHeightPixels - FileSelectMapRomData.EntryWindowTop),
            FileSelectMapNavigationPhase.AreaReturnRequested when returnWindow is not null => Window(Area(), Frame(), returnWindow),
            FileSelectMapNavigationPhase.AreaReturnRequested => Frame(),
            _ => Area(),
        };
        return new(scene.Memory, scene.Layers, scene.ObjectSelection, brightness);
    }

    private static LayeredRenderSnapshot Window(LayeredRenderSnapshot areaScene, LayeredRenderSnapshot roomScene, FileSelectMapWindow window)
    {
        if (window.IsComplete) return window.IsReturning ? areaScene : roomScene;
        int top = (byte)window.Top;
        int bottom = Math.Min(SnesPpuLayout.ScreenHeightPixels, top + Math.Max(1, (int)unchecked((byte)(window.Bottom - window.Top))));
        int left = (byte)window.Left, right = (byte)window.Right;
        // The legacy HDMA projection performs no row copies for a wholly off-screen
        // interval or crossed horizontal edges; resolve that before contract validation.
        if (left > right || top >= bottom) return areaScene;
        return Insert(areaScene, roomScene, left, top, right + 1, bottom);
    }

    private static LayeredRenderSnapshot Insert(LayeredRenderSnapshot basis, LayeredRenderSnapshot inside,
        int left, int top, int right, int bottom)
    {
        RenderLayer[] layers = [.. basis.Layers, new WindowedSceneRenderLayer(inside, left, top, right, bottom)];
        return new(basis.Memory, layers, basis.ObjectSelection, basis.Brightness);
    }
}
