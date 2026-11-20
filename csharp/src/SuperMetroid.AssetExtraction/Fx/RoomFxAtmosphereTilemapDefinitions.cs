using SuperMetroid.Core.Fx;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Independently regular fields of rain $8A:A100 and fog $8A:A940.</summary>
[EncodingOwner("rain and fog layer-three tilemap word fields")]
public static class RoomFxAtmosphereTilemapDefinitions
{
    /// <summary>Rain's palette field: bits 10 to 12; other fields remain separate content.</summary>
    public const ushort RainCalculatedMask = 0x1c00;
    /// <summary>Fog's palette and priority fields: bits 10 to 13.</summary>
    public const ushort FogCalculatedMask = 0x3c00;

    /// <summary>Selects the independently converted field mask for rain or fog only.</summary>
    public static ushort CalculatedMask(RoomFxType type) => type switch
    {
        RoomFxType.Rain => RainCalculatedMask,
        RoomFxType.Fog => FogCalculatedMask,
        RoomFxType.None or
            RoomFxType.Lava or
            RoomFxType.Acid or
            RoomFxType.Water or
            RoomFxType.Spores or
            RoomFxType.ScrollingSky or
            RoomFxType.Fireflea or
            RoomFxType.TourianEntranceStatue or
            RoomFxType.CeresRidley or
            RoomFxType.CeresElevator or
            RoomFxType.CeresHaze => throw new ArgumentOutOfRangeException(nameof(type)),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>
    /// The calculated fields of the cell at <paramref name="index"/>: rain's blank first row uses palette 0 and its
    /// remaining rows palette 6; fog uses palette 6 and clear priority throughout. Values cover only the selected
    /// mask; the other fields are the page's own content (<see cref="RoomFxLayer3TilemapFormat"/>).
    /// </summary>
    public static ushort CalculatedFields(RoomFxType type, int index)
    {
        _ = CalculatedMask(type);
        if ((uint)index >= RoomFxLayer3TilemapFormat.CellsPerPage)
            throw new ArgumentOutOfRangeException(nameof(index));
        return (ushort)(type == RoomFxType.Rain && index < RoomFxLayer3TilemapFormat.WidthInTiles ? 0 : 6 << 10);
    }
}
