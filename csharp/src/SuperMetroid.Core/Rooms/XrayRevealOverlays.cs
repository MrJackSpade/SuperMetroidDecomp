using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Ports uncollected-item and room-special reveal writes after the terrain pass.</summary>
public static class XrayRevealOverlays
{
    /// <summary>Applies $84:831A in descending PLM slot order, then room-authored record order.</summary>
    public static void Apply(ISnesAddressSpace bus, RoomLevelData level, Span<ushort> tilemap,
        IReadOnlyList<CollectiblePlmSnapshot> items, Bank80SystemState system,
        ushort specialPointer, ushort layer1X, ushort layer1Y)
    {
        if (tilemap.Length != XrayTilemapLayout.BufferWords)
            throw new ArgumentException("X-ray overlays require both native tilemap screens.", nameof(tilemap));
        for (int i = items.Count - 1; i >= 0; i--)
        {
            CollectiblePlmSnapshot item = items[i];
            if (unchecked((short)item.RoomArgument) < 0 || system.HasCollectedItemBit(item.RoomArgument)) continue;
            int graphics = item.Kind < InWorldCollectibleKind.Bombs
                ? XrayOverlayRomData.DynamicGraphicsSlots + (int)item.Kind : item.GraphicsSlot;
            if ((uint)graphics >= XrayOverlayRomData.DynamicGraphicsSlots * 2)
                throw new InvalidDataException($"Item ${item.Header:X4} has invalid X-ray graphics slot {graphics}.");
            int pointer = ReadWord(bus, XrayOverlayRomData.ItemDrawPointers + graphics * 2);
            ushort word = (ushort)(ReadWord(bus, XrayOverlayRomData.ItemBank | (pointer + 2)) & 0x0fff);
            Write(level, tilemap, word, item.BlockIndex % level.WidthInBlocks,
                item.BlockIndex / level.WidthInBlocks, layer1X, layer1Y);
        }
        if (specialPointer == 0) return;
        for (int pointer = specialPointer; ; pointer += 4)
        {
            if (pointer < 0x8000 || pointer > ushort.MaxValue - 3)
                throw new InvalidDataException("X-ray special-room records cross the ROM bank without a terminator.");
            ushort coordinates = ReadWord(bus, XrayOverlayRomData.RoomBank | pointer);
            if (coordinates == 0) break;
            ushort word = ReadWord(bus, XrayOverlayRomData.RoomBank | (pointer + 2));
            Write(level, tilemap, word, (byte)coordinates, coordinates >> 8, layer1X, layer1Y);
        }
    }

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

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
