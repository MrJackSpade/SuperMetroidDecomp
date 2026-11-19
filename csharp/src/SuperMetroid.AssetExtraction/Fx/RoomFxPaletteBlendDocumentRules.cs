using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The installed room-FX blend document's rules (<see cref="RoomFxPaletteBlendDefinitions"/>): the eight retail blends in
/// document order and each blend's document key, which <see cref="RoomEffectTintKeys"/> writes out.
/// </summary>
public static class RoomFxPaletteBlendDocumentRules
{
    /// <summary>The eight named retail blend resources in document order, without stored identities.</summary>
    public static IEnumerable<RoomEffectTint> Ids
    {
        get
        {
            yield return RoomEffectTint.Lava;
            yield return RoomEffectTint.LandingSiteRain;
            yield return RoomEffectTint.IndigoMaridiaWater;
            yield return RoomEffectTint.WaterAndAcid;
            yield return RoomEffectTint.Fog;
            yield return RoomEffectTint.TealMaridiaWater;
            yield return RoomEffectTint.GreenMaridiaWater;
            yield return RoomEffectTint.PurpleMaridiaWater;
        }
    }

    /// <summary>
    /// The installed document's key for <paramref name="id"/>. Each key names the tint as the blend's identity does (#1740),
    /// never the cartridge's colour-table offset.
    /// </summary>
    /// <param name="id">One of the eight blends exposed by <see cref="Ids"/>; <see cref="RoomEffectTint.None"/> clears the blend and has no resource key.</param>
    /// <exception cref="InvalidDataException">The blend is not one of the eight catalogued blends.</exception>
    public static string Key(RoomEffectTint id) => Ids.Contains(id) ? RoomEffectTintKeys.Installed.Of(id)
        : throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued.");
}
