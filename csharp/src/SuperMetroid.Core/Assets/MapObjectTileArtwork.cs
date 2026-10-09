using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Map OBJ sheet with procedural reserve bars and selector borders and independently authored pixel edits.</summary>
internal sealed class MapObjectTileArtwork
{
    /// <summary>$B6:C8E0..C9FF, menu tiles47..4F: six partial bars, empty, full and cap.</summary>
    internal const int FirstReserveTile = 0x47, ReserveTileCount = 9;
    /// <summary>$B6:CB60..CBDf, tiles5B..5E: selector left corner, horizontal line and right ends.</summary>
    internal const int FirstHighlightTile = 0x5b, HighlightTileCount = 4;
    /// <summary>Shared reserve cursor46 atB6:C8C0, reused in the left selector corner with swapped colors1/2.</summary>
    private const int CursorTile = 0x46;
    /// <summary>Planar bytes for OBJ tiles not supplied by a dedicated menu-art renderer or procedural tile rule.</summary>
    private readonly byte[] otherCharacters;
    /// <summary>Pixel source for the small menu font tiles in the object sheet.</summary>
    private readonly MenuSmallFontArtwork smallFont;
    /// <summary>Pixel source for the larger title lettering tiles in the object sheet.</summary>
    private readonly MenuLargeFontArtwork largeFont;
    /// <summary>Pixel source for compact elevator lettering tiles in the object sheet.</summary>
    private readonly MenuCompactLetteringArtwork elevatorLettering;
    /// <summary>Pixel source for the panel corner and edge tiles in the object sheet.</summary>
    private readonly MenuPanelTileArtwork panel;
    /// <summary>Pixel source for the shoulder-button icon tiles in the object sheet.</summary>
    private readonly MenuShoulderButtonArtwork shoulderButtons;
    /// <summary>Pixel source for the highlighted shoulder-button frame tiles.</summary>
    private readonly MenuShoulderHighlightArtwork shoulderHighlight;
    /// <summary>Pixel source for map marker tiles used by the object sheet.</summary>
    private readonly MapMarkerTileArtwork markers;
    /// <summary>Pixel source for beveled square tiles in the object sheet.</summary>
    private readonly MenuBeveledSquareArtwork squares;
    /// <summary>Pixel source for thin border tiles in the object sheet.</summary>
    private readonly MenuThinBorderArtwork thinBorder;
    /// <summary>Imported per-pixel overrides for the otherwise procedural reserve bar tiles; null when source pixels match the rule.</summary>
    private readonly Dictionary<int, byte>? reserveEdits;
    /// <summary>Imported per-pixel overrides for the otherwise procedural selector border tiles; null when source pixels match the rule.</summary>
    private readonly Dictionary<int, byte>? highlightEdits;

