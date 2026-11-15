using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Names the shade of each tint a room effect selects (#1512, #1898).</summary>
public static class RoomEffectTintShading
{
    /// <summary>The shade the effect overlay's and the HUD's pictures show under <paramref name="tint"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tint is undefined.</exception>
    public static PictureShade Of(RoomEffectTint tint) => tint switch
    {
        RoomEffectTint.None => RoomEffectTintShades.None,
        RoomEffectTint.Lava => RoomEffectTintShades.Lava,
        RoomEffectTint.LandingSiteRain => RoomEffectTintShades.LandingSiteRain,
        RoomEffectTint.IndigoMaridiaWater => RoomEffectTintShades.IndigoMaridiaWater,
        RoomEffectTint.WaterAndAcid => RoomEffectTintShades.WaterAndAcid,
        RoomEffectTint.Fog => RoomEffectTintShades.Fog,
        RoomEffectTint.TealMaridiaWater => RoomEffectTintShades.TealMaridiaWater,
        RoomEffectTint.GreenMaridiaWater => RoomEffectTintShades.GreenMaridiaWater,
        RoomEffectTint.PurpleMaridiaWater => RoomEffectTintShades.PurpleMaridiaWater,
        _ => throw new ArgumentOutOfRangeException(nameof(tint), tint, "Undefined room-effect tint."),
    };
}
