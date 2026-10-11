using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

public sealed partial class FileSelectAreaMapGraphics
{
    /// <summary>Captures BG1/subscreen math followed by labels, preserving OBJ exclusion from color addition.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true)
    {
        OamBuffer oam = PrepareLabels(usedStationMasks);
        var foreground = new Bg4BppRenderLayer(MenuPpuState.Bg1TilemapWord, 0, new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null);
        return new(PpuMemorySnapshot.Capture(Vram, Cgram, oam), new RenderLayer[] {
            foreground,
            new BgSubscreenAddRenderLayer(FileSelectMapRomData.AreaBackgroundVram,
                FileSelectMapRomData.AreaBackgroundCharacters, includeBackdropInColorMath ? null : foreground),
            new ObjRenderLayer() }, FileSelectMapRomData.ObjectSelection, SnesPpuLayout.MaximumMasterBrightness);
    }
}

public sealed partial class FileSelectRoomMapGraphics
{
    /// <summary>Captures the installed room map/frame and optional marker/arrows without advancing any animation.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot(Position16<MapPixels> scroll = default,
        FileSelectStationMarker? marker = null, FileSelectMapAnimations? animations = null, bool frameOnly = false)
    {
        var layers = new List<RenderLayer>();
        if (frameOnly)
            layers.Add(new Bg4BppRenderLayer(MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters, new Position16<BgPlanePixels>(0, FileSelectMapRomData.RoomFrameVerticalScroll), new Extent16<TilemapCells>(32, 32), null));
        else
            foreach (bool priority in new[] { false, true })
            {
                layers.Add(new Bg4BppRenderLayer(MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters, new Position16<BgPlanePixels>(0, FileSelectMapRomData.RoomFrameVerticalScroll), new Extent16<TilemapCells>(32, 32), priority));
                layers.Add(new Bg4BppRenderLayer(MenuPpuState.Bg1TilemapWord, FileSelectMapRomData.RoomCharacters,
                    scroll.AsBgPlaneScroll(), new Extent16<TilemapCells>(64, 32), priority));
            }
        OamBuffer oam;
        if (marker is not null && !frameOnly)
        {
            oam = PrepareIcons(scroll, marker, animations);
            layers.Add(new ObjRenderLayer());
        }
        else { oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame(); }
        return new(PpuMemorySnapshot.Capture(Vram, Cgram, oam), layers.ToArray(), FileSelectMapRomData.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness);
    }
}
