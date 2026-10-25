namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>One page of installed Torizo characters, copied from the cartridge as one file.</summary>
/// <param name="Page">The page.</param>
/// <param name="ByteCount">The page's length in bytes.</param>
/// <param name="FileName">The installed file that holds the page.</param>
internal readonly record struct TorizoTileSheet(TorizoTilePage Page, int ByteCount, string FileName);
