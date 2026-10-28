using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Map OBJ sheet with procedural reserve bars and selector borders and independently authored pixel edits.</summary>
internal sealed class MapObjectTileArtwork
{
    /// <summary>$B6:C8E0..C9FF, menu tiles47..4F: six partial bars, empty, full and cap.</summary>
    internal const int FirstReserveTile = MapMarkerCellDefinitions.FirstReserveCell, ReserveTileCount = 9;
    /// <summary>$B6:CB60..CBDf, tiles5B..5E: selector left corner, horizontal line and right ends.</summary>
    internal const int FirstHighlightTile = MapMarkerCellDefinitions.FirstHighlightCell, HighlightTileCount = 4;
    /// <summary>Shared reserve cursor46 atB6:C8C0, reused in the left selector corner with swapped colors1/2.</summary>
    private const int CursorTile = MapMarkerCellDefinitions.ReserveCursorCell;
    /// <summary>Unmodeled OBJ characters retained as encoded 4bpp source data.</summary>
    internal readonly byte[] otherCharacters;
    /// <summary>Calculates menu's small-letter glyph characters.</summary>
    private readonly MenuSmallFontArtwork smallFont;
    /// <summary>Calculates large title glyph characters.</summary>
    private readonly MenuLargeFontArtwork largeFont;
    /// <summary>Calculates the compact elevator label characters.</summary>
    private readonly MenuCompactLetteringArtwork elevatorLettering;
    /// <summary>Calculates menu panel art from editable pixels.</summary>
    private readonly MenuPanelTileArtwork panel;
    /// <summary>Calculates L/R button artwork.</summary>
    private readonly MenuShoulderButtonArtwork shoulderButtons;
    /// <summary>Calculates the selected shoulder-button frame.</summary>
    private readonly MenuShoulderHighlightArtwork shoulderHighlight;
    /// <summary>Calculates map marker OBJ characters.</summary>
    private readonly MapMarkerTileArtwork markers;
    /// <summary>Calculates beveled selector square pieces.</summary>
    private readonly MenuBeveledSquareArtwork squares;
    /// <summary>Calculates thin menu border segments.</summary>
    private readonly MenuThinBorderArtwork thinBorder;
    /// <summary>Reserve-bar pixels differing from their procedural fill and border.</summary>
    internal readonly Dictionary<int, byte>? reserveEdits;
    /// <summary>Selector-border pixels differing from its cursor-derived or straight-edge form.</summary>
    internal readonly Dictionary<int, byte>? highlightEdits;

    /// <summary>Partitions an imported OBJ sheet into retained characters and calculated artwork with pixel edits.</summary>
    internal MapObjectTileArtwork(IndexedPngImage image)
    {
        byte[] encoded = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        smallFont = new(image);
        largeFont = new(image);
        elevatorLettering = new(image);
        panel = new(image);
        shoulderButtons = new(image);
        shoulderHighlight = new(image);
        markers = new(image);
        squares = new(image);
        thinBorder = new(image);
        otherCharacters = new byte[encoded.Length - (ReserveTileCount + HighlightTileCount + MenuSmallFontArtwork.TileCount + MenuLargeFontArtwork.TileCount + MenuCompactLetteringArtwork.TileCount + MenuPanelTileArtwork.TileCount + MenuShoulderButtonArtwork.TileCount + MenuShoulderHighlightArtwork.TileCount + MapMarkerTileArtwork.TileCount + MenuBeveledSquareArtwork.TileCount + MenuThinBorderArtwork.TileCount) * 32];
        int stored = 0;
        for (int tile = 0; tile < encoded.Length / 32; tile++)
        {
            if (IsReserve(tile) || IsHighlight(tile) || MenuSmallFontArtwork.Contains(tile) || MenuLargeFontArtwork.Contains(tile) || MenuCompactLetteringArtwork.Contains(tile) || MenuPanelTileArtwork.Contains(tile) || MenuShoulderButtonArtwork.Contains(tile) || MenuShoulderHighlightArtwork.Contains(tile) || MapMarkerTileArtwork.Contains(tile) || MenuBeveledSquareArtwork.Contains(tile) || MenuThinBorderArtwork.Contains(tile)) continue;
            encoded.AsSpan(tile * 32, 32).CopyTo(otherCharacters.AsSpan(stored, 32));
            stored += 32;
        }
        for (int tile = FirstReserveTile; tile < FirstReserveTile + ReserveTileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
            if (pixel != ReservePixel(tile, new(checked((ushort)x), checked((ushort)y))))
                (reserveEdits ??= new()).Add((tile - FirstReserveTile) * 64 + y * 8 + x, (byte)pixel);
        }
        for (int tile = FirstHighlightTile; tile < FirstHighlightTile + HighlightTileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int pixel = image.Pixels[(tile / 16 * 8 + y) * image.Size.X + tile % 16 * 8 + x];
            if (pixel != HighlightPixel(tile, new(checked((ushort)x), checked((ushort)y))))
                (highlightEdits ??= new()).Add((tile - FirstHighlightTile) * 64 + y * 8 + x, (byte)pixel);
        }
    }

    /// <summary>Identifies reserve-bar characters within the map OBJ sheet.</summary>
    private static bool IsReserve(int tile) => tile is >= FirstReserveTile and < (FirstReserveTile + ReserveTileCount);
    /// <summary>Identifies selector-border characters within the map OBJ sheet.</summary>
    private static bool IsHighlight(int tile) => tile is >= FirstHighlightTile and < (FirstHighlightTile + HighlightTileCount);

