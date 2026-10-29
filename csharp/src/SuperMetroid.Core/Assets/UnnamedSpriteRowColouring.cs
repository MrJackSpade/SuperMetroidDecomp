namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colours a sprite palette row holds where what they are is not yet named (#1464): a frontend screen's sprite row,
/// which the screen's colour owners will name. Gameplay actors no longer draw in it; verification still gives it to
/// actors as a fixture colouring and decodes native rows that no member or shared role names into it. It is the one
/// colouring still identified by a row, and it is removed as those owners are named.
/// </summary>
/// <param name="Row">The sprite palette row, 0 through <see cref="SpriteImage.MaximumPaletteRow"/>.</param>
public sealed record UnnamedSpriteRowColouring(int Row) : SpriteColouring
{
    /// <summary>The sprite palette row, 0 through <see cref="SpriteImage.MaximumPaletteRow"/>.</summary>
    public int Row { get; init; } = Ensure.BetweenInclusive(0, SpriteImage.MaximumPaletteRow, Row);
}
