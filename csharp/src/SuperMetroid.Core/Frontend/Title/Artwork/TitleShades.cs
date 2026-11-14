using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The shades the import pre-colours the title's sheets in (#1512): each colouring a title composition draws in, and the
/// copyright's colours as the title's skip restores them. The floor's console lights are palette-effect shades
/// (<see cref="Fx.ColourEffects.ColourEffectShades"/>). Which inks each recolours, and to what, is the import's.
/// </summary>
public static class TitleShades
{
    /// <summary>A title composition's parts drawn in <paramref name="colouring"/>.</summary>
    public static PictureShade Drawn(TitleSpriteColouring colouring) => new($"Title{colouring}", 0);

    /// <summary>
    /// The copyright's glyph colours as the skip to the title screen restores them, laid over the copyright's drawn shade
    /// once the title screen's objects are rebuilt.
    /// </summary>
    public static PictureShade RestoredCopyright { get; } = new("TitleRestoredCopyright", 0);
}
