namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>What one Torizo artwork look copies over the sprite characters: which installed characters, and where.</summary>
/// <param name="Page">The page of installed characters the look copies from.</param>
/// <param name="PageByte">The byte of the page the copied characters start at.</param>
/// <param name="ByteCount">The bytes of characters the look copies.</param>
/// <param name="FirstCharacter">The sprite character the copied characters begin at.</param>
internal readonly record struct TorizoLookUpload(TorizoTilePage Page, int PageByte, int ByteCount, int FirstCharacter);
