namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>An inclusive run of eight-pixel cells in the menu character atlas, numbered as the native tilemaps number them.</summary>
/// <param name="First">The run's first atlas cell.</param>
/// <param name="Last">The run's last atlas cell.</param>
internal readonly record struct MenuAtlasCellRange(int First, int Last)
{
    /// <summary>A run of one cell.</summary>
    internal MenuAtlasCellRange(int cell) : this(cell, cell)
    {
    }
}
