using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Where instruction <c>$84:8764</c> writes the tiles of the four collectible slots' blocks.</summary>
[EncodingOwner("byte offset of the collectible slots in the common room element block table")]
internal static class CollectibleSlotSourceDefinitions
{
    /// <summary>The byte offset, in the common room elements' block table, of the first block the four slots write.</summary>
    internal const int FirstBlockByteOffset = 0x0470;
}
