using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A gameplay frame's background colours (#1512): only the solid backdrop. Gameplay's background pictures are coloured
/// from their <see cref="GameplayShading"/>, so no colour group of theirs shows a shared colour set.
/// </summary>
/// <param name="backdrop">The colour shown where nothing is drawn.</param>
internal sealed class GameplayBackdrop(RgbColor backdrop) : ShownBackgrounds
{
    /// <inheritdoc />
    internal override RgbColor Backdrop { get; } = backdrop;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Always: a gameplay picture is coloured from its frame's shading.</exception>
    internal override ReadOnlySpan<RgbColor> Group(SpriteImage sheet, int group) =>
        throw new InvalidOperationException($"Gameplay picture {Ensure.NotNull(sheet).Name} is coloured from its frame's shading, not a colour set.");

    /// <inheritdoc />
    internal override ShownBackgrounds WithBackdrop(RgbColor backdrop) => new GameplayBackdrop(backdrop);
}