    /// <summary>Border index12; left corner reuses cursor46 shifted right with colors1/2 exchanged.
    /// Source cursor edits are compensated by independent corner-pixel edits captured at import.</summary>
    internal byte HighlightPixel(int tile, Position16<CharacterPixels> point)
    {
        if (!IsHighlight(tile) || (uint)point.X >= 8 || (uint)point.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        if (tile == FirstHighlightTile)
        {
            if (point.X == 0 || point.Y == 0) return 12;
            int pixel = 0;
            int cursorOffset = 0;
            for (int sourceTile = 0; sourceTile < CursorTile; sourceTile++)
                if (!IsReserve(sourceTile) && !IsHighlight(sourceTile) && !MenuSmallFontArtwork.Contains(sourceTile) && !MenuLargeFontArtwork.Contains(sourceTile) && !MenuCompactLetteringArtwork.Contains(sourceTile) && !MenuPanelTileArtwork.Contains(sourceTile) && !MenuShoulderButtonArtwork.Contains(sourceTile) && !MenuShoulderHighlightArtwork.Contains(sourceTile) && !MapMarkerTileArtwork.Contains(sourceTile) && !MenuBeveledSquareArtwork.Contains(sourceTile) && !MenuThinBorderArtwork.Contains(sourceTile)) cursorOffset += 32;
            for (int plane = 0; plane < 4; plane++)
                pixel |= ((otherCharacters[cursorOffset + plane / 2 * 16 + point.Y * 2 + plane % 2] >> (8 - point.X)) & 1) << plane;
            return (byte)(pixel == 1 ? 2 : pixel == 2 ? 1 : pixel);
        }
        if (tile == FirstHighlightTile + 1) return point.Y == 0 ? (byte)12 : (byte)0;
        return (point.Y == 0 && point.X < 2) || (tile == FirstHighlightTile + 2 && point.X == 1) ? (byte)12 : (byte)0;
    }
    /// <summary>Calculates the native border, fill and empty-interior indices of one reserve pixel.</summary>
    internal static byte ReservePixel(int tile, Position16<CharacterPixels> point)
    {
        if ((uint)(tile - FirstReserveTile) >= ReserveTileCount || (uint)point.X >= 8 || (uint)point.Y >= 8)
            throw new ArgumentOutOfRangeException(nameof(tile));
        if (point.Y < 2) return 0;
        if (tile == FirstReserveTile + 8) return point.X == 0 ? (byte)5 : (byte)0;
        if (point.Y is 2 or 7 || point.X == 0) return 5;
        int fill = tile < FirstReserveTile + 6 ? tile - FirstReserveTile + 1 : tile == FirstReserveTile + 6 ? 0 : 7;
        return point.X <= fill ? (byte)9 : (byte)14;
    }

    /// <summary>The menu's OBJ characters as a sprite page (#1358): character 0 is the first map-object character.</summary>
    internal SpriteArtwork Page() => new(new SpriteArtworkId(SpriteArtworkKind.MenuObjects), Encode());

    /// <summary>Encodes retained and calculated menu characters, in OBJ name order.</summary>
    internal byte[] Encode()
    {
        var transfer = new byte[MapSpriteFormat.ByteCount];
        int stored = 0;
        for (int tile = 0; tile < MapSpriteFormat.ByteCount / 32; tile++)
        {
            bool reserve = IsReserve(tile);
            bool font = MenuSmallFontArtwork.Contains(tile);
            bool title = MenuLargeFontArtwork.Contains(tile);
            bool lettering = MenuCompactLetteringArtwork.Contains(tile);
            bool panelTile = MenuPanelTileArtwork.Contains(tile);
            bool shoulder = MenuShoulderButtonArtwork.Contains(tile);
            bool shoulderFrame = MenuShoulderHighlightArtwork.Contains(tile);
            bool marker = MapMarkerTileArtwork.Contains(tile);
            bool square = MenuBeveledSquareArtwork.Contains(tile);
            bool thin = MenuThinBorderArtwork.Contains(tile);
            if (!reserve && !IsHighlight(tile) && !font && !title && !lettering && !panelTile && !shoulder && !shoulderFrame && !marker && !square && !thin)
            {
                otherCharacters.AsSpan(stored, 32).CopyTo(transfer.AsSpan(tile * 32, 32));
                stored += 32;
                continue;
            }
            var edits = reserve ? reserveEdits : highlightEdits;
            int first = reserve ? FirstReserveTile : FirstHighlightTile;
            for (int y = 0; y < 8; y++)
            for (int plane = 0; plane < 4; plane++)
            {
                int encoded = 0;
                for (int x = 0; x < 8; x++)
                {
                    int key = (tile - first) * 64 + y * 8 + x;
                    int pixel = thin ? thinBorder.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : square ? squares.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : marker ? markers.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : shoulderFrame ? shoulderHighlight.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : shoulder ? shoulderButtons.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : panelTile ? panel.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : lettering ? elevatorLettering.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : title ? largeFont.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : font ? smallFont.Pixel(tile, new(checked((ushort)x), checked((ushort)y))) : edits is not null && edits.ContainsKey(key)
                        ? edits[key] : reserve ? ReservePixel(tile, new(checked((ushort)x), checked((ushort)y))) : HighlightPixel(tile, new(checked((ushort)x), checked((ushort)y)));
                    encoded |= ((pixel >> plane) & 1) << (7 - x);
                }
                transfer[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] = (byte)encoded;
            }
        }
        return transfer;
    }
}
