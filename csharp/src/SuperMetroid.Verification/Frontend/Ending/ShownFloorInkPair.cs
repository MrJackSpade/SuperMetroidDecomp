using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>What two turning floors lay at one floor point (#1568): each floor's picture there, by its place among the floor's pictures, and that picture's ink.</summary>
/// <param name="LivePicture">The place among the live floor's pictures of the picture laid there.</param>
/// <param name="LiveInk">The live picture's ink there.</param>
/// <param name="MemoryPicture">The place among the memory floor's pictures of the picture laid there.</param>
/// <param name="MemoryInk">The memory picture's ink there.</param>
internal readonly record struct ShownFloorInkPair(int LivePicture, SpriteImageInk LiveInk, int MemoryPicture, SpriteImageInk MemoryInk);
