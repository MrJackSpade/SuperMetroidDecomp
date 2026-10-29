using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The colours a frontend screen's sprite draws in, named by the screen's colour group (#1464): a whole sprite set the
/// screen holds under <paramref name="Group"/>, such as the title logo's or the opening's explosions' colours.
/// </summary>
/// <param name="Group">The screen colour group, one whole sprite set, the sprite draws in.</param>
public sealed record ScreenSpriteColouring(ScreenColourGroup Group) : SpriteColouring
{
    /// <summary>The sprite set the screen holds the group's colours in.</summary>
    internal int SpriteSet => ScreenColourGroups.SpriteSet(Group);

    /// <summary>
    /// What a frontend sheet frame's palette-inheriting parts show when drawn in <paramref name="group"/>'s colours, or
    /// null where the sheet coloured them in that very set.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="group"/> is not one whole sprite set.</exception>
    internal static DrawnColouring? Drawn(ScreenColourGroup group)
    {
        var colouring = new ScreenSpriteColouring(group);
        return colouring.SpriteSet == FrontendSheetPalette.FramePalette ? null : new DrawnColouring(colouring, colouring.SpriteSet);
    }
}
