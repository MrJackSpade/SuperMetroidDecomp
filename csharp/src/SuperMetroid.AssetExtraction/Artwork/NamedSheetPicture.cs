namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One painted picture a sheet holds and the names of the frames that show it (#1513).</summary>
/// <param name="Picture">The picture.</param>
/// <param name="Names">The names of the frames that show it, each once.</param>
internal sealed record NamedSheetPicture(SheetPicture Picture, List<string> Names);
