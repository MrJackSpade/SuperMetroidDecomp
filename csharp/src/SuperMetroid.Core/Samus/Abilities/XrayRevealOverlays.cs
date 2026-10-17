using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Ports uncollected-item and room-special reveal writes after the terrain pass.</summary>
public static class XrayRevealOverlays
{
    /// <summary>Applies $84:831A in descending PLM slot order, then room-authored record order.</summary>
    public static void Apply(RoomLevelData level, Span<ushort> tilemap,
        IReadOnlyList<CollectiblePlmSnapshot> items, Bank80SystemState system,
        ushort specialPointer, Position16<RoomPixels> layer1Position,
        XrayRevealVisualCatalog? visuals = null)
    {
        Ensure.LengthEqual(tilemap, XrayTilemapLayout.BufferWords);
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
                throw new InvalidDataException($"Item ${(int)item.Header:X4} has invalid X-ray graphics slot {graphics}.");
            var metatile = new RoomLevelWord(overlays.ItemMetatile(graphics));
            Write(level, tilemap, metatile, level.Size.CellAtRowMajorIndex(item.BlockIndex), layer1Position);
        }
        if (specialPointer == 0) return;
        foreach (XrayRoomOverlayVisual tile in overlays.RoomTiles(specialPointer))
            Write(level, tilemap, tile.Metatile, tile.Block, layer1Position);
    }

    /// <summary>Copies a visible metatile's four tile words, swapping rows for native vertical flip without changing tile flip bits.</summary>
    private static void Write(RoomLevelData level, Span<ushort> tilemap, RoomLevelWord metatileWord, Position16<RoomBlocks> block,
        Position16<RoomPixels> layer1Position)
    {
        // The overlay's metatile within the visible window, relative to the camera's block.
        Offset16<RoomBlocks> visible = block - layer1Position.ToRoomBlock();
        int x = visible.X, y = visible.Y;
        // $91:D04C admits only the first 16x16 metatiles, not the fine-scroll column.
        if ((uint)x >= XrayTilemapLayout.MetatileColumns - 1 || (uint)y >= XrayTilemapLayout.MetatileRows) return;
        int metatile = metatileWord.VisualBlockIndex;
        int source = metatile * 8;
        ReadOnlySpan<byte> definitions = level.BlockDefinitions.Span;
        if (source + 8 > definitions.Length)
            throw new InvalidDataException($"X-ray overlay metatile ${metatile:X4} is outside room definitions.");
        int destination = y * XrayTilemapLayout.MetatileRowStride + x * 2;
        // $91:D0A6 swaps the two tile rows for vertical flip. It neither toggles each
        // tile's flip bits nor implements horizontal flip; preserve that native behavior.
        bool flip = (metatileWord.VisualFlipFlags & LevelBlockFlipFlags.Vertical) != 0;
        for (int row = 0; row < 2; row++)
        for (int col = 0; col < 2; col++)
        {
            int offset = source + (flip ? 1 - row : row) * 4 + col * 2;
            tilemap[destination + row * 32 + col] = (ushort)(SnesLittleEndian.Word(definitions[offset], definitions[offset + 1]));
        }
    }

}
