using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Names the shade of each tint a room effect selects (#1512, #1898).</summary>
public static class RoomFxBlendShading
{
    /// <summary>The shade the effect overlay pictures show under <paramref name="blend"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The blend is undefined.</exception>
    public static PictureShade Of(RoomEffectTint blend) => blend switch
    {
        RoomEffectTint.None => RoomFxBlendShades.None,
        RoomEffectTint.Lava => RoomFxBlendShades.Lava,
        RoomEffectTint.LandingSiteRain => RoomFxBlendShades.LandingSiteRain,
        RoomEffectTint.IndigoMaridiaWater => RoomFxBlendShades.IndigoMaridiaWater,
        RoomEffectTint.WaterAndAcid => RoomFxBlendShades.WaterAndAcid,
        RoomEffectTint.Fog => RoomFxBlendShades.Fog,
        RoomEffectTint.TealMaridiaWater => RoomFxBlendShades.TealMaridiaWater,
        RoomEffectTint.GreenMaridiaWater => RoomFxBlendShades.GreenMaridiaWater,
        RoomEffectTint.PurpleMaridiaWater => RoomFxBlendShades.PurpleMaridiaWater,
        _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, "Undefined room-effect tint."),
    };
}
