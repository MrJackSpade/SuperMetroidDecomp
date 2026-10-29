namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colours a sprite palette row holds where what they are is not named (#1464). Neither gameplay actors nor
/// frontend screens draw in it: verification gives it to fixtures and decodes native rows that no member or shared
/// role names into it. It is the one colouring still identified by a row.
/// </summary>
/// <param name="Row">The sprite palette row, 0 through <see cref="SpriteImage.MaximumPaletteRow"/>.</param>
public sealed record UnnamedSpriteRowColouring(int Row) : SpriteColouring
{
    /// <summary>The sprite palette row, 0 through <see cref="SpriteImage.MaximumPaletteRow"/>.</summary>
    public int Row { get; init; } = Ensure.BetweenInclusive(0, SpriteImage.MaximumPaletteRow, Row);
}
