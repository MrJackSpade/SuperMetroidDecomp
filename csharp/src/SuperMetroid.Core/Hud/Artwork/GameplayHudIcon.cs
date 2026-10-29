using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>One installed item icon (#1466): where its two rows of cells sit on the HUD and the tile each shows.</summary>
/// <param name="Anchor">The cell its upper-left tile sits in.</param>
/// <param name="Width">Its width in cells.</param>
/// <param name="Cells">Its tiles, row by row.</param>
internal sealed record GameplayHudIcon(Position16<TilemapCells> Anchor, int Width, ImmutableArray<BackgroundTile> Cells)
{
    /// <summary>Every icon is two cells tall.</summary>
    internal const int Height = 2;

    /// <summary>The HUD cell the icon's <paramref name="cell"/>th tile sits in, counting row by row.</summary>
    internal Position16<TilemapCells> CellAt(int cell) =>
        Anchor + new Offset16<TilemapCells>(checked((short)(cell % Width)), checked((short)(cell / Width)));
}
