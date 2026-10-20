namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous four-bit character page an artwork set copies from the cartridge as one file.</summary>
/// <param name="SourceAddress">The page's first byte on the SNES bus.</param>
/// <param name="ByteCount">The page's length in bytes.</param>
/// <param name="FileName">The installed file that holds the page.</param>
internal readonly record struct TileSheetDefinition(int SourceAddress, int ByteCount, string FileName);
