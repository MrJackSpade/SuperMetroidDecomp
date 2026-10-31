using SuperMetroid.Core.Fx;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Independently regular fields of rain $8A:A100 and fog $8A:A940.</summary>
[EncodingOwner("rain and fog layer-three tilemap word fields")]
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
        RoomFxType.None or
            RoomFxType.Lava or
            RoomFxType.Acid or
            RoomFxType.Water or
            RoomFxType.Spores or
            RoomFxType.ScrollingSky or
            RoomFxType.UnusedScrollingSky or
            RoomFxType.Fireflea or
            RoomFxType.TourianEntranceStatue or
            RoomFxType.CeresRidley or
            RoomFxType.CeresElevator or
            RoomFxType.CeresHaze => throw new ArgumentOutOfRangeException(nameof(type)),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>
    /// Cell0..1055: rain's blank first row uses palette0, its remaining32 rows use6.
    /// Fog uses palette6 and clear priority throughout. Values cover only the selected
    /// mask; the independent graphical fields are described on the page owner below.
    /// </summary>
    public static ushort CalculatedFields(RoomFxType type, int index)
    {
        _ = CalculatedMask(type);
        if ((uint)index >= RoomFxLayer3TilemapFormat.CellsPerPage)
            throw new ArgumentOutOfRangeException(nameof(index));
        return (ushort)(type == RoomFxType.Rain && index < RoomFxLayer3TilemapFormat.WidthInTiles ? 0 : 6 << 10);
    }
}
