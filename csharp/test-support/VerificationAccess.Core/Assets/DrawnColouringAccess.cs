namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="DrawnColouring"/> members production does not use.</summary>
internal static class DrawnColouringAccess
{
    /// <summary>Draws a fixture in a numbered sprite row.</summary>
    extension(DrawnColouring)
    {
        /// <summary>A draw in the colours of sprite row <paramref name="row"/>, whose owner a fixture does not name.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The row is not a sprite palette row.</exception>
        internal static DrawnColouring InUnnamedRow(int row) => new(new UnnamedSpriteRowColouring(row), row);
    }
}
