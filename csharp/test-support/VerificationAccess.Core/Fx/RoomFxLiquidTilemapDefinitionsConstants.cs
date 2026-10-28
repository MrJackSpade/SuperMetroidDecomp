using System.Buffers.Binary;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomFxLiquidTilemapDefinitions"/> that only verification reads.</summary>
internal static class RoomFxLiquidTilemapDefinitionsConstants
{
    /// <summary>Recognizes an exact stock page so its redundant input bytes need not be retained.</summary>
    internal static bool Matches(RoomFxType type, ReadOnlySpan<byte> bytes)
    {
        RoomFxLiquidTilemapDefinitions.ValidateType(type);
        if (bytes.Length != RoomFxLayer3TilemapFormat.PageByteCount) return false;
        for (int index = 0; index < RoomFxLayer3TilemapFormat.CellsPerPage; index++)
            if (BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]) != RoomFxLiquidTilemapDefinitions.Cell(type, index).Raw) return false;
        return true;
    }
}
