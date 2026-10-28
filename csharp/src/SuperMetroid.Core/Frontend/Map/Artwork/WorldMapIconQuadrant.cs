namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Where a cell sits in a two-by-two icon, and which of the icon's highlights it shows.</summary>
/// <param name="Right">Whether the cell is in the icon's right column.</param>
/// <param name="Lower">Whether the cell is in the icon's lower row.</param>
/// <param name="VerticalHighlight">Whether the icon's vertical arm is highlighted.</param>
/// <param name="HorizontalHighlight">Whether the icon's horizontal arm is highlighted.</param>
[IndependentFlag("a cell's column, row and the icon's two highlighted arms are independent facts of the cell")]
internal readonly record struct WorldMapIconQuadrant(bool Right, bool Lower, bool VerticalHighlight, bool HorizontalHighlight);
