using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Where the cartridge stores the standard sprite sheet.</summary>
internal static class StandardObjectArtworkSourceDefinitions
{
    /// <summary>The standard sprite sheet at $9A:D200, shared by gameplay and cinematics.</summary>
    internal static readonly SnesAddress Sheet = new(0x9a, 0xd200);
}
