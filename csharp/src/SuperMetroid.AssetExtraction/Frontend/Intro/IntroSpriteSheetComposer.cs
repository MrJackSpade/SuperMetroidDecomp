using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Areas.Ceres;

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
    private static IEnumerable<ICinematicSpriteFrames> Families(IntroCinematicArtwork intro) =>
    [
        intro.DiscoveryActorSprites, intro.EggEffectSprites, intro.RinkaSprites, intro.ScientistSprites,
        intro.MotherBrainSprites, intro.MotherBrainExplosionSprites, intro.CaretSprites,
    ];

    /// <summary>What the intro sheet composes.</summary>
    internal static SpriteSheetSources Sources(IntroCinematicArtwork intro)
    {
        Ensure.NotNull(intro);
        return Sources(Families(intro), intro);
    }

    /// <summary>What a sheet of <paramref name="families"/> over the intro OBJ page of <paramref name="intro"/> composes.</summary>
    internal static SpriteSheetSources Sources(IEnumerable<ICinematicSpriteFrames> families, IntroCinematicArtwork intro)
    {
        Ensure.NotNull(intro);
        return Compose(families, IntroObjectPage.Of(intro), intro.Palette.LoadTo);
    }

    /// <summary>What the Ceres flight sheet of <paramref name="flight"/> composes.</summary>
    internal static SpriteSheetSources CeresFlightSources(CeresFlightArtwork flight)
    {
        Ensure.NotNull(flight);
        return Compose([flight.Sprites], CeresFlightCharacters.Of(flight).ObjectPage, flight.Palette.LoadTo);
    }

    /// <summary>
    /// What the Ceres destruction scene's sheet composes: the scene's destruction and shared Ceres flight families over
    /// the OBJ characters <paramref name="uploads"/> leave, coloured as the scene loads the flight's palette.
    /// </summary>
    internal static SpriteSheetSources CeresDestructionSources(IntroCinematicArtwork intro, IReadOnlyList<CeresDestructionUpload> uploads)
    {
        Ensure.NotNull(intro);
        SpriteArtwork page = CeresDestructionObjectPage.Of(intro, uploads);
        return Compose([intro.CeresFlight.Sprites, intro.CeresDestruction.Sprites], page, intro.CeresFlight.Palette.LoadTo);
    }

    /// <summary>The frames the Ceres destruction scene's sheets hold.</summary>
    internal static IEnumerable<string> CeresDestructionFrameNames(IntroCinematicArtwork intro) =>
        FrameNames([Ensure.NotNull(intro).CeresFlight.Sprites, intro.CeresDestruction.Sprites]);

    /// <summary>The frames the intro sheet of <paramref name="intro"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IntroCinematicArtwork intro) => FrameNames(Families(Ensure.NotNull(intro)));

    /// <summary>The frames the Ceres flight sheet of <paramref name="flight"/> holds.</summary>
    internal static IEnumerable<string> CeresFlightFrameNames(CeresFlightArtwork flight) =>
        FrameNames([Ensure.NotNull(flight).Sprites]);

    /// <summary>
    /// Each family's composition of every pose over <paramref name="page"/>, coloured as <paramref name="loadColours"/>
    /// loads them, at the menus' object sizes the intro and Ceres scenes draw at.
    /// </summary>
    private static SpriteSheetSources Compose(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page, Action<ScreenPalette> loadColours)
    {
        var colours = new ScreenPalette();
        loadColours(colours);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection);
        return new SpriteSheetSources([.. Frames(families, page, sizes)], colours.Colors[SpriteColours..].ToArray(), sizes,
            PartsKeepOwnRows: true);
    }

    /// <summary>
    /// Each family's composition of every pose over <paramref name="page"/>, its objects at <paramref name="sizes"/> and its
    /// palette-inheriting parts in the sheet's frame palette.
    /// </summary>
    internal static IEnumerable<SheetFrameSource> Frames(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page,
        ScreenObjectSizes sizes) =>
        Ensure.NotNull(families).SelectMany(family => family.Frames.OrderBy(pair => pair.Key.Index).Select(pair =>
        {
            string name = CinematicSpriteSheet.FrameName(family.Family, pair.Key);
            return new SheetFrameSource(name, pair.Value.ToFrame(name, 0, sizes), page, FrontendSheetPalette.FramePalette);
        }));

    /// <summary>The frames a sheet of <paramref name="families"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IEnumerable<ICinematicSpriteFrames> families) =>
        families.SelectMany(family => family.Frames.Keys.Select(pose => CinematicSpriteSheet.FrameName(family.Family, pose)));
}
