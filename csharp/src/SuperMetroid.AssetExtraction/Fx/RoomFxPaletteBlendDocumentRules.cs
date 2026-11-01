using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The installed room-FX blend document's rules (<see cref="RoomFxPaletteBlendDefinitions"/>): the eight retail blends in
/// document order, each blend's document key, and the stock Ceres haze tints the document records.
/// </summary>
public static class RoomFxPaletteBlendDocumentRules
{
    /// <summary>Stock blue-channel fixed-color amplitude for Ceres haze.</summary>
    public static RgbColor StockCeresHazeBlue => StockCeresHaze(CeresHazeColor.Blue);

    /// <summary>Stock red-channel fixed-color amplitude after Ceres Ridley.</summary>
    public static RgbColor StockCeresHazeRed => StockCeresHaze(CeresHazeColor.Red);

    /// <summary>
    /// The Ceres haze selects a blue or red tint; it stops fading in at counter16,
    /// after counter15 was the final written amplitude. Calculate that selected RGB
    /// axis directly instead of retaining two default tint records.
    /// </summary>
    public static RgbColor StockCeresHaze(CeresHazeColor color) => RoomFxPaletteBlendRules.StockCeresHazeTint(color == CeresHazeColor.Red);

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
    /// used, so renaming or renumbering a blend never changes an installed or overridden key (#1415).
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
