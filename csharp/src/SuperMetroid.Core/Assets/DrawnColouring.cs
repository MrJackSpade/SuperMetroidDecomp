namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colours a draw shows in place of the ones its image was coloured with (#1464): what they are, and the sprite
/// palette row that held them when the draw was published. The row is fixed at publication, by the resolver of the
/// graphics set loaded then, so a frame resolves the draw's colours from it until colour sets are keyed by owner.
/// </summary>
public sealed record DrawnColouring
{
    /// <summary>Keeps what the draw shows and the row that holds it.</summary>
    /// <param name="shows">What colours the draw shows.</param>
    /// <param name="row">The sprite palette row holding them at publication.</param>
    /// <exception cref="ArgumentOutOfRangeException">The row is not a sprite palette row.</exception>
    public DrawnColouring(SpriteColouring shows, int row)
    {
        Shows = Ensure.NotNull(shows);
        Row = Ensure.BetweenInclusive(0, SpriteImage.MaximumPaletteRow, row);
    }

    /// <summary>What colours the draw shows.</summary>
    public SpriteColouring Shows { get; }

    /// <summary>The sprite palette row holding them when the draw was published.</summary>
    public int Row { get; }
}
