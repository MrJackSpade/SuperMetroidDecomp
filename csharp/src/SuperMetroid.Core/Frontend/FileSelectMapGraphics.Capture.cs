using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

public sealed partial class FileSelectAreaMapGraphics
{
    /// <summary>Captures BG1/subscreen math followed by labels, preserving OBJ exclusion from color addition.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot(ReadOnlySpan<ushort> usedStationMasks, bool includeBackdropInColorMath = true)
    {
        OamBuffer oam = PrepareLabels(usedStationMasks);
        var foreground = new Bg4BppRenderLayer(MenuPpuState.Bg1TilemapWord, 0, 0, 0, 32, 32, null);
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
    public LayeredRenderSnapshot CaptureRenderSnapshot(ushort horizontalScroll = 0, ushort verticalScroll = 0,
        FileSelectStationMarker? marker = null, FileSelectMapAnimations? animations = null, bool frameOnly = false)
    {
        var layers = new List<RenderLayer>();
        if (frameOnly)
            layers.Add(new Bg4BppRenderLayer(MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters, 0,
                FileSelectMapRomData.RoomFrameVerticalScroll, 32, 32, null));
        else
            foreach (bool priority in new[] { false, true })
            {
                layers.Add(new Bg4BppRenderLayer(MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters, 0,
                    FileSelectMapRomData.RoomFrameVerticalScroll, 32, 32, priority));
                layers.Add(new Bg4BppRenderLayer(MenuPpuState.Bg1TilemapWord, FileSelectMapRomData.RoomCharacters,
                    horizontalScroll, verticalScroll, 64, 32, priority));
            }
        OamBuffer oam;
        if (marker is not null && !frameOnly)
        {
            oam = PrepareIcons(horizontalScroll, verticalScroll, marker, animations);
            layers.Add(new ObjRenderLayer());
        }
        else { oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame(); }
        return new(PpuMemorySnapshot.Capture(Vram, Cgram, oam), layers.ToArray(), FileSelectMapRomData.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness);
    }
}
