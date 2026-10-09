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

    /// <summary>
    /// Composes the area and room snapshots for the current map-window edges. A completed
    /// expansion selects the room, a completed return selects the area, and an active valid
    /// rectangle inserts the room scene within the area scene.
    /// </summary>
    /// <param name="areaScene">Area-map snapshot used as the visible base during the transition.</param>
    /// <param name="roomScene">Room snapshot revealed inside the moving window.</param>
    /// <param name="window">Current transition state providing completion, direction, and edge positions.</param>
    /// <returns>The selected complete scene or a snapshot containing the clipped room insertion.</returns>
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

    /// <summary>
    /// Adds a child-scene layer over the basis snapshot while retaining the basis memory,
    /// object selection, and brightness; child pixels replace the basis within the rectangle.
    /// </summary>
    /// <param name="basis">Snapshot providing the parent memory and layers outside the rectangle.</param>
    /// <param name="inside">Snapshot whose scene is shown within the rectangle.</param>
    /// <param name="left">Inclusive left screen-pixel edge of the child scene.</param>
    /// <param name="top">Inclusive top scanline of the child scene.</param>
    /// <param name="right">Exclusive right screen-pixel edge of the child scene.</param>
    /// <param name="bottom">Exclusive bottom scanline of the child scene.</param>
    /// <returns>A snapshot with the child scene inserted as its final render layer.</returns>
    private static LayeredRenderSnapshot Insert(LayeredRenderSnapshot basis, LayeredRenderSnapshot inside,
        int left, int top, int right, int bottom)
    {
        RenderLayer[] layers = [.. basis.Layers, new WindowedSceneRenderLayer(inside, left, top, right, bottom)];
        return new(basis.Memory, layers, basis.ObjectSelection, basis.Brightness);
    }
}
