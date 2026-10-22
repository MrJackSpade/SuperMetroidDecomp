using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening's cinematic sheets (#1392): every family's composition of each pose over its screen's OBJ page,
/// its palette-inheriting parts in the sheet's frame palette, coloured as the screen loads its palette. The intro sheet
/// holds the intro's families; the Ceres flight sheet holds the flight's.
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
        Ensure.NotNull(intro);
        return Compose(families, intro.ObjectPage, intro.Palette.LoadTo);
    }

    /// <summary>What the Ceres flight sheet of <paramref name="flight"/> composes.</summary>
    internal static SpriteSheetSources CeresFlightSources(CeresFlightArtworkCatalog flight)
    {
        Ensure.NotNull(flight);
        return Compose([flight.Sprites], flight.ObjectPage, flight.Palette.LoadTo);
    }

    /// <summary>The frames the intro sheet of <paramref name="intro"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IntroCinematicArtworkCatalog intro) => FrameNames(Families(Ensure.NotNull(intro)));

    /// <summary>The frames the Ceres flight sheet of <paramref name="flight"/> holds.</summary>
    internal static IEnumerable<string> CeresFlightFrameNames(CeresFlightArtworkCatalog flight) =>
        FrameNames([Ensure.NotNull(flight).Sprites]);

    /// <summary>Each family's composition of every pose over <paramref name="page"/>, coloured as <paramref name="loadColours"/> loads them.</summary>
    private static SpriteSheetSources Compose(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page, Action<SnesCgram> loadColours)
    {
        Ensure.NotNull(families);
        List<SheetFrameSource> frames = [.. families.SelectMany(family => family.Frames.OrderBy(pair => pair.Key.Index).Select(pair =>
        {
            string name = CinematicSpriteSheet.FrameName(family.Family, pair.Key);
            return new SheetFrameSource(name, pair.Value.ToFrame(name, 0), page, FrontendSheetPalette.FramePalette);
        }))];
        var cgram = new SnesCgram();
        loadColours(cgram);
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), PartsKeepOwnRows: true);
    }

    /// <summary>The frames a sheet of <paramref name="families"/> holds.</summary>
    private static IEnumerable<string> FrameNames(IEnumerable<ICinematicSpriteFrames> families) =>
        families.SelectMany(family => family.Frames.Keys.Select(pose => CinematicSpriteSheet.FrameName(family.Family, pose)));
}
