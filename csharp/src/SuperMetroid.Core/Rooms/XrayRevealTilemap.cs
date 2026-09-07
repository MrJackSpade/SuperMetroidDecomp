using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Builds the cartridge's copied-BG1/revealed-BG2 screens in native write order.</summary>
public static class XrayRevealTilemap
{
    /// <summary>
    /// Ports $91:CB8E-CFD9's base-copy and terrain-reveal passes. Item and special-room
    /// overlays are applied afterwards by their respective owners, as on the cartridge.
    /// </summary>
    public static ushort[] Build(ISnesAddressSpace bus, RoomLevelData level, SnesVram vram,
        ushort bg1HorizontalScroll, ushort bg1VerticalScroll, ushort layer1X, ushort layer1Y, byte area)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(vram);
        var result = new ushort[XrayTilemapLayout.BufferWords];
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        for (int col = 0; col < XrayTilemapLayout.MetatileColumns; col++)
        {
            int sourceX = ((bg1HorizontalScroll >> 4) + col) * 2;
            int sourceY = ((bg1VerticalScroll >> 4) + row) * 2;
            int destination = Destination(col, row);
            Write(destination, ReadBg1(sourceX, sourceY));
            Write(destination + 1, ReadBg1(sourceX + 1, sourceY));
            Write(destination + 32, ReadBg1(sourceX, sourceY + 1));
            Write(destination + 33, ReadBg1(sourceX + 1, sourceY + 1));
        }
        int firstBlock = (byte)(layer1Y >> 4) * level.WidthInBlocks + (layer1X >> 4);
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        {
            int first = firstBlock + row * level.WidthInBlocks;
            // The prior column may own a 2-wide reveal crossing the left edge. Its
            // right half is copied before this row's own commands overwrite it.
            if (first != 0 && Lookup(first - 1) is { } predecessor)
            {
                if (predecessor.Command == XrayRevealCodePointers.CopySquare)
                    CopyMetatile(Destination(0, row) + XrayTilemapLayout.MetatileRowStride, predecessor.BottomRight);
                if (predecessor.Command is XrayRevealCodePointers.CopyWide or XrayRevealCodePointers.CopySquare)
                    CopyMetatile(Destination(0, row), predecessor.TopRight);
            }
            for (int col = 0; col < XrayTilemapLayout.MetatileColumns; col++)
            {
                int index = first + col;
                if (Lookup(index) is not { } reveal) continue;
                int destination = Destination(col, row);
                if (reveal.Command is XrayRevealCodePointers.HorizontalExtension or XrayRevealCodePointers.VerticalExtension)
                {
                    if (XrayRevealExtensions.Resolve(bus, level, index) is { } replacement)
                        CopyMetatile(destination, replacement);
                    continue;
                }
                if (reveal.Command == XrayRevealCodePointers.CopyBrinstar && area != (byte)AreaId.Brinstar)
                    continue;
                CopyMetatile(destination, reveal.TopLeft);
                // The last column of screen one suppresses right-hand writes. Screen
                // two is visited separately with native remaining-count zero, not one.
                bool right = col != XrayTilemapLayout.MetatileColumns - 2;
                if (right && reveal.Command is XrayRevealCodePointers.CopyWide or XrayRevealCodePointers.CopySquare)
                    CopyMetatile(destination + 2, reveal.TopRight);
                if (reveal.Command is XrayRevealCodePointers.CopyTall or XrayRevealCodePointers.CopySquare)
                    CopyMetatile(destination + XrayTilemapLayout.MetatileRowStride, reveal.BottomLeft);
                if (right && reveal.Command == XrayRevealCodePointers.CopySquare)
                    CopyMetatile(destination + XrayTilemapLayout.MetatileRowStride + 2, reveal.BottomRight);
            }
        }
        return result;

        XrayRevealDefinition? Lookup(int index)
        {
            RoomCollisionBlock block = level.GetPlmCollisionBlockByIndex(index);
            return XrayRevealTable.Find(bus, block.CollisionType, block.Behavior);
        }
        ushort ReadBg1(int x, int y)
        {
            int tile = ((x & 63) / 32) * XrayTilemapLayout.ScreenWords + (y & 31) * 32 + (x & 31);
            return vram.ReadWord(SnesPpuLayout.GameplayBg1TilemapWord + tile);
        }
        void CopyMetatile(int destination, ushort metatile)
        {
            ReadOnlySpan<byte> definitions = level.BlockDefinitions.Span;
            int offset = metatile * 8;
            if (offset + 8 > definitions.Length)
                throw new InvalidDataException($"X-ray metatile ${metatile:X4} is outside the room definition table.");
            Write(destination, Word(offset)); Write(destination + 1, Word(offset + 2));
            Write(destination + 32, Word(offset + 4)); Write(destination + 33, Word(offset + 6));
            ushort Word(int position) => (ushort)(level.BlockDefinitions.Span[position] | level.BlockDefinitions.Span[position + 1] << 8);
        }
        void Write(int index, ushort value)
        {
            // Bottom-edge multi-block commands can write into subsequent native scratch
            // RAM. Only these two transferred screens belong to this display projection.
            if (index < result.Length) result[index] = value;
        }
    }

    private static int Destination(int column, int row) => row * XrayTilemapLayout.MetatileRowStride +
        (column == XrayTilemapLayout.MetatileColumns - 1 ? XrayTilemapLayout.ScreenWords : column * 2);
}
