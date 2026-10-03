using System.Buffers.Binary;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Independently regular fields of rain $8A:A100 and fog $8A:A940.</summary>
public static class RoomFxAtmosphereTilemapDefinitions
{
    /// <summary>Rain's palette field occupies bits10..12; other fields remain separate content.</summary>
    public const ushort RainCalculatedMask = 0x1c00;
    /// <summary>Fog's palette and priority fields occupy bits10..13.</summary>
    public const ushort FogCalculatedMask = 0x3c00;

    /// <summary>Selects the independently converted field mask for rain or fog only.</summary>
    public static ushort CalculatedMask(RoomFxType type) => type switch
    {
        RoomFxType.Rain => RainCalculatedMask,
        RoomFxType.Fog => FogCalculatedMask,
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>
    /// Cell0..1055: rain's blank first row uses palette0, its remaining32 rows use6.
    /// Fog uses palette6 and clear priority throughout. Values cover only the selected
    /// mask; no disposition for other character, flip or rain-priority fields is implied.
    /// </summary>
    public static ushort CalculatedFields(RoomFxType type, int index)
    {
        _ = CalculatedMask(type);
        if ((uint)index >= RoomFxLayer3TilemapFormat.CellsPerPage)
            throw new ArgumentOutOfRangeException(nameof(index));
        return (ushort)(type == RoomFxType.Rain && index < RoomFxLayer3TilemapFormat.WidthInTiles ? 0 : 6 << 10);
    }
}

/// <summary>Separates calculated atmosphere fields from imported fields and arbitrary custom edits.</summary>
internal sealed class RoomFxAtmosphereTilemap
{
    private readonly RoomFxType type;
    private readonly ushort[] preservedBits;
    private readonly Dictionary<int, ushort>? customFields;

    public RoomFxAtmosphereTilemap(RoomFxType type, ReadOnlySpan<byte> bytes)
    {
        ushort mask = RoomFxAtmosphereTilemapDefinitions.CalculatedMask(type);
        Ensure.LengthEqual(bytes, RoomFxLayer3TilemapFormat.PageByteCount);
        this.type = type;
        preservedBits = new ushort[RoomFxLayer3TilemapFormat.CellsPerPage];
        for (int index = 0; index < preservedBits.Length; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * 2)..]);
            preservedBits[index] = (ushort)(word & ~mask);
            ushort fields = (ushort)(word & mask);
            if (fields != RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index))
                (customFields ??= []).Add(index, fields);
        }
    }

    /// <summary>Composes an output transfer without storing the calculated fields in a cache.</summary>
    public byte[] CreateTransfer()
    {
        var bytes = new byte[RoomFxLayer3TilemapFormat.PageByteCount];
        for (int index = 0; index < preservedBits.Length; index++)
        {
            ushort fields = customFields is not null && customFields.TryGetValue(index, out ushort edited)
                ? edited : RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), (ushort)(preservedBits[index] | fields));
        }
        return bytes;
    }
}
