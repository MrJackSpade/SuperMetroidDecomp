using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening's cinematic sheets (#1392): every family's composition of each pose over its screen's OBJ page,
/// its palette-inheriting parts in the sprite set of the colouring its actors draw it in, so each frame is pictured in its
/// own colours (#1513), with a shade for each colouring its actors draw in (#1512). The intro sheet holds the intro's
/// families; each Ceres scene's sheet holds the poses its actors show. The compositions are import data (#1638): the sheets'
/// frames are named from the families' poses alone (<see cref="CinematicSpritePoses"/>).
/// </summary>
internal static class IntroSpriteSheetComposer
{
    /// <summary>The intro sheet's cinematic families, in composition order.</summary>
    private static readonly CinematicSpriteFamily[] IntroFamilies =
    [
        CinematicSpriteFamily.IntroDiscoveryActor, CinematicSpriteFamily.IntroEggEffect, CinematicSpriteFamily.IntroRinka,
        CinematicSpriteFamily.IntroScientist, CinematicSpriteFamily.IntroMotherBrain, CinematicSpriteFamily.IntroMotherBrainExplosion,
        CinematicSpriteFamily.IntroCaret, CinematicSpriteFamily.IntroNextPageArrow,
    ];

    /// <summary>The Ceres destruction scene's families: the shared Ceres flight family and the destruction family.</summary>
    private static readonly CinematicSpriteFamily[] CeresDestructionFamilies = [CinematicSpriteFamily.CeresFlight, CinematicSpriteFamily.CeresDestruction];

    /// <summary>The intro's cinematic families' compositions.</summary>
    private static IEnumerable<ICinematicSpriteFrames> Families(ImportIntroCinematicArtwork intro) => IntroFamilies.Select(intro.SpritesOf);

    /// <summary>
    /// The Ceres scene's poses as the Zebes reveal draws them (#1513): the reveal clears the station's actors as it loads its
    /// own characters, and from then on only the planet, the four star sheets and the title run.
    /// </summary>
    /// <exception cref="InvalidDataException">The reveal's actors show a pose no Ceres family composes.</exception>
    private static List<ScenePoses> ZebesRevealPoses() =>
        ScenePosesOf("The Zebes reveal", CeresDestructionFamilies, CeresSceneActors.ZebesReveal());

    /// <summary>
    /// The Ceres scene's poses as the destruction draws them before the Zebes reveal (#1513): its asteroids, vortex and
    /// blasts. The planet and its title are the reveal's, drawn only over the characters the reveal loads.
    /// </summary>
    /// <exception cref="InvalidDataException">The destruction's actors show a pose no Ceres family composes.</exception>
    private static List<ScenePoses> DestructionScenePoses() =>
        ScenePosesOf("The Ceres destruction", CeresDestructionFamilies, CeresSceneActors.Destruction());

    /// <summary>The Ceres flight's poses as the flight draws them (#1513): those its stars, asteroids, vortex and station show.</summary>
    /// <exception cref="InvalidDataException">The flight's actors show a pose its family does not compose.</exception>
    private static List<ScenePoses> FlightPoses() =>
        ScenePosesOf("The Ceres flight", [CinematicSpriteFamily.CeresFlight], CeresSceneActors.Flight());

    /// <summary>
    /// <paramref name="families"/> keeping only the poses a Ceres scene's actors show, as <paramref name="poses"/> holds
    /// them (#1513), each drawn from the family that composes it for the scene: a pose of the destruction's own from the
    /// destruction family and any other from the flight's. A pose the scene never draws is not pictured: its characters
    /// are another scene's.
    /// </summary>
    /// <exception cref="InvalidDataException">The scene shows a pose none of its families composes.</exception>
    private static List<ScenePoses> ScenePosesOf(string scene, IEnumerable<CinematicSpriteFamily> families,
        Dictionary<CinematicSpritePose, CeresSpriteColouring> poses)
    {
        var shown = new HashSet<CinematicSpritePose>(poses.Keys);
        var destruction = new HashSet<CinematicSpritePose>(CeresDestructionSpriteDefinitions.Poses);
        var composed = new HashSet<CinematicSpritePose>();
        var scenePoses = new List<ScenePoses>();
        foreach (CinematicSpriteFamily family in families)
        {
            composed.UnionWith(CinematicSpritePoses.Of(family));
            scenePoses.Add(new ScenePoses(family, [.. CinematicSpritePoses.Of(family).Where(pose =>
                shown.Contains(pose) && destruction.Contains(pose) == (family == CinematicSpriteFamily.CeresDestruction))]));
        }
        foreach (CinematicSpritePose pose in shown)
        {
            if (!composed.Contains(pose))
                throw new InvalidDataException($"{scene} shows pose {pose}, which none of its families composes.");
        }
        return scenePoses;
    }

