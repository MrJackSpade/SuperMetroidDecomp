namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One 8x8 cell of a boss body picture that a column dissolve erases (#1652): where the cell lies in the picture, the band
/// of the dissolve whose step it shows, and the group of eight pixel columns of the dissolved image it shows. A step of the
/// dissolve erases one pixel column of one group in every band; a cell shows its band's erasures of its own group.
/// </summary>
/// <param name="Column">The cell's column in the picture, counted in cells from its left.</param>
/// <param name="Row">The cell's row in the picture, counted in cells from its top.</param>
/// <param name="Band">The dissolve band the cell shows.</param>
/// <param name="Group">The group of eight pixel columns of the dissolved image the cell shows.</param>
public readonly record struct DissolvingCell(int Column, int Row, int Band, int Group);
