using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Where the opening's colours lie among its sheets' inks (#1512): the colours each cross-fade group moves, the sprite
/// set each actor colouring draws in, and the narration letters' colour variants. A colour is its set's place times
/// sixteen plus its colour within the set, as the opening loads its palette; a background colour is the same ink on the
/// scene and narration sheets, and a sprite colour the same ink on the intro sprite sheet's own-palette parts.
/// </summary>
internal static class IntroSheetInks
{
    /// <summary>The colours of one set.</summary>
    internal const int SetColours = 16;

    /// <summary>The colours of one narration letter colour variant: the two-bit characters' colours.</summary>
    internal const int VariantColours = 4;

    /// <summary>The first background colour each background group moves, and how many it moves.</summary>
    private static readonly Dictionary<IntroBackgroundFade, (int First, int Count)> BackgroundGroups = new()
    {
        [IntroBackgroundFade.GameplayBackground] = (0, 20),
        [IntroBackgroundFade.ClearedBackground] = (0, 16),
        [IntroBackgroundFade.Environment] = (3 * SetColours, 16),
        [IntroBackgroundFade.NarrationText] = (SetColours + 4, 3),
        [IntroBackgroundFade.NarrationBackground] = (7 * SetColours, 16),
        [IntroBackgroundFade.DiscoveryBackground] = (2 * SetColours, 16),
    };

    /// <summary>The first sprite colour each sprite group moves, and how many it moves.</summary>
    private static readonly Dictionary<IntroSpriteFade, (int First, int Count)> SpriteGroups = new()
    {
        [IntroSpriteFade.Samus] = (6 * SetColours + 9, 6),
        [IntroSpriteFade.NarrationPortrait] = (4 * SetColours, 32),
        [IntroSpriteFade.NarrationObjects] = (7 * SetColours, 16),
        [IntroSpriteFade.DiscoveryScientists] = (6 * SetColours, 9),
    };

    /// <summary>The sprite set each actor colouring draws in.</summary>
    private static readonly Dictionary<IntroSpriteColouring, int> Sets = new()
    {
        [IntroSpriteColouring.MotherBrain] = 7,
        [IntroSpriteColouring.CrossFadeObjects] = 6,
        [IntroSpriteColouring.DiscoveryActors] = 7,
        [IntroSpriteColouring.Explosions] = 5,
    };

    /// <summary>The background colours <paramref name="colours"/> moves.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The group is not one the opening fades.</exception>
    internal static IEnumerable<int> Background(IntroBackgroundFade colours) => BackgroundGroups.TryGetValue(colours, out var group)
        ? Enumerable.Range(group.First, group.Count)
        : throw new ArgumentOutOfRangeException(nameof(colours), colours, "Not a faded opening background group.");

    /// <summary>The sprite colours <paramref name="colours"/> moves.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The group is not one the opening fades.</exception>
    internal static IEnumerable<int> Sprite(IntroSpriteFade colours) => SpriteGroups.TryGetValue(colours, out var group)
        ? Enumerable.Range(group.First, group.Count)
        : throw new ArgumentOutOfRangeException(nameof(colours), colours, "Not a faded opening sprite group.");

    /// <summary>The sprite set <paramref name="colouring"/> draws in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is not an opening colouring.</exception>
    internal static int SpriteSet(IntroSpriteColouring colouring) => Sets.TryGetValue(colouring, out int set)
        ? set
        : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "Not an opening actor colouring.");
}