    /// <summary>Each family's compositions of the poses <paramref name="scenePoses"/> keeps for the scene, from <paramref name="intro"/>.</summary>
    private static ICinematicSpriteFrames[] SceneFamilies(ImportIntroCinematicArtwork intro, List<ScenePoses> scenePoses) =>
        [.. scenePoses.Select(entry => CinematicShownFrames.Of(intro.SpritesOf(entry.Family), entry.Poses, _ => true))];

    /// <summary>The frames a sheet of <paramref name="scenePoses"/> holds.</summary>
    private static IEnumerable<string> FrameNames(List<ScenePoses> scenePoses) =>
        scenePoses.SelectMany(entry => entry.Poses.Select(pose => CinematicSpriteSheet.FrameName(entry.Family, pose)));

    /// <summary>The poses one family composes for one scene.</summary>
    /// <param name="Family">The family.</param>
    /// <param name="Poses">The poses of the family the scene's actors show.</param>
    private sealed record ScenePoses(CinematicSpriteFamily Family, HashSet<CinematicSpritePose> Poses);

    /// <summary>What the intro sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportIntroCinematicArtwork intro)
    {
        Ensure.NotNull(intro);
        return Sources(Families(intro), intro);
    }

    /// <summary>
    /// What a sheet of the opening's <paramref name="families"/> over the intro OBJ page of <paramref name="intro"/>
    /// composes: each family's poses in the sprite set of the colouring the opening draws that family in (#1513), so the
    /// sheet pictures every frame in its own colours.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A family is not one the opening draws.</exception>
    internal static SpriteSheetSources Sources(IEnumerable<ICinematicSpriteFrames> families, ImportIntroCinematicArtwork intro)
    {
        Ensure.NotNull(intro);
        // The intro's actors draw in each colouring and cross-fade level as shades of the sheet (#1512).
        RgbColor[] sprites = [.. intro.Palette.SpriteColours];
        return Compose(families, IntroObjectPage.Of(intro), (family, _) => IntroSheetInks.SpriteSet(IntroFamilyColourings.Of(family)),
            sprites, IntroSheetShades.Sprites(sprites));
    }

    /// <summary>
    /// What the Ceres flight sheet of <paramref name="flight"/> composes: the poses its actors show, each in its colouring
    /// (#1513), in the flight's colours <paramref name="colours"/>, with a shade for each record the station's navigation
    /// lights show, coloured from <paramref name="paletteFx"/> (#1733).
    /// </summary>
    /// <exception cref="InvalidDataException">A navigation lights record has no installed colour.</exception>
    internal static SpriteSheetSources CeresFlightSources(CeresFlightCharacters flight, CeresFlightPalette colours, IPaletteFxColorSource paletteFx)
    {
        Ensure.NotNull(flight);
        ICinematicSpriteFrames[] families = [.. FlightPoses().Select(entry => CinematicShownFrames.Of(flight.Sprites, entry.Poses, _ => true))];
        return CeresSources(families, CeresSceneActors.Flight(), flight.ObjectPage, Ensure.NotNull(colours),
            CeresSheetInks.StationNavigationLightShades(Ensure.NotNull(paletteFx)));
    }

    /// <summary>
    /// What the Ceres destruction scene's sheet composes: the poses its actors show before the Zebes reveal, from the
    /// scene's destruction and shared Ceres flight families, over the OBJ characters the station's scene loads, coloured
    /// as the scene loads the flight's palette (#1513).
    /// </summary>
    internal static SpriteSheetSources CeresDestructionSources(ImportIntroCinematicArtwork intro) =>
        DestructionSources(intro, SceneFamilies(Ensure.NotNull(intro), DestructionScenePoses()), CeresSceneActors.Destruction(),
            CeresDestructionObjectPage.Characters(intro, zebesRevealed: false), []);

    /// <summary>
    /// What the Zebes reveal's sheet composes: the poses the reveal's actors show over the OBJ characters once the reveal
    /// loads its own (#1513), with a shade for each record the PLANET ZEBES caption's fades show, coloured from
    /// <paramref name="paletteFx"/> (#1733).
    /// </summary>
    /// <exception cref="InvalidDataException">A caption fade record has no installed colour.</exception>
    internal static SpriteSheetSources ZebesRevealSources(ImportIntroCinematicArtwork intro, IPaletteFxColorSource paletteFx) =>
        DestructionSources(intro, SceneFamilies(Ensure.NotNull(intro), ZebesRevealPoses()), CeresSceneActors.ZebesReveal(),
            CeresDestructionObjectPage.Characters(intro, zebesRevealed: true), CeresSheetInks.ZebesTitleFadeShades(Ensure.NotNull(paletteFx)));

