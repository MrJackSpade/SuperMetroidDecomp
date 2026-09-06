using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>Captures the shared menu compositor without retaining menu state.</summary>
internal static class MenuRenderSnapshotCapture
{
    internal static LayeredRenderSnapshot Capture(MenuPpuState ppu, OamBuffer oam,
        ushort bg1VerticalScroll, byte brightness)
    {
        // These menus currently compose entire BG2, BG1 and OBJ planes in that order.
        // Preserve that established path here; do not silently replace it with the
        // gameplay priority ladder while moving composition to another backend.
        RenderLayer[] layers =
        [
            new Bg4BppRenderLayer(MenuPpuState.Bg2TilemapWord, 0, 0, 0, 32, 32, null),
            new Bg4BppRenderLayer(MenuPpuState.Bg1TilemapWord, 0, 0, bg1VerticalScroll, 32, 32, null),
            new ObjRenderLayer(),
        ];
        return new(PpuMemorySnapshot.Capture(ppu.Vram, ppu.Cgram, oam), layers,
            MenuRenderDefinitions.ObjectSelection, brightness);
    }
}
