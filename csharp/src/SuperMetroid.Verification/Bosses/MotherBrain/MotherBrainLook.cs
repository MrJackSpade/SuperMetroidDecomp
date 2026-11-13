using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// The colours Mother Brain shows under one of her shades (#1512), laid in the cartridge's colour order over a seeded
/// colour table for the checks that compare them with the cartridge's copies: the import's background shade over her
/// body's colours and its sprite shade over her brain's and legs'.
/// </summary>
internal static class MotherBrainLook
{
    /// <summary>The colours <paramref name="rainbow"/>'s shade <paramref name="shade"/> shows over <paramref name="seed"/>.</summary>
    internal static SnesCgram OfRainbow(InstalledMotherBrainRainbowPalette rainbow, PictureShade shade, SnesCgram? seed = null) =>
        Of(MotherBrainSheetShades.RainbowBackground(rainbow), MotherBrainSheetShades.RainbowSprites(rainbow), shade, seed);

    /// <summary>The colours <paramref name="health"/>'s band <paramref name="band"/> shows over <paramref name="seed"/>.</summary>
    internal static SnesCgram OfHealth(MotherBrainHealthPalettePresentation health, int band, SnesCgram? seed = null) =>
        Of(MotherBrainSheetShades.HealthBackground(health), MotherBrainSheetShades.HealthSprites(health), MotherBrainShades.Health(band), seed);

    /// <summary>
    /// <paramref name="colours"/> with the shades <paramref name="state"/>'s encounter shows over them: those over her
    /// room and her body layer, then those over her own sprites and the Baby, as the import pre-colours them from
    /// <paramref name="maps"/> and <paramref name="bosses"/>.
    /// </summary>
    internal static ushort[] Over(MotherBrainEnemyState state, SuperMetroid.AssetExtraction.Frontend.Map.ImportAreaMapPresentation maps, BossSourceArtwork bosses, ushort[] colours)
    {
        ArgumentNullException.ThrowIfNull(state);
        IReadOnlyList<SheetShade> room = MotherBrainSheetShades.Room(maps);
        IReadOnlyList<SheetShade> body = MotherBrainSheetShades.BodyLayer(maps, bosses);
        IReadOnlyList<SheetShade> sprites = MotherBrainSheetShades.Sprites(maps, bosses);
        foreach (PictureShade shade in state.Colours.RoomShading.Shades)
            BossShadeColours.Lay(colours, room, shade, BossShadeColours.Room);
        foreach (PictureShade shade in state.Colours.BodyLayerShading.Shades)
            BossShadeColours.Lay(colours, body, shade, BossShadeColours.Room);
        foreach (PictureShade shade in state.Colours.OwnShading.Shades)
            BossShadeColours.Lay(colours, sprites, shade, BossShadeColours.Sprites);
        foreach (PictureShade shade in state.Colours.BabyShading.Shades)
            BossShadeColours.Lay(colours, sprites, shade, BossShadeColours.Sprites);
        return colours;
    }

    /// <summary>The colours <paramref name="shade"/> shows over <paramref name="seed"/>, from whichever of the lists holds it.</summary>
    private static SnesCgram Of(IReadOnlyList<SheetShade> background, IReadOnlyList<SheetShade> sprites, PictureShade shade, SnesCgram? seed)
    {
        ushort[] colours = BossShadeColours.Filled(0);
        if (seed is not null)
            for (int colour = 0; colour < SnesCgram.ColorCount; colour++)
                colours[colour] = seed.Colors[colour].ToWord();
        if (background.Any(candidate => candidate.Shade == shade))
            BossShadeColours.Lay(colours, background, shade, BossShadeColours.Room);
        if (sprites.Any(candidate => candidate.Shade == shade))
            BossShadeColours.Lay(colours, sprites, shade, BossShadeColours.Sprites);
        var cgram = new SnesCgram();
        for (int colour = 0; colour < SnesCgram.ColorCount; colour++)
            cgram.SetColor(colour, RgbColor.FromWord(colours[colour]));
        return cgram;
    }
}
