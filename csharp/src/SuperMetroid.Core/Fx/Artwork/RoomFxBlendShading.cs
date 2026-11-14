using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Names the shade of each palette blend a room effect selects (#1512).</summary>
public static class RoomFxBlendShading
{
    /// <summary>The shade the effect overlay pictures show under <paramref name="blend"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The blend is undefined.</exception>
    public static PictureShade Of(RoomFxPaletteBlend blend) => blend switch
    {
        RoomFxPaletteBlend.None => RoomFxBlendShades.None,
        RoomFxPaletteBlend.Lava => RoomFxBlendShades.Lava,
        RoomFxPaletteBlend.LandingSiteRain => RoomFxBlendShades.LandingSiteRain,
        RoomFxPaletteBlend.IndigoMaridiaWater => RoomFxBlendShades.IndigoMaridiaWater,
        RoomFxPaletteBlend.WaterAndAcid => RoomFxBlendShades.WaterAndAcid,
        RoomFxPaletteBlend.Fog => RoomFxBlendShades.Fog,
        RoomFxPaletteBlend.TealMaridiaWater => RoomFxBlendShades.TealMaridiaWater,
        RoomFxPaletteBlend.GreenMaridiaWater => RoomFxBlendShades.GreenMaridiaWater,
        RoomFxPaletteBlend.PurpleMaridiaWater => RoomFxBlendShades.PurpleMaridiaWater,
        _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, "Undefined room-FX palette blend."),
    };
}
