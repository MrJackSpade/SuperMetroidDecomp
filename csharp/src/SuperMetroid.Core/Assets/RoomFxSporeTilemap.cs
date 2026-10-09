using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

/// <summary>Constant stock spore attributes in all 1056 words at $8A:98C0..A0FF.</summary>
public static class RoomFxSporeTilemapDefinitions
{
    /// <summary>Cell0..1055 uses palette6, clear priority and no flips: attribute word $1800.</summary>
    public static ushort Attributes(int index)
    {
        if ((uint)index >= RoomFxLayer3TilemapFormat.CellsPerPage)
            throw new ArgumentOutOfRangeException(nameof(index));
        return 6 << 10;
    }
}

/// <summary>Loaded particle composition with calculated stock attributes and explicit custom edits.</summary>
/// <remarks>
/// The original character field places 40 isolated particles (four $50, sixteen $51,
/// twenty $52) among 1016 blank $4E cells. Those particular positions and particle
/// choices are graphical composition, not motion samples or semantic dispatch.
/// Reciting them in cases or fitting coordinates would disguise the same drawing.
/// Character placement is retained under the arbitrary-art/nonsense exception;
/// palette, priority and flip fields are independently calculated rather than exempted.
/// </remarks>
internal sealed class RoomFxSporeTilemap
{
    /// <summary>The low ten character-index bits retained for every cell in the extracted page.</summary>
    private readonly ushort[] characters;

    /// <summary>Only cell attributes that differ from the calculated stock palette, priority, and flip bits.</summary>
    private readonly Dictionary<int, ushort>? customAttributes;

    /// <summary>Separates character indices from stock attributes while preserving explicit attribute edits.</summary>
    /// <param name="bytes">One complete Layer 3 tilemap page in little-endian tilemap-word order.</param>
    public RoomFxSporeTilemap(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, RoomFxLayer3TilemapFormat.PageByteCount);
        characters = new ushort[RoomFxLayer3TilemapFormat.CellsPerPage];
        for (int index = 0; index < characters.Length; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]);
            characters[index] = (ushort)(word & 0x03ff);
            ushort attributes = new Hardware.SnesBgTilemapWord(word).WithCharacterIndex(0).Raw;
            if (attributes != RoomFxSporeTilemapDefinitions.Attributes(index))
                (customAttributes ??= []).Add(index, attributes);
        }
    }

    /// <summary>Produces a transfer without caching a reconstructed attribute/page table.</summary>
    public byte[] CreateTransfer()
    {
        var bytes = new byte[RoomFxLayer3TilemapFormat.PageByteCount];
        for (int index = 0; index < characters.Length; index++)
        {
            ushort attributes = customAttributes is not null && customAttributes.TryGetValue(index, out ushort edited)
                ? edited : RoomFxSporeTilemapDefinitions.Attributes(index);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), (ushort)(characters[index] | attributes));
        }
        return bytes;
    }
}
