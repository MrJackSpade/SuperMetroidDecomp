using System.Buffers.Binary;

namespace SuperMetroid.Core.Fx.Artwork;

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
    /// <summary>Authored particle character placement retained from the installed page.</summary>
    private readonly ushort[] characters;
    /// <summary>Attribute words that differ from the calculated stock palette and flip selection.</summary>
    internal readonly Dictionary<int, ushort>? customAttributes;

    /// <summary>Separates arbitrary particle placement from stock attributes and explicit attribute edits.</summary>
    public RoomFxSporeTilemap(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, RoomFxLayer3TilemapFormat.PageByteCount);
        characters = new ushort[RoomFxLayer3TilemapFormat.CellsPerPage];
        for (int index = 0; index < characters.Length; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]);
            characters[index] = (ushort)new Hardware.SnesBgTilemapWord(word).CharacterIndex;
            ushort attributes = new Hardware.SnesBgTilemapWord(word).WithCharacterIndex(0).Raw;
            if (attributes != RoomFxSporeTilemapDefinitions.Attributes(index))
                (customAttributes ??= []).Add(index, attributes);
        }
    }

    /// <summary>Composes the page without caching a reconstructed attribute/page table.</summary>
    public byte[] ComposePage()
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
