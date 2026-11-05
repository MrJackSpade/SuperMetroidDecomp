namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Where a rotting corpse lies in one of its frame's pictures (#1531): the picture row the corpse's first pixel row lies at,
/// and for each of the picture's eight-pixel character columns how it shows the corpse rotting, or none for
/// a column that shows no rotting rows. A column shows one column of the corpse, and its rotting rows begin at the band its
/// flesh begins at, which no move of a row above it reaches. Every other pixel of the picture shows itself. The import
/// that installs a layout requires at least one column.
/// </summary>
/// <param name="Top">The picture row the corpse's first pixel row lies at, which may lie above the picture.</param>
/// <param name="Columns">
/// How each character column shows the corpse rotting, from the picture's left; none for a column that shows no rotting rows.
/// </param>
public sealed record CorpsePictureLayout(int Top, IReadOnlyList<CorpseColumnRot?> Columns);
