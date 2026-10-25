using System.Buffers.Binary;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Separates calculated attributes from the authored rain/fog image. Rain character
/// positions draw scattered drops; fog characters and their orientations assemble
/// irregular cloud silhouettes. Rain also carries authored per-cell layering and
/// two horizontal/one vertical orientation bits (including a flipped blank).
/// These describe the composition itself, not particle motion: the runtime scrolls
/// the complete page. Replacing its drawing with coordinate exceptions or a fitted
/// generator would merely encode the same artwork as instructions. Preserve those
/// graphical fields and arbitrary custom edits; calculate the uniform attributes.
/// See atmosphereGraphicalFieldsReview in the issue1165 inventory for field evidence.
/// </summary>
internal sealed class RoomFxAtmosphereTilemap
{
    /// <summary>Rain or fog determines which tilemap attribute bits are calculated.</summary>
    private readonly RoomFxType type;
    /// <summary>Authored character, orientation, and unconverted attribute bits for every page cell.</summary>
    internal readonly ushort[] preservedBits;
    /// <summary>Per-cell edited attributes that differ from the calculated rain or fog defaults.</summary>
    internal readonly Dictionary<int, ushort>? customFields;

    /// <summary>Separates authored visual bits from calculable atmosphere attributes in a full native page.</summary>
    public RoomFxAtmosphereTilemap(RoomFxType type, ReadOnlySpan<byte> bytes)
    {
        int mask = RoomFxAtmosphereTilemapDefinitions.CalculatedMask(type);
        Ensure.LengthEqual(bytes, RoomFxLayer3TilemapFormat.PageByteCount);
        this.type = type;
        preservedBits = new ushort[RoomFxLayer3TilemapFormat.CellsPerPage];
        for (int index = 0; index < preservedBits.Length; index++)
        {
            int word = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]);
            preservedBits[index] = (ushort)(word & ~mask);
            int fields = (ushort)(word & mask);
            if (fields != RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index))
                (customFields ??= []).Add(index, checked((ushort)fields));
        }
    }

    /// <summary>Composes the page without storing the calculated fields in a cache.</summary>
    public byte[] ComposePage()
    {
        var bytes = new byte[RoomFxLayer3TilemapFormat.PageByteCount];
        for (int index = 0; index < preservedBits.Length; index++)
        {
            int fields = customFields is not null && customFields.ContainsKey(index)
                ? customFields[index] : RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), (ushort)(preservedBits[index] | fields));
        }
        return bytes;
    }
}
