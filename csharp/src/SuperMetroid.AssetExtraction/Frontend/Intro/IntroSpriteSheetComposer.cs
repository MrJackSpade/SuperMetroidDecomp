using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening's cinematic sheets (#1392): every family's composition of each pose over its screen's OBJ page,
/// its palette-inheriting parts in the sheet's frame palette, coloured in the screen's colours with a shade for each colouring its actors draw in (#1512). The intro sheet
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

    /// <summary>
    /// The Ceres scene's families as the Zebes reveal draws them (#1513): the reveal clears the station's actors as it
    /// loads its own characters, and from then on only the planet, the four star sheets and the title run.
    /// </summary>
    /// <exception cref="InvalidDataException">The reveal's actors show a pose no Ceres family composes.</exception>
    private static ICinematicSpriteFrames[] ZebesRevealFamilies(IntroCinematicArtwork intro) =>
        SceneFamilies("The Zebes reveal", CeresDestructionFamilies(intro), CeresSceneActors.ZebesReveal());

    /// <summary>
    /// The Ceres scene's families as the destruction draws them before the Zebes reveal (#1513): its asteroids, vortex and
    /// blasts. The planet and its title are the reveal's, drawn only over the characters the reveal loads.
    /// </summary>
    /// <exception cref="InvalidDataException">The destruction's actors show a pose no Ceres family composes.</exception>
    private static ICinematicSpriteFrames[] DestructionSceneFamilies(IntroCinematicArtwork intro) =>
        SceneFamilies("The Ceres destruction", CeresDestructionFamilies(intro), CeresSceneActors.Destruction());

    /// <summary>The Ceres flight's family as the flight draws it (#1513): the poses its stars, asteroids, vortex and station show.</summary>
    /// <exception cref="InvalidDataException">The flight's actors show a pose its family does not compose.</exception>
    private static ICinematicSpriteFrames[] FlightFamilies(CeresFlightArtwork flight) =>
        SceneFamilies("The Ceres flight", [CeresFlightCharacters.Of(flight).Sprites], CeresSceneActors.Flight());

    /// <summary>
    /// <paramref name="families"/> keeping only the poses a Ceres scene's actors show, as <paramref name="poses"/> holds
    /// them (#1513), each drawn from the family that composes it for the scene: a pose of the destruction's own from the
    /// destruction family and any other from the flight's. A pose the scene never draws is not pictured: its characters
    /// are another scene's.
    /// </summary>
    /// <exception cref="InvalidDataException">The scene shows a pose none of its families composes.</exception>
    private static ICinematicSpriteFrames[] SceneFamilies(string scene, IEnumerable<ICinematicSpriteFrames> families,
        Dictionary<CinematicSpritePose, CeresSpriteColouring> poses)
    {
        var shown = new HashSet<CinematicSpritePose>(poses.Keys);
        var destruction = new HashSet<CinematicSpritePose>(CeresDestructionSpriteDefinitions.Poses);
        ICinematicSpriteFrames[] shownFamilies = [.. families.Select(family => CinematicShownFrames.Of(family, shown,
            pose => destruction.Contains(pose) == (family.Family == CinematicSpriteFamily.CeresDestruction)))];
        CinematicShownFrames.RequireComposed(scene, shown, shownFamilies);
        return shownFamilies;
    }

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
        // The intro's actors draw in each colouring and cross-fade level as shades of the sheet (#1512).
        RgbColor[] sprites = [.. IntroCinematicSourceArtwork.Of(intro).Palette.SpriteColours];
        return Compose(families, IntroObjectPage.Of(intro), sprites,
            IntroSheetShades.Sprites(sprites, FrontendSheetPalette.FrameColourGroup.Number * IntroSheetInks.SetColours));
    }

    /// <summary>What the Ceres flight sheet of <paramref name="flight"/> composes.</summary>
    internal static SpriteSheetSources CeresFlightSources(CeresFlightArtwork flight)
    {
        Ensure.NotNull(flight);
        CeresFlightCharacters characters = CeresFlightCharacters.Of(flight);
        return CeresSources(FlightFamilies(flight), characters.ObjectPage, characters.Palette);
    }

    /// <summary>
    /// What the Ceres destruction scene's sheet composes: the poses its actors show before the Zebes reveal, from the
    /// scene's destruction and shared Ceres flight families, over the OBJ characters the station's scene loads, coloured
    /// as the scene loads the flight's palette (#1513).
    /// </summary>
    internal static SpriteSheetSources CeresDestructionSources(IntroCinematicArtwork intro) =>
        DestructionSources(intro, DestructionSceneFamilies(Ensure.NotNull(intro)),CeresDestructionObjectPage.Characters(intro, zebesRevealed: false));

    /// <summary>
    /// What the Zebes reveal's sheet composes: the poses the reveal's actors show over the OBJ characters once the reveal
    /// loads its own (#1513).
    /// </summary>
    internal static SpriteSheetSources ZebesRevealSources(IntroCinematicArtwork intro) =>
        DestructionSources(intro, ZebesRevealFamilies(Ensure.NotNull(intro)), CeresDestructionObjectPage.Characters(intro, zebesRevealed: true));

    /// <summary><paramref name="families"/> over the OBJ <paramref name="characters"/> the scene has loaded.</summary>
    private static SpriteSheetSources DestructionSources(IntroCinematicArtwork intro, IEnumerable<ICinematicSpriteFrames> families, byte[] characters)
    {
        SpriteArtwork page = CeresDestructionObjectPage.Of(characters);
        return CeresSources(families, page, CeresFlightCharacters.Of(intro.CeresFlight).Palette);
    }

    /// <summary>The frames the Ceres destruction scene's sheet holds: the poses its actors show before the reveal (#1513).</summary>
    internal static IEnumerable<string> CeresDestructionFrameNames(IntroCinematicArtwork intro) =>
        FrameNames(DestructionSceneFamilies(Ensure.NotNull(intro)));

    /// <summary>The frames the Zebes reveal's sheet holds: the poses its actors show (#1513).</summary>
    internal static IEnumerable<string> ZebesRevealFrameNames(IntroCinematicArtwork intro) =>
        FrameNames(ZebesRevealFamilies(Ensure.NotNull(intro)));

    /// <summary>The frames the intro sheet of <paramref name="intro"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(IntroCinematicArtwork intro) => FrameNames(Families(Ensure.NotNull(intro)));

    /// <summary>The frames the Ceres flight sheet of <paramref name="flight"/> holds: the poses its actors show (#1513).</summary>
    internal static IEnumerable<string> CeresFlightFrameNames(CeresFlightArtwork flight) =>
        FrameNames(FlightFamilies(Ensure.NotNull(flight)));

    /// <summary>
    /// Each Ceres family's composition of every pose over <paramref name="page"/>, coloured in the flight's sprite colours
    /// with a drawn shade for each colouring a Ceres actor draws in (#1512). The engine flicker recolours a background
    /// colour, which no sprite shows, so the sprite sheets need no palette-effect shade.
    /// </summary>
    private static SpriteSheetSources CeresSources(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page, CeresFlightPalette palette)
    {
        RgbColor[] sprites = [.. Ensure.NotNull(palette).SpriteColours];
        return Compose(families, page, sprites,
            CeresSheetInks.DrawnShades(sprites, FrontendSheetPalette.FrameColourGroup.Number * CeresSheetInks.SetColours));
    }

    /// <summary>
    /// Each family's composition of every pose over <paramref name="page"/>, coloured in <paramref name="sprites"/> with
    /// <paramref name="shades"/>, at the menus' object sizes the intro and Ceres scenes draw at.
    /// </summary>
    private static SpriteSheetSources Compose(IEnumerable<ICinematicSpriteFrames> families, SpriteArtwork page, RgbColor[] sprites,
        IReadOnlyList<SheetShade> shades)
    {
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection);
        return new SpriteSheetSources([.. Frames(families, page, sizes)], sprites, sizes, PartsKeepOwnRows: true) { Shades = shades };
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
