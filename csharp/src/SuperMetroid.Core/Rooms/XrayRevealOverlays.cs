using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Ports uncollected-item and room-special reveal writes after the terrain pass.</summary>
public static class XrayRevealOverlays
{
    /// <summary>Applies $84:831A in descending PLM slot order, then room-authored record order.</summary>
    public static void Apply(ISnesAddressSpace bus, RoomLevelData level, Span<ushort> tilemap,
        IReadOnlyList<CollectiblePlmSnapshot> items, Bank80SystemState system,
        ushort specialPointer, ushort layer1X, ushort layer1Y,
        XrayRevealVisualCatalog? visuals = null)
    {
        if (tilemap.Length != XrayTilemapLayout.BufferWords)
            throw new ArgumentException("X-ray overlays require both native tilemap screens.", nameof(tilemap));
        if (visuals is null)
            throw new InvalidOperationException("X-ray reveal requires installed overlay visuals.");
        XrayOverlayVisualCatalog overlays = visuals.Overlays ?? throw new InvalidOperationException(
            "X-ray reveal requires installed item and room-overlay definitions.");
        for (int i = items.Count - 1; i >= 0; i--)
        {
            CollectiblePlmSnapshot item = items[i];
            if (unchecked((short)item.RoomArgument) < 0 || system.HasCollectedItemBit(item.RoomArgument)) continue;
            int graphics = item.Kind < InWorldCollectibleKind.Bombs
                ? XrayOverlayRomData.DynamicGraphicsSlots + (int)item.Kind : item.GraphicsSlot;
            if ((uint)graphics >= XrayOverlayRomData.DynamicGraphicsSlots * 2)
                throw new InvalidDataException($"Item ${item.Header:X4} has invalid X-ray graphics slot {graphics}.");
            ushort word = overlays.ItemMetatile(graphics);
            Write(level, tilemap, word, item.BlockIndex % level.WidthInBlocks,
                item.BlockIndex / level.WidthInBlocks, layer1X, layer1Y);
        }
        if (specialPointer == 0) return;
        foreach (XrayRoomOverlayVisual tile in overlays.RoomTiles(specialPointer))
            Write(level, tilemap, tile.Word, tile.X, tile.Y, layer1X, layer1Y);
    }

    /// <summary>Draws one room overlay metatile into the visible X-ray tilemap, preserving the native vertical-flip row swap.</summary>
    /// <param name="level">Room block definitions used to expand the metatile into four tile words.</param>
    /// <param name="tilemap">The two-screen native tilemap buffer to update.</param>
    /// <param name="word">The visual metatile index and flip bits to draw.</param>
    /// <param name="x">The overlay's horizontal room coordinate in metatiles.</param>
    /// <param name="y">The overlay's vertical room coordinate in metatiles.</param>
    /// <param name="layer1X">Horizontal layer scroll in pixels, used to position the overlay on screen.</param>
    /// <param name="layer1Y">Vertical layer scroll in pixels, used to position the overlay on screen.</param>
    /// <exception cref="InvalidDataException">The selected metatile extends beyond the room's block definitions.</exception>
    private static void Write(RoomLevelData level, Span<ushort> tilemap, ushort word, int x, int y,
        ushort layer1X, ushort layer1Y)
    {
        x -= layer1X >> 4;
        y -= layer1Y >> 4;
        // $91:D04C admits only the first 16x16 metatiles, not the fine-scroll column.
        if ((uint)x >= XrayTilemapLayout.MetatileColumns - 1 || (uint)y >= XrayTilemapLayout.MetatileRows) return;
        int metatile = word & 0x03ff;
        int source = metatile * 8;
        ReadOnlySpan<byte> definitions = level.BlockDefinitions.Span;
        if (source + 8 > definitions.Length)
            throw new InvalidDataException($"X-ray overlay metatile ${metatile:X4} is outside room definitions.");
        int destination = y * XrayTilemapLayout.MetatileRowStride + x * 2;
        // $91:D0A6 swaps the two tile rows for vertical flip. It neither toggles each
        // tile's flip bits nor implements horizontal flip; preserve that native behavior.
        bool flip = (word & 0x0800) != 0;
        for (int row = 0; row < 2; row++)
        for (int col = 0; col < 2; col++)
        {
            int offset = source + (flip ? 1 - row : row) * 4 + col * 2;
            tilemap[destination + row * 32 + col] = (ushort)(definitions[offset] | definitions[offset + 1] << 8);
        }
    }

}
