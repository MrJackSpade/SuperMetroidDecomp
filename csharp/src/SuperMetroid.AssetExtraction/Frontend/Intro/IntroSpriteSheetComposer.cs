using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the intro sheet (#1392): every intro cinematic family's composition of each pose over the intro OBJ page, its
/// palette-inheriting parts in the sheet's frame palette, coloured as the intro loads its palette.
/// </summary>
internal static class IntroSpriteSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>The intro's cinematic families.</summary>
    private static IEnumerable<ICinematicSpriteFrames> Families(IntroCinematicArtworkCatalog intro) =>
    [
        intro.DiscoveryActorSprites, intro.EggEffectSprites, intro.RinkaSprites, intro.ScientistSprites,
        intro.MotherBrainSprites, intro.MotherBrainExplosionSprites, intro.CaretSprites,
    ];

    /// <summary>What the intro sheet composes.</summary>
    internal static SpriteSheetSources Sources(IntroCinematicArtworkCatalog intro)
    {
        Ensure.NotNull(intro);
        return Sources(Families(intro), intro);
    }

    /// <summary>What a sheet of <paramref name="families"/> over the intro OBJ page of <paramref name="intro"/> composes.</summary>
    internal static SpriteSheetSources Sources(IEnumerable<ICinematicSpriteFrames> families, IntroCinematicArtworkCatalog intro)
    {
        Ensure.NotNull(families);
        Ensure.NotNull(intro);
        SpriteArtwork page = intro.ObjectPage;
        List<SheetFrameSource> frames = [.. families.SelectMany(family => family.Frames.OrderBy(pair => pair.Key.Index).Select(pair =>
        {
            string name = CinematicSpriteSheet.FrameName(family.Family, pair.Key);
            return new SheetFrameSource(name, pair.Value.ToFrame(name, 0), page, FrontendSheetPalette.FramePalette);
        }))];
        var cgram = new SnesCgram();
        intro.Palette.LoadTo(cgram);
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), PartsKeepOwnRows: true);
    }

    /// <summary>The frames the intro sheet of <paramref name="intro"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IntroCinematicArtworkCatalog intro) =>
        Families(Ensure.NotNull(intro)).SelectMany(family => family.Frames.Keys.Select(pose => CinematicSpriteSheet.FrameName(family.Family, pose)));
}
