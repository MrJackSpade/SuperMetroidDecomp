namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous four-bit character page an artwork set installs as one file.</summary>
/// <param name="ByteCount">The page's length in bytes.</param>
/// <param name="FileName">The installed file that holds the page.</param>
internal readonly record struct TileSheetDefinition(int ByteCount, string FileName);
