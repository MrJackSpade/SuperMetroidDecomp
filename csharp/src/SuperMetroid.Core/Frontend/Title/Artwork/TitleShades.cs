using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The shades the import pre-colours the title's sheets in (#1512): each colouring a title composition draws in, and the
/// copyright's colours as the title's skip restores them. The floor's console lights are colour-effect shades
/// (<see cref="Fx.ColourEffects.ColourEffectShades"/>). Which inks each recolours, and to what, is the import's.
/// </summary>
public static partial class TitleShades
{
    /// <summary>
    /// The installed name of each colouring's shade (#2511), written out so renaming a colouring's identity never changes a
    /// name on disk.
    /// </summary>
    private static readonly FrozenDictionary<TitleSpriteColouring, string> DrawnKeys =
        new Dictionary<TitleSpriteColouring, string>
        {
            [TitleSpriteColouring.Text] = "TitleText",
            [TitleSpriteColouring.Logo] = "TitleLogo",
            [TitleSpriteColouring.Copyright] = "TitleCopyright",
        }.ToFrozenDictionary();

    /// <summary>A title composition's parts drawn in <paramref name="colouring"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring has no installed name.</exception>
    public static PictureShade Drawn(TitleSpriteColouring colouring) => new(Ensure.Listed(DrawnKeys, colouring), 0);

    /// <summary>
    /// The copyright's glyph colours as the skip to the title screen restores them, laid over the copyright's drawn shade
    /// once the title screen is rebuilt.
    /// </summary>
    public static PictureShade RestoredCopyright { get; } = new("TitleRestoredCopyright", 0);
}
