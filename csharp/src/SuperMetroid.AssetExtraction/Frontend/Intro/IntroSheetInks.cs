using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Where the opening's colours lie among its sheets' inks (#1512, #1597): the colours each crossfaded scene shows, the
/// sprite set each actor colouring draws in, and the narration letters' colour variants. A colour is its set's place times
/// sixteen plus its colour within the set, as the opening loads its palette; a background colour is the same ink on the
/// scene and narration sheets, and a sprite colour the same ink on the intro sprite sheet's own-palette parts.
/// </summary>
internal static class IntroSheetInks
{
    /// <summary>The colours of one set.</summary>
    internal const int SetColours = 16;

    /// <summary>The colours of one narration letter colour variant: the two-bit characters' colours.</summary>
    internal const int VariantColours = 4;

    /// <summary>
    /// The background inks the old Mother Brain fight's lights recolour (#1733): from the fifth colour of the second set, the
    /// flashback room's lit colours, to that set's end.
    /// </summary>
    internal static InkRange FightLights => new(SetColours + 4, 2 * SetColours);

    /// <summary>
    /// The background colours each crossfaded scene shows, as runs in rising order: the narration's text and backdrop with
    /// the first four of the next set, and its portrait; the flashback rooms' lit colours and background; the laboratory's
    /// background. The cartridge fades those first four with the narration on its way out but not on its way back; they
    /// are the narration's both ways (#1597).
    /// </summary>
    private static readonly Dictionary<IntroCrossfadeScene, InkRange[]> BackgroundScenes = new()
    {
        [IntroCrossfadeScene.Narration] = [InkRange.Run(0, 20), InkRange.Run(3 * SetColours, 16)],
        [IntroCrossfadeScene.Flashback] = [InkRange.Run(SetColours + 4, 3), InkRange.Run(7 * SetColours, 16)],
        [IntroCrossfadeScene.Laboratory] = [InkRange.Run(2 * SetColours, 16)],
    };

    /// <summary>
    /// The sprite colours each crossfaded scene shows, as runs in rising order: the flashback Samus's beam colours with the
    /// narration, the flashback's objects and actors, and the laboratory's scientists.
    /// </summary>
    private static readonly Dictionary<IntroCrossfadeScene, InkRange[]> SpriteScenes = new()
    {
        [IntroCrossfadeScene.Narration] = [InkRange.Run(6 * SetColours + 9, 6)],
        [IntroCrossfadeScene.Flashback] = [InkRange.Run(4 * SetColours, 32), InkRange.Run(7 * SetColours, 16)],
        [IntroCrossfadeScene.Laboratory] = [InkRange.Run(6 * SetColours, 9)],
    };

    /// <summary>The sprite set each actor colouring draws in.</summary>
    private static readonly Dictionary<IntroSpriteColouring, int> Sets = new()
    {
        [IntroSpriteColouring.MotherBrain] = 7,
        [IntroSpriteColouring.CrossFadeObjects] = 6,
        [IntroSpriteColouring.DiscoveryActors] = 7,
        [IntroSpriteColouring.Explosions] = 5,
    };

    /// <summary>The background colour runs <paramref name="scene"/> shows, in rising order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The scene is not one the opening crossfades.</exception>
    internal static IReadOnlyList<InkRange> BackgroundRuns(IntroCrossfadeScene scene) => BackgroundScenes.TryGetValue(scene, out InkRange[]? runs)
        ? runs
        : throw new ArgumentOutOfRangeException(nameof(scene), scene, "Not a crossfaded opening scene.");

    /// <summary>The sprite colour runs <paramref name="scene"/> shows, in rising order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The scene is not one the opening crossfades.</exception>
    internal static IReadOnlyList<InkRange> SpriteRuns(IntroCrossfadeScene scene) => SpriteScenes.TryGetValue(scene, out InkRange[]? runs)
        ? runs
        : throw new ArgumentOutOfRangeException(nameof(scene), scene, "Not a crossfaded opening scene.");

    /// <summary>The background colours <paramref name="scene"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The scene is not one the opening crossfades.</exception>
    internal static IEnumerable<int> Background(IntroCrossfadeScene scene) => BackgroundRuns(scene).SelectMany(run => run.Inks);

    /// <summary>The sprite colours <paramref name="scene"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The scene is not one the opening crossfades.</exception>
    internal static IEnumerable<int> Sprite(IntroCrossfadeScene scene) => SpriteRuns(scene).SelectMany(run => run.Inks);

    /// <summary>The sprite set <paramref name="colouring"/> draws in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is not an opening colouring.</exception>
    internal static int SpriteSet(IntroSpriteColouring colouring) => Sets.TryGetValue(colouring, out int set)
        ? set
        : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "Not an opening actor colouring.");
}
