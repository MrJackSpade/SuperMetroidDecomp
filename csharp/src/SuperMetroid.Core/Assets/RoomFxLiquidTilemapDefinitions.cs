using System.Buffers.Binary;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated stock liquid BG3 layouts at $8A:8000,8840,9080.</summary>
public static class RoomFxLiquidTilemapDefinitions
{
    /// <summary>
    /// Returns cell0..1055 of a 32x33 lava, acid or water page. Lava's first row
    /// repeats three surface characters; its remaining rows are solid body tiles.
    /// Acid adds a blank row above that surface. Water adds the same blank row,
    /// four surface rows repeated across four columns, then a repeating 4x4 body.
    /// Palette/priority select the native liquid layer; all flip bits are clear.
    /// </summary>
    public static SnesBgTilemapWord Cell(RoomFxType type, int index)
    {
        ValidateType(type);
        if ((uint)index >= RoomFxLayer3TilemapFormat.CellsPerPage)
            throw new ArgumentOutOfRangeException(nameof(index));
        int row = index / 32, column = index % 32;
        bool blank = type != RoomFxType.Lava && row == 0;
        int character;
        if (blank) character = 0x4e;
        else if (type == RoomFxType.Water)
            character = row <= 4 ? 0x90 + 4 * (row - 1) + column % 4
                : 0xa0 + 4 * ((row - 5) % 4) + column % 4;
        else
            character = row == (type == RoomFxType.Lava ? 0 : 1) ? 0x50 + column % 3 : 0x53;
        int palette = blank ? 3 : type == RoomFxType.Acid ? 0 : 6;
        return SnesBgTilemapWord.Create(character, palette,
            type == RoomFxType.Water && !blank, 0);
    }

    /// <summary>Creates the requested transfer buffer on demand; no generated page is cached.</summary>
    public static byte[] CreateTransfer(RoomFxType type)
    {
        ValidateType(type);
        var bytes = new byte[RoomFxLayer3TilemapFormat.PageByteCount];
        for (int index = 0; index < RoomFxLayer3TilemapFormat.CellsPerPage; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), Cell(type, index).Raw);
        return bytes;
    }

    /// <summary>Recognizes an exact stock page so its redundant input bytes need not be retained.</summary>
    internal static bool Matches(RoomFxType type, ReadOnlySpan<byte> bytes)
    {
        ValidateType(type);
        if (bytes.Length != RoomFxLayer3TilemapFormat.PageByteCount) return false;
        for (int index = 0; index < RoomFxLayer3TilemapFormat.CellsPerPage; index++)
            if (BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]) != Cell(type, index).Raw) return false;
        return true;
    }

    /// <summary>Requires a room effect with a calculated liquid BG3 page definition.</summary>
    /// <param name="type">Effect type; only Lava, Acid, and Water have generated pages.</param>
    /// <exception cref="ArgumentOutOfRangeException">The type is not one of the supported liquid effects.</exception>
    private static void ValidateType(RoomFxType type)
    {
        if (type is not (RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water))
            throw new ArgumentOutOfRangeException(nameof(type));
    }
}
