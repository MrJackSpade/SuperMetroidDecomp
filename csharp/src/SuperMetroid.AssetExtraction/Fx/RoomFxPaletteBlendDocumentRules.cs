using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The installed room-FX blend document's rules (<see cref="RoomFxPaletteBlendDefinitions"/>): the eight retail blends in
/// document order and each blend's document key.
/// </summary>
public static class RoomFxPaletteBlendDocumentRules
{
    /// <summary>The eight named retail blend resources in document order, without stored identities.</summary>
    public static IEnumerable<RoomFxPaletteBlend> Ids
    {
        get
        {
            yield return RoomFxPaletteBlend.Lava;
            yield return RoomFxPaletteBlend.LandingSiteRain;
            yield return RoomFxPaletteBlend.IndigoMaridiaWater;
            yield return RoomFxPaletteBlend.WaterAndAcid;
            yield return RoomFxPaletteBlend.Fog;
            yield return RoomFxPaletteBlend.TealMaridiaWater;
            yield return RoomFxPaletteBlend.GreenMaridiaWater;
            yield return RoomFxPaletteBlend.PurpleMaridiaWater;
        }
    }

    /// <summary>
    /// The installed document's key for <paramref name="id"/>. Each key names the tint as the blend's identity does (#1740),
    /// never the cartridge's colour-table offset.
    /// </summary>
    /// <param name="id">One of the eight blends exposed by <see cref="Ids"/>; <see cref="RoomFxPaletteBlend.None"/> clears the blend and has no resource key.</param>
    /// <exception cref="InvalidDataException">The blend is not one of the eight catalogued blends.</exception>
    public static string Key(RoomFxPaletteBlend id) => id switch
    {
        RoomFxPaletteBlend.Lava => "lava",
        RoomFxPaletteBlend.LandingSiteRain => "landing-site-rain",
        RoomFxPaletteBlend.IndigoMaridiaWater => "indigo-maridia-water",
        RoomFxPaletteBlend.WaterAndAcid => "water-and-acid",
        RoomFxPaletteBlend.Fog => "fog",
        RoomFxPaletteBlend.TealMaridiaWater => "teal-maridia-water",
        RoomFxPaletteBlend.GreenMaridiaWater => "green-maridia-water",
        RoomFxPaletteBlend.PurpleMaridiaWater => "purple-maridia-water",
        RoomFxPaletteBlend.None => throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued."),
        _ => throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued."),
    };
}
