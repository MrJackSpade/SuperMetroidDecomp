using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Names the shade of each palette blend a room-FX record selects (#1512).</summary>
public static class RoomFxBlendShading
{
    /// <summary>The shade the effect overlay pictures show under <paramref name="blend"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The blend is undefined.</exception>
    public static PictureShade Of(RoomFxPaletteBlend blend) => blend switch
    {
        RoomFxPaletteBlend.None => RoomFxBlendShades.None,
        RoomFxPaletteBlend.Lava => RoomFxBlendShades.Lava,
        RoomFxPaletteBlend.LandingSiteRain => RoomFxBlendShades.LandingSiteRain,
        RoomFxPaletteBlend.MaridiaWaterA => RoomFxBlendShades.MaridiaWaterA,
        RoomFxPaletteBlend.WaterAndAcid => RoomFxBlendShades.WaterAndAcid,
        RoomFxPaletteBlend.Fog => RoomFxBlendShades.Fog,
        RoomFxPaletteBlend.MaridiaWaterB => RoomFxBlendShades.MaridiaWaterB,
        RoomFxPaletteBlend.MaridiaWaterC => RoomFxBlendShades.MaridiaWaterC,
        RoomFxPaletteBlend.MaridiaWaterD => RoomFxBlendShades.MaridiaWaterD,
        _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, "Undefined room-FX palette blend."),
    };
}
