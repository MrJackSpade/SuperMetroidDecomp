using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction;

/// <summary>Where the cartridge stores the standard sprite sheet.</summary>
[EncodingOwner("cartridge address of the standard sprite sheet")]
internal static class StandardObjectArtworkSourceDefinitions
{
    /// <summary>The standard sprite sheet at $9A:D200, shared by gameplay and cinematics.</summary>
    internal static readonly SnesAddress Sheet = new(0x9a, 0xd200);
}