    /// <summary>
    /// <paramref name="families"/> over the OBJ <paramref name="characters"/> the scene has loaded, each pose in the
    /// colouring <paramref name="colourings"/> names for it, with <paramref name="effects"/>, the shades of the colour effects
    /// the scene's actors show.
    /// </summary>
    private static SpriteSheetSources DestructionSources(ImportIntroCinematicArtwork intro, IEnumerable<ICinematicSpriteFrames> families,
        Dictionary<CinematicSpritePose, CeresSpriteColouring> colourings, byte[] characters, IReadOnlyList<SheetShade> effects)
    {
        SpriteArtwork page = CeresDestructionObjectPage.Of(characters);
        return CeresSources(families, colourings, page, intro.FlightPalette, effects);
    }

    /// <summary>The frames the Ceres destruction scene's sheet holds: the poses its actors show before the reveal (#1513).</summary>
    internal static IEnumerable<string> CeresDestructionFrameNames() => FrameNames(DestructionScenePoses());

    /// <summary>The frames the Zebes reveal's sheet holds: the poses its actors show (#1513).</summary>
    internal static IEnumerable<string> ZebesRevealFrameNames() => FrameNames(ZebesRevealPoses());

    /// <summary>The frames the intro sheet holds: every pose of the intro's families.</summary>
    internal static IEnumerable<string> FrameNames() =>
        IntroFamilies.SelectMany(family => CinematicSpritePoses.Of(family).Select(pose => CinematicSpriteSheet.FrameName(family, pose)));

    /// <summary>The frames the Ceres flight sheet holds: the poses its actors show (#1513).</summary>
    internal static IEnumerable<string> CeresFlightFrameNames() => FrameNames(FlightPoses());

    /// <summary>
    /// Each Ceres family's composition of every pose over <paramref name="page"/>, each in the sprite set of the colouring
    /// <paramref name="colourings"/> names for it, coloured in the flight's sprite colours with a drawn shade for each
    /// colouring a Ceres actor draws in (#1512, #1513) and <paramref name="effects"/>, the shades of the colour effects the
    /// scene's actors show (#1733). The engine flicker and the floor's navigation lights recolour background colours, which
    /// no sprite shows.
    /// </summary>
    private static SpriteSheetSources CeresSources(IEnumerable<ICinematicSpriteFrames> families,
        Dictionary<CinematicSpritePose, CeresSpriteColouring> colourings, SpriteArtwork page, CeresFlightPalette palette,
        IReadOnlyList<SheetShade> effects)
    {
        RgbColor[] sprites = [.. Ensure.NotNull(palette).SpriteColours];
        return Compose(families, page, (_, pose) => CeresSheetInks.SpriteSet(colourings[pose]), sprites,
            [.. CeresSheetInks.DrawnShades(sprites), .. effects]);
    }

    /// <summary>
    /// Each family's composition of every pose over <paramref name="page"/>, its palette-inheriting parts in the sprite set
    /// <paramref name="set"/> gives the family's pose, coloured in <paramref name="sprites"/> with <paramref name="shades"/>,
    /// at the menus' object sizes the intro and Ceres scenes draw at.
    /// </summary>
    private static SpriteSheetSources Compose(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page,
        Func<CinematicSpriteFamily, CinematicSpritePose, int> set, RgbColor[] sprites, IReadOnlyList<SheetShade> shades)
    {
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection);
        return new SpriteSheetSources([.. Frames(families, page, sizes, set)], sprites, sizes, PartsKeepOwnRows: true) { Shades = shades };
    }

    /// <summary>
    /// Each family's composition of every pose over <paramref name="page"/>, its objects at <paramref name="sizes"/> and its
    /// palette-inheriting parts in the sprite set <paramref name="set"/> gives the family's pose.
    /// </summary>
    internal static IEnumerable<SheetFrameSource> Frames(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page,
        ScreenObjectSizes sizes, Func<CinematicSpriteFamily, CinematicSpritePose, int> set) =>
        Ensure.NotNull(families).SelectMany(family => family.Frames.OrderBy(pair => CinematicSpritemapSources.CartridgeOrder(pair.Key)).Select(pair =>
        {
            string name = CinematicSpriteSheet.FrameName(family.Family, pair.Key);
            return new SheetFrameSource(name, pair.Value.ToFrame(name, 0, sizes), page, set(family.Family, pair.Key));
        }));

}