    /// <summary>Encodes the indexed OBJ sheet and separates reusable artwork from procedural tiles and authored pixel exceptions.</summary>
    /// <param name="image">Indexed sprite-sheet image supplying the map object's source pixels.</param>
    internal MapObjectTileArtwork(IndexedPngImage image)
    {
        byte[] encoded = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4);
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
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != ReservePixel(tile, x, y))
                (reserveEdits ??= new()).Add((tile - FirstReserveTile) * 64 + y * 8 + x, pixel);
        }
        for (int tile = FirstHighlightTile; tile < FirstHighlightTile + HighlightTileCount; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            byte pixel = image.Pixels[(tile / 16 * 8 + y) * image.Width + tile % 16 * 8 + x];
            if (pixel != HighlightPixel(tile, x, y))
                (highlightEdits ??= new()).Add((tile - FirstHighlightTile) * 64 + y * 8 + x, pixel);
        }
    }

    /// <summary>Reports whether a tile index belongs to the contiguous reserve bar region generated from reserve pixel rules.</summary>
    /// <param name="tile">OBJ tile index in the map sprite sheet.</param>
    /// <returns>True for one of the reserve bar, empty, full, or cap tiles.</returns>
    private static bool IsReserve(int tile) => tile >= FirstReserveTile && tile < FirstReserveTile + ReserveTileCount;

    /// <summary>Reports whether a tile index belongs to the selector border region with procedural pixel rules.</summary>
    /// <param name="tile">OBJ tile index in the map sprite sheet.</param>
    /// <returns>True for one of the four selector corner or edge tiles.</returns>
    private static bool IsHighlight(int tile) => tile >= FirstHighlightTile && tile < FirstHighlightTile + HighlightTileCount;

    /// <summary>Border index12; left corner reuses cursor46 shifted right with colors1/2 exchanged.
    /// Source cursor edits are compensated by independent corner-pixel edits captured at import.</summary>
    internal byte HighlightPixel(int tile, int x, int y)
    {
        if (!IsHighlight(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
        if (tile == FirstHighlightTile)
        {
            if (x == 0 || y == 0) return 12;
            int pixel = 0;
            int cursorOffset = 0;
            for (int sourceTile = 0; sourceTile < CursorTile; sourceTile++)
                if (!IsReserve(sourceTile) && !IsHighlight(sourceTile) && !MenuSmallFontArtwork.Contains(sourceTile) && !MenuLargeFontArtwork.Contains(sourceTile) && !MenuCompactLetteringArtwork.Contains(sourceTile) && !MenuPanelTileArtwork.Contains(sourceTile) && !MenuShoulderButtonArtwork.Contains(sourceTile) && !MenuShoulderHighlightArtwork.Contains(sourceTile) && !MapMarkerTileArtwork.Contains(sourceTile) && !MenuBeveledSquareArtwork.Contains(sourceTile) && !MenuThinBorderArtwork.Contains(sourceTile)) cursorOffset += 32;
            for (int plane = 0; plane < 4; plane++)
                pixel |= ((otherCharacters[cursorOffset + plane / 2 * 16 + y * 2 + plane % 2] >> (8 - x)) & 1) << plane;
            return (byte)(pixel == 1 ? 2 : pixel == 2 ? 1 : pixel);
        }
        if (tile == FirstHighlightTile + 1) return y == 0 ? (byte)12 : (byte)0;
        return (y == 0 && x < 2) || (tile == FirstHighlightTile + 2 && x == 1) ? (byte)12 : (byte)0;
    }
    /// <summary>Calculates the native border, fill and empty-interior indices of one reserve pixel.</summary>
    internal static byte ReservePixel(int tile, int x, int y)
    {
        if ((uint)(tile - FirstReserveTile) >= ReserveTileCount || (uint)x >= 8 || (uint)y >= 8)
            throw new ArgumentOutOfRangeException(nameof(tile));
        if (y < 2) return 0;
        if (tile == FirstReserveTile + 8) return x == 0 ? (byte)5 : (byte)0;
        if (y is 2 or 7 || x == 0) return 5;
        int fill = tile < FirstReserveTile + 6 ? tile - FirstReserveTile + 1 : tile == FirstReserveTile + 6 ? 0 : 7;
        return x <= fill ? (byte)9 : (byte)14;
    }

    /// <summary>Builds the complete map OBJ tile payload from imported and generated artwork and writes it to VRAM.</summary>
    /// <param name="vram">Destination VRAM receiving the encoded tile bytes.</param>
    /// <param name="destinationByte">Byte offset at which the sprite-sheet payload begins.</param>
    internal void LoadTo(SnesVram vram, int destinationByte)
    {
        // This is the immediate transfer payload, never a retained calculated lookup.
        Span<byte> transfer = stackalloc byte[MapSpriteFormat.ByteCount];
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
                otherCharacters.AsSpan(stored, 32).CopyTo(transfer.Slice(tile * 32, 32));
                stored += 32;
                continue;
            }
            var edits = reserve ? reserveEdits : highlightEdits;
            int first = reserve ? FirstReserveTile : FirstHighlightTile;
            for (int y = 0; y < 8; y++)
            for (int plane = 0; plane < 4; plane++)
            {
                byte encoded = 0;
                for (int x = 0; x < 8; x++)
                {
                    int key = (tile - first) * 64 + y * 8 + x;
                    byte pixel = thin ? thinBorder.Pixel(tile, x, y) : square ? squares.Pixel(tile, x, y) : marker ? markers.Pixel(tile, x, y) : shoulderFrame ? shoulderHighlight.Pixel(tile, x, y) : shoulder ? shoulderButtons.Pixel(tile, x, y) : panelTile ? panel.Pixel(tile, x, y) : lettering ? elevatorLettering.Pixel(tile, x, y) : title ? largeFont.Pixel(tile, x, y) : font ? smallFont.Pixel(tile, x, y) : edits is not null && edits.TryGetValue(key, out byte authored)
                        ? authored : reserve ? ReservePixel(tile, x, y) : HighlightPixel(tile, x, y);
                    encoded |= (byte)(((pixel >> plane) & 1) << (7 - x));
                }
                transfer[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] = encoded;
            }
        }
        vram.LoadBytes(destinationByte, transfer);
    }
}
