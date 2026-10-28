using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The tiles of one two-cell stalactite drawing in the intro cave.</summary>
/// <param name="Top">The first of the two top-row tiles; the second follows it in the atlas.</param>
/// <param name="LowerLeft">The lower row's left tile.</param>
/// <param name="LowerRight">The lower row's right tile.</param>
internal sealed record IntroCeilingPointTiles(BackgroundTile Top, BackgroundTile LowerLeft, BackgroundTile LowerRight);
