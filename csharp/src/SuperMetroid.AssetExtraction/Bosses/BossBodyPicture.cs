namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>One picture of a boss body sheet before it is painted: its sheet frame name, its size in cells and its cells.</summary>
/// <param name="Name">The sheet frame's name.</param>
/// <param name="Columns">The picture's width in cells.</param>
/// <param name="Rows">The picture's height in cells.</param>
/// <param name="Cells">The cells painted; any other is left clear.</param>
internal sealed record BossBodyPicture(string Name, int Columns, int Rows, IReadOnlyList<BossBodyPaintedCell> Cells);
