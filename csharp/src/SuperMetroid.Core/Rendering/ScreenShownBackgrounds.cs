using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's background sets as a frame shows them (#1464): each colour group of a screen's background image
/// shows the screen's set of the same identity.
/// </summary>
/// <param name="sets">The screen's background sets as the frame shows them.</param>
internal sealed class ScreenShownBackgrounds(ShownColourSets sets) : ShownBackgrounds
{
    /// <summary>The screen's background sets as the frame shows them.</summary>
    internal ShownColourSets Sets { get; } = Ensure.NotNull(sets);

    /// <inheritdoc />
    internal override RgbColor Backdrop => Sets.Set(0)[0];

    /// <inheritdoc />
    internal override ReadOnlySpan<RgbColor> Group(SpriteImage sheet, int group) => Sets.Set(group);

    /// <inheritdoc />
    internal override ShownBackgrounds WithBackdrop(RgbColor backdrop) => new ScreenShownBackgrounds(Sets.With(0, 0, backdrop));
}
