using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Room-select BG1 map and BG2 frame installed by $81:A725 and $82:9517.</summary>
/// <remarks>Owns graphics only; selection, scrolling, windows and station markers belong to the menu state.</remarks>
public sealed partial class FileSelectRoomMapGraphics
{
    private readonly ISnesAddressSpace bus;
    private readonly MenuPpuState ppu;
    private readonly FileSelectMapIcons icons;
    public SnesVram Vram => ppu.Vram;
    public SnesCgram Cgram => ppu.Cgram;
    internal Bank80SystemState MapSystem => icons.MapSystem;

    public FileSelectRoomMapGraphics(ISnesAddressSpace bus, Bank80SystemState system, AreaId area,
        MapRevealMode revealMode = MapRevealMode.None, AreaMapPresentationCatalog? mapPresentation = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(system);
        this.bus = bus;
        icons = new FileSelectMapIcons(bus, system, area);
        icons.BindStations(mapPresentation?.Stations);
        int index = AreaIds.ToIndex(area);
        if (index >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        ppu = new MenuPpuState(bus, mapPresentation?.Tiles, mapPresentation?.Palettes);
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        ppu.Vram.LoadBytes(MenuPpuState.Bg1TilemapWord * 2,
            AreaMapTilemapBuilder.Build(mapPresentation?.Get(area) ?? AreaMapRomData.Load(bus, area), system, hidden, revealMode));

        var frame = new byte[FileSelectMapRomData.TilemapBytes];
        RomDataReader.ReadFixedBank(bus, FileSelectMapRomData.RoomFrame, 1600).CopyTo(frame, 0);
        for (int word = 800; word < frame.Length / 2; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(word * 2), FileSelectMapRomData.RoomFrameBlank);
        // Native copies backwards from footer word 160 through word 1, not word 0.
        RomDataReader.ReadFixedBank(bus, FileSelectMapRomData.RoomFrameFooter + 2, 320).CopyTo(frame, 1600);
        ushort label = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.RoomLabelPointers + index * 2);
        for (int word = 0; word < 12; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan((170 + word) * 2),
                (ushort)(RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.MenuObjectBank | (label + word * 2))
                    & FileSelectMapRomData.RoomLabelMask));
        ppu.Vram.LoadBytes(MenuPpuState.Bg2TilemapWord * 2, frame);
    }

    /// <summary>Reprojects current host artwork without resetting menu animation or scroll state.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        icons.BindStations(catalog?.Stations);
        if (catalog is not null)
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                if (color < MapAnimationRomData.PaletteDestination || color >= MapAnimationRomData.PaletteDestination + MapPaletteCycleFormat.ColorCount)
                    Cgram.SetColor(color, catalog.Palettes.FileSelect[color]);
        if (catalog is not null) catalog.Tiles.LoadTo(Vram, FileSelectMapRomData.RoomCharacters * 2);
        else Vram.LoadBytes(FileSelectMapRomData.RoomCharacters * 2,
            RomDataReader.ReadFixedBank(bus, MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount));
        var system = icons.MapSystem;
        var area = icons.MapArea;
        MapTileWord hidden = system.HasAreaMap(area)
            ? MapTileWords.PauseBlank : MapTileWords.FileSelectUndownloadedBlank;
        ppu.Vram.LoadBytes(MenuPpuState.Bg1TilemapWord * 2,
            AreaMapTilemapBuilder.Build(catalog?.Get(area) ?? AreaMapRomData.Load(bus, area), system, hidden));
    }

    /// <summary>BG2-only endpoint of $81:AC2D; room-map cells are not installed until $81:AD17.</summary>
    public Rgba32[] RenderFrameOnly()
    {
        var pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        Array.Fill(pixels, Cgram.GetRgba(0));
        SnesLayerCompositor.Composite(pixels, SnesBgTilemapRenderer.Render4BppViewport(
            Vram, Cgram, MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters,
            0, 24, FrontendFrame.Width, FrontendFrame.Height, 32, 32));
        return pixels;
    }

    /// <summary>Draws the saved-station marker over the room map without advancing its animation.</summary>
    public Rgba32[] Render(ushort horizontalScroll, ushort verticalScroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations = null)
    {
        ArgumentNullException.ThrowIfNull(marker);
        Rgba32[] pixels = RenderBackgrounds(horizontalScroll, verticalScroll);
        OamBuffer oam = PrepareIcons(horizontalScroll, verticalScroll, marker, animations);
        SnesLayerCompositor.Composite(pixels, SnesObjRenderer.Render(oam, Vram, Cgram, obsel: 0x03));
        return pixels;
    }

    private OamBuffer PrepareIcons(ushort horizontalScroll, ushort verticalScroll, FileSelectStationMarker marker,
        FileSelectMapAnimations? animations)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        icons.DrawBeforeMarker(oam, horizontalScroll, verticalScroll);
        marker.Draw(bus, oam, horizontalScroll, verticalScroll);
        icons.DrawAfterMarker(oam, horizontalScroll, verticalScroll,
            animations is null ? null : () => animations.DrawArrows(oam));
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Renders Mode-1 BG priorities with independent scrolling for map and fixed frame.</summary>
    public Rgba32[] RenderBackgrounds(ushort horizontalScroll, ushort verticalScroll)
    {
        var pixels = new Rgba32[256 * 224];
        Array.Fill(pixels, ppu.Cgram.GetRgba(0));
        foreach (bool priority in new[] { false, true })
        {
            // Mode 1 orders BG2 before BG1 within each background priority tier.
            SnesLayerCompositor.Composite(pixels, SnesBgTilemapRenderer.Render4BppViewport(
                Vram, Cgram, MenuPpuState.Bg2TilemapWord, FileSelectMapRomData.RoomCharacters,
                0, 24, 256, 224, 32, 32, priority: priority));
            SnesLayerCompositor.Composite(pixels, SnesBgTilemapRenderer.Render4BppViewport(
                Vram, Cgram, MenuPpuState.Bg1TilemapWord, FileSelectMapRomData.RoomCharacters,
                horizontalScroll, verticalScroll, 256, 224, priority: priority));
        }
        return pixels;
    }
}
