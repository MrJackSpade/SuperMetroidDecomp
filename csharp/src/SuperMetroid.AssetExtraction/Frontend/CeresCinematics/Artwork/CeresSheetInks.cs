using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Where the Ceres cinematics' colours lie among their sheets' inks (#1512): the sprite set each Ceres actor colouring draws
/// in, the background inks the gunship engine's flicker and the station's navigation lights recolour on the turning floor,
/// and the sprite inks the navigation lights and the PLANET ZEBES caption's fades recolour (#1733). A colour is its set's
/// place times sixteen plus its colour within the set, as the Ceres flight loads its palette.
/// </summary>
internal static class CeresSheetInks
{
    /// <summary>The colours of one set.</summary>
    internal const int SetColours = 16;

    /// <summary>The sprite set each actor colouring draws in.</summary>
    private static readonly Dictionary<CeresSpriteColouring, int> Sets = new()
    {
        [CeresSpriteColouring.SpaceObjects] = 4,
        [CeresSpriteColouring.Station] = 6,
        [CeresSpriteColouring.Zebes] = 7,
        [CeresSpriteColouring.ZebesTitle] = 0,
        [CeresSpriteColouring.Explosions] = 5,
    };

    /// <summary>The background ink the gunship engine's flicker writes first: the last colour of the sixth background set.</summary>
    internal const int EngineFirst = 5 * SetColours + 15;

    /// <summary>The ink after the last the engine's flicker may write: the end of its background set.</summary>
    internal const int EngineEnd = 6 * SetColours;

    /// <summary>
    /// The colour within its set the station's navigation lights write first, on the floor's seventh background set and
    /// on the station sprite's set alike; they may write to the set's end.
    /// </summary>
    private const int NavigationLightsColour = 13;

    /// <summary>The background set the station's navigation lights blink in on the turning floor.</summary>
    private const int NavigationLightsBackgroundSet = 6;

    /// <summary>The colour within the caption's set the PLANET ZEBES caption's fades write first: the one after its transparent one.</summary>
    private const int ZebesTitleFadeColour = 1;

    /// <summary>The sprite set <paramref name="colouring"/> draws in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is not a Ceres actor colouring.</exception>
    internal static int SpriteSet(CeresSpriteColouring colouring) => Sets.TryGetValue(colouring, out int set)
        ? set
        : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "Not a Ceres actor colouring.");

    /// <summary>
    /// The shade of every record the Ceres destruction's gunship engine flicker shows, coloured from
    /// <paramref name="colours"/> over the background inks it recolours, which the gunship's floor shows it in.
    /// </summary>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes past the engine's set.</exception>
    internal static IReadOnlyList<SheetShade> EngineShades(IPaletteFxColorSource colours) =>
        PaletteFxScreenShades.Of([CeresDestructionSceneDefinitions.GunshipEngineFlicker], PaletteFxColours.CutsceneGunshipEngine,
            new InkRange(EngineFirst, EngineEnd), Ensure.NotNull(colours));

    /// <summary>
    /// The shade of every record the Ceres destruction's navigation lights show (#1733), coloured from
    /// <paramref name="colours"/> over the background inks they recolour, which the turning floor shows them in.
    /// </summary>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes past the lights' set.</exception>
    internal static IReadOnlyList<SheetShade> FloorNavigationLightShades(IPaletteFxColorSource colours) =>
        PaletteFxScreenShades.Of([CeresDestructionSceneDefinitions.NavigationLights], PaletteFxColours.CeresNavigationLightsBackground,
            RunToSetEnd(NavigationLightsBackgroundSet, NavigationLightsColour), Ensure.NotNull(colours));

    /// <summary>
    /// The shade of every record the Ceres flight's navigation lights show (#1733), coloured from <paramref name="colours"/>
    /// over the station sprite's inks they recolour.
    /// </summary>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes past the station's set.</exception>
    internal static IReadOnlyList<SheetShade> StationNavigationLightShades(IPaletteFxColorSource colours) =>
        PaletteFxScreenShades.Of([CeresFlightSceneDefinitions.NavigationLights], PaletteFxColours.CeresNavigationLightsSprite,
            RunToSetEnd(SpriteSet(CeresSpriteColouring.Station), NavigationLightsColour), Ensure.NotNull(colours));

    /// <summary>
    /// The shade of every record the PLANET ZEBES caption's fades show (#1733), coloured from <paramref name="colours"/> over
    /// the caption sprite's inks they recolour.
    /// </summary>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes past the caption's set.</exception>
    internal static IReadOnlyList<SheetShade> ZebesTitleFadeShades(IPaletteFxColorSource colours) =>
        PaletteFxScreenShades.Of([CeresDestructionSceneDefinitions.ZebesTextFadeIn, CeresDestructionSceneDefinitions.ZebesTextFadeOut],
            PaletteFxColours.PlanetZebesText, RunToSetEnd(SpriteSet(CeresSpriteColouring.ZebesTitle), ZebesTitleFadeColour), Ensure.NotNull(colours));

    /// <summary>The inks from colour <paramref name="colour"/> of set <paramref name="set"/> to that set's end.</summary>
    private static InkRange RunToSetEnd(int set, int colour) => new(set * SetColours + colour, (set + 1) * SetColours);

    /// <summary>
    /// The drawn shade of every Ceres actor colouring (#1513): its sprite set's colours from <paramref name="sprites"/> over
    /// that set's own inks, which every frame the colouring draws is coloured in, so the shade shows each such frame as the
    /// sheet pictures it and carries whether its draws take part in colour math.
    /// </summary>
    internal static IReadOnlyList<SheetShade> DrawnShades(RgbColor[] sprites)
    {
        Ensure.NotNull(sprites);
        return [.. Enum.GetValues<CeresSpriteColouring>().Select(colouring => SheetShade.DrawnRun(CeresShades.Drawn(colouring),
            SpriteSet(colouring) * SetColours, sprites.AsSpan(SpriteSet(colouring) * SetColours, SetColours), SpriteSet(colouring)))];
    }
}
