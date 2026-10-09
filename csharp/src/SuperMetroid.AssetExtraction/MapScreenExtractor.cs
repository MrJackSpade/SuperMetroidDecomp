using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Resolves native world layers and final room-map frames into editable visual resources.</summary>
public static class MapScreenExtractor
{
    /// <summary>Builds the thirteen final world-map/room-frame tilemap pages and exports their shared foreground/background character atlases.</summary>
    /// <param name="bus">Import-capable cartridge source for bank-$81/$82 menu layers, room-frame header/footer/labels, and bank-$8E planar characters.</param>
    /// <returns>A new filename-keyed dictionary of owned buffers: versioned map-screen JSON, a 128-by-344 four-bit foreground PNG, and a 128-by-48 two-bit background PNG.</returns>
    /// <remarks>Each page contains 32-by-32 tile cells with atlas coordinates, palette, priority, and flips. Room frames include native blank fill, footer words 1..160, and masked area labels; returned diagnostic PNG palettes are not runtime colors. No files are written and navigation/discovery data is excluded.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    public static Dictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        var pages = new Dictionary<string, MapPresentationCell[]>();
        Add(MapScreenDefinitions.WorldForeground, RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.AreaForeground, MapScreenDefinitions.PageBytes), WorldMapArtworkFormat.TileColumns);
        for (int index = 0; index < MapScreenDefinitions.ZebesAreas; index++)
        {
            var area = (AreaId)index;
            Add(MapScreenDefinitions.WorldBackground(area), RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                FileSelectMapRomData.AreaBackgrounds + index * MapScreenDefinitions.PageBytes, MapScreenDefinitions.PageBytes), WorldMapArtworkFormat.TileColumns);
            var frame = new byte[MapScreenDefinitions.PageBytes];
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.RoomFrame, FileSelectMapRomData.RoomFrameHeaderWords * 2).CopyTo(frame, 0);
            for (int word = FileSelectMapRomData.RoomFrameHeaderWords; word < MapScreenDefinitions.PageCells; word++)
                BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(word * 2), FileSelectMapRomData.RoomFrameBlank);
            // Native's reverse copy supplies footer words 1..160, not 0..159.
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.RoomFrameFooter + 2, FileSelectMapRomData.RoomFrameFooterWords * 2).CopyTo(frame, FileSelectMapRomData.RoomFrameHeaderWords * 2);
            ushort label = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.RoomLabelPointers + index * 2);
            for (int word = 0; word < FileSelectMapRomData.RoomLabelWords; word++)
                BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan((FileSelectMapRomData.RoomLabelDestinationWord + word) * 2),
                    (ushort)(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.MenuObjectBank | (label + word * 2)) & FileSelectMapRomData.RoomLabelMask));
            Add(MapScreenDefinitions.RoomFrame(area), frame, MapTileAtlasFormat.TileColumns);
        }
        using var json = new MemoryStream();
        MapScreenPresentation.Write(json, new() { Version = MapScreenDefinitions.Version, Pages = pages });
        return new()
        {
            [MapScreenDefinitions.FileName] = json.ToArray(),
            [WorldMapArtworkFormat.ForegroundFile] = Atlas(WorldMapArtworkFormat.ForegroundSource, WorldMapArtworkFormat.ForegroundBytes, 4),
            [WorldMapArtworkFormat.BackgroundFile] = Atlas(WorldMapArtworkFormat.BackgroundSource, WorldMapArtworkFormat.BackgroundBytes, 2)
        };

        void Add(string name, byte[] bytes, int columns)
        {
            var cells = new MapPresentationCell[MapScreenDefinitions.PageCells];
            for (int index = 0; index < cells.Length; index++)
            {
                var word = new MapTileWord(BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(index * 2)));
                cells[index] = new() { TileColumn = word.CharacterIndex % columns, TileRow = word.CharacterIndex / columns,
                    Palette = word.PaletteIndex, Priority = word.HasPriority,
                    FlipX = (word.Raw & MapPresentationFormat.FlipXBit) != 0, FlipY = (word.Raw & MapPresentationFormat.FlipYBit) != 0 };
            }
            pages.Add(name, cells);
        }
        byte[] Atlas(int address, int count, int bpp)
        {
            byte[] pixels = SnesGraphics.DecodePlanarTiles(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), address, count), bpp,
                WorldMapArtworkFormat.TileColumns, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(1 << bpp));
            return png.ToArray();
        }
    }
}
