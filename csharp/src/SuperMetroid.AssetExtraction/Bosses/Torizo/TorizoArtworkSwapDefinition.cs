using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>One Torizo artwork swap: which installed characters it shows.</summary>
/// <param name="Page">The page of installed characters the swap shows from.</param>
/// <param name="PageByte">The byte of the page the shown characters start at.</param>
/// <param name="ByteCount">The bytes of characters the swap shows.</param>
internal readonly record struct TorizoArtworkSwapDefinition(TorizoTilePage Page, int PageByte, int ByteCount);
