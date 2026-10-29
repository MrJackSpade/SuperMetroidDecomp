using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Gameplay's background sets as a frame shows them (#1464): each colour group of a gameplay background image shows the
/// set the import pipeline named for it when it installed the image.
/// </summary>
/// <param name="sets">The background sets as the frame shows them.</param>
internal sealed class GameplayShownBackgrounds(ShownBackgroundColourSets sets) : ShownBackgrounds
{
    /// <summary>The background sets as the frame shows them.</summary>
    internal ShownBackgroundColourSets Sets { get; } = Ensure.NotNull(sets);

    /// <inheritdoc />
    internal override RgbColor Backdrop => Sets.Set(BackgroundColourSetDefinitions.Backdrop)[BackgroundColourSetDefinitions.BackdropColour];

    /// <inheritdoc />
    internal override ReadOnlySpan<RgbColor> Group(SpriteImage sheet, int group) => Sets.Set(Ensure.NotNull(sheet).BackgroundSet(group));

    /// <inheritdoc />
    internal override ShownBackgrounds WithBackdrop(RgbColor backdrop) =>
        WithBackgroundColour(BackgroundColourSetDefinitions.Backdrop, BackgroundColourSetDefinitions.BackdropColour, backdrop);

    /// <summary>These colours with colour <paramref name="colour"/> of background set <paramref name="set"/> shown in <paramref name="value"/>.</summary>
    internal GameplayShownBackgrounds WithBackgroundColour(BackgroundColourSet set, int colour, RgbColor value) =>
        new GameplayShownBackgrounds(Sets.With(set, colour, value));
}
