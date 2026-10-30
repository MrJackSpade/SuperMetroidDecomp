using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening's cinematic sheets (#1392): every family's composition of each pose over its screen's OBJ page,
/// its palette-inheriting parts in the sheet's frame palette, coloured as the screen loads its palette. The intro sheet
/// holds the intro's families; the Ceres flight sheet holds the flight's.
/// </summary>
internal static class IntroSpriteSheetComposer
{

    /// <summary>The intro's cinematic families.</summary>
    private static IEnumerable<ICinematicSpriteFrames> Families(IntroCinematicArtwork intro)
    {
        IntroCinematicSourceArtwork sources = IntroCinematicSourceArtwork.Of(intro);
        return
        [
            sources.DiscoveryActorSprites, sources.EggEffectSprites, sources.RinkaSprites, sources.ScientistSprites,
            sources.MotherBrainSprites, sources.MotherBrainExplosionSprites, sources.CaretSprites,
        ];
    }

    /// <summary>The Ceres destruction scene's families: the shared Ceres flight family and the destruction family.</summary>
    private static IEnumerable<ICinematicSpriteFrames> CeresDestructionFamilies(IntroCinematicArtwork intro) =>
        [CeresFlightCharacters.Of(intro.CeresFlight).Sprites, IntroCinematicSourceArtwork.Of(intro).CeresDestructionSprites];

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
        CeresFlightCharacters characters = CeresFlightCharacters.Of(flight);
        return Compose([characters.Sprites], characters.ObjectPage, flight.Palette.LoadTo);
    }

    /// <summary>
    /// What the Ceres destruction scene's sheet composes: the scene's destruction and shared Ceres flight families over
    /// the OBJ characters the station's scene loads, coloured as the scene loads the flight's palette.
    /// </summary>
    internal static SpriteSheetSources CeresDestructionSources(IntroCinematicArtwork intro) =>
        DestructionSources(intro, CeresDestructionObjectPage.Characters(Ensure.NotNull(intro), zebesRevealed: false));

    /// <summary>What the Zebes reveal's sheet composes: the same families over the OBJ characters once the reveal loads its own.</summary>
    internal static SpriteSheetSources ZebesRevealSources(IntroCinematicArtwork intro) =>
        DestructionSources(intro, CeresDestructionObjectPage.Characters(Ensure.NotNull(intro), zebesRevealed: true));

    /// <summary>The destruction's families over the OBJ <paramref name="characters"/> the scene has loaded.</summary>
    private static SpriteSheetSources DestructionSources(IntroCinematicArtwork intro, byte[] characters)
    {
        SpriteArtwork page = CeresDestructionObjectPage.Of(characters);
        return Compose(CeresDestructionFamilies(intro), page, intro.CeresFlight.Palette.LoadTo);
    }

    /// <summary>The frames the Ceres destruction scene's sheets hold.</summary>
    internal static IEnumerable<string> CeresDestructionFrameNames(IntroCinematicArtwork intro) =>
        FrameNames(CeresDestructionFamilies(Ensure.NotNull(intro)));

    /// <summary>The frames the intro sheet of <paramref name="intro"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IntroCinematicArtwork intro) => FrameNames(Families(Ensure.NotNull(intro)));

    /// <summary>The frames the Ceres flight sheet of <paramref name="flight"/> holds.</summary>
    internal static IEnumerable<string> CeresFlightFrameNames(CeresFlightArtwork flight) =>
        FrameNames([CeresFlightCharacters.Of(Ensure.NotNull(flight)).Sprites]);

    /// <summary>
    /// Each family's composition of every pose over <paramref name="page"/>, coloured as <paramref name="loadColours"/>
    /// loads them, at the menus' object sizes the intro and Ceres scenes draw at.
    /// </summary>
    private static SpriteSheetSources Compose(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page, Action<ScreenPalette> loadColours)
    {
        var colours = new ScreenPalette();
        loadColours(colours);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection);
        return new SpriteSheetSources([.. Frames(families, page, sizes)], colours.SpriteSets.All.Colors.ToArray(), sizes,
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
            return new SheetFrameSource(name, pair.Value.ToFrame(name, 0, sizes), page, FrontendSheetPalette.FrameColourGroup.Number);
        }));

    /// <summary>The frames a sheet of <paramref name="families"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IEnumerable<ICinematicSpriteFrames> families) =>
        families.SelectMany(family => family.Frames.Keys.Select(pose => CinematicSpriteSheet.FrameName(family.Family, pose)));
}
