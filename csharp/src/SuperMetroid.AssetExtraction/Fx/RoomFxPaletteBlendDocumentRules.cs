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
            yield return RoomFxPaletteBlend.MaridiaWaterA;
            yield return RoomFxPaletteBlend.WaterAndAcid;
            yield return RoomFxPaletteBlend.Fog;
            yield return RoomFxPaletteBlend.MaridiaWaterB;
            yield return RoomFxPaletteBlend.MaridiaWaterC;
            yield return RoomFxPaletteBlend.MaridiaWaterD;
        }
    }

    /// <summary>
    /// The installed document's key for <paramref name="id"/>. The keys keep the names the installed file has always
    /// used, so renaming or renumbering a blend never changes an installed key (#1415).
    /// </summary>
    /// <param name="id">One of the eight blends exposed by <see cref="Ids"/>; <see cref="RoomFxPaletteBlend.None"/> clears the blend and has no resource key.</param>
    /// <exception cref="InvalidDataException">The blend is not one of the eight catalogued blends.</exception>
    public static string Key(RoomFxPaletteBlend id) => id switch
    {
        RoomFxPaletteBlend.Lava => "blend-02",
        RoomFxPaletteBlend.LandingSiteRain => "blend-22",
        RoomFxPaletteBlend.MaridiaWaterA => "blend-42",
        RoomFxPaletteBlend.WaterAndAcid => "blend-48",
        RoomFxPaletteBlend.Fog => "blend-62",
        RoomFxPaletteBlend.MaridiaWaterB => "blend-E2",
        RoomFxPaletteBlend.MaridiaWaterC => "blend-E8",
        RoomFxPaletteBlend.MaridiaWaterD => "blend-EE",
        RoomFxPaletteBlend.None => throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued."),
        _ => throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued."),
    };
}
