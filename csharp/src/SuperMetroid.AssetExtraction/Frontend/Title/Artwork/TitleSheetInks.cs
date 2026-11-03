using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// Where the title's colours lie among its sheets' inks (#1512): the sprite set each title composition colouring draws in,
/// the copyright's two glyph colours within its set, and the floor inks each console light recolours. An ink is its colour
/// group's place times sixteen plus its colour.
/// </summary>
internal static class TitleSheetInks
{
    /// <summary>The colours of one colour group.</summary>
    internal const int GroupColours = 16;

    /// <summary>The sprite set each colouring draws in.</summary>
    private static readonly Dictionary<TitleSpriteColouring, int> Sets = new()
    {
        [TitleSpriteColouring.Text] = 1,
        [TitleSpriteColouring.Logo] = 2,
        [TitleSpriteColouring.Copyright] = 4,
    };

    /// <summary>The colour of the copyright's set its glyph white is, which the skip restores.</summary>
    internal const int CopyrightWhiteColour = 9;

    /// <summary>The colour of the copyright's set its glyph tint is, which the skip restores.</summary>
    internal const int CopyrightRedColour = 10;

    /// <summary>The first floor ink each console light recolours, and the ink after the last it may recolour.</summary>
    private static readonly Dictionary<PaletteFxColours, (int First, int End)> FloorInks = new()
    {
        [PaletteFxColours.BabyMetroidTubeLight] = (2 * GroupColours + 10, 3 * GroupColours),
        [PaletteFxColours.TitleScreenDisplays] = (2 * GroupColours + 14, 3 * GroupColours),
    };

    /// <summary>The console lights the floor shows.</summary>
    internal static IEnumerable<PaletteFxColours> FloorLights => FloorInks.Keys;

    /// <summary>The sprite set <paramref name="colouring"/> draws in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is not a title colouring.</exception>
    internal static int SpriteSet(TitleSpriteColouring colouring) => Sets.TryGetValue(colouring, out int set)
        ? set
        : throw new ArgumentOutOfRangeException(nameof(colouring), colouring, "Not a title colouring.");

    /// <summary>The floor inks <paramref name="light"/> recolours: its first, and the one after the last it may.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The floor shows no such light.</exception>
    internal static (int First, int End) FloorLight(PaletteFxColours light) => FloorInks.TryGetValue(light, out var inks)
        ? inks
        : throw new ArgumentOutOfRangeException(nameof(light), light, "The title floor shows no such light.");
}
