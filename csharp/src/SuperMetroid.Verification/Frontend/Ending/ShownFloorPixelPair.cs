namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// What two turning floors lay at one floor point (#1568, #1633): each floor's picture there, by its place among the floor's
/// pictures, the live picture's colour key there, by its place among its sheet's keys, the palette index the memory
/// picture's pixel there was composed as, and whether each pixel is clear.
/// </summary>
/// <param name="LivePicture">The place among the live floor's pictures of the picture laid there.</param>
/// <param name="LiveKey">The place among the live picture's sheet's colour keys of the key of its pixel there.</param>
/// <param name="LiveClear">Whether the live picture's pixel there is clear.</param>
/// <param name="MemoryPicture">The place among the memory floor's pictures of the picture laid there.</param>
/// <param name="MemoryIndex">The palette index the memory picture's pixel there was composed as.</param>
/// <param name="MemoryClear">Whether the memory picture's pixel there is clear.</param>
internal readonly record struct ShownFloorPixelPair(int LivePicture, int LiveKey, bool LiveClear, int MemoryPicture, int MemoryIndex, bool MemoryClear);
