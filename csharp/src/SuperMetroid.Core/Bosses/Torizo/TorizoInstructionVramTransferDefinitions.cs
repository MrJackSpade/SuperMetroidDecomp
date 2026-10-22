using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Compiled control data for every $814B transfer in the Bomb/Golden Torizo
/// instruction region. Source bytes are editable character art, supplied by
/// <c>TorizoInstructionVramArtwork</c>; no ROM bank is reconstructed at runtime.
/// </summary>
internal static class TorizoInstructionVramTransferDefinitions
{
    /// <summary>Sorted $814B transfer sites with immutable lengths, source identities, and OBJ destinations.</summary>
    internal static readonly TileTransferDefinition[] Entries =
    [
        new(0x0040, 0xaab479, 0x7300),
        new(0x0040, 0xaab679, 0x7400),
        new(0x0020, 0xaab4b9, 0x7e70),
        new(0x0020, 0xaab6b9, 0x7f70),
        new(0x0020, 0xaab4d9, 0x7e50),
        new(0x0020, 0xaab6d9, 0x7f50),
        new(0x0040, 0xaab0a5, 0x7c80),
        new(0x0040, 0xaab0a5, 0x7ca0),
        new(0x0040, 0xaab0a5, 0x7d80),
        new(0x0040, 0xaab0a5, 0x7da0),
        new(0x0040, 0xaab0a5, 0x7e80),
        new(0x0040, 0xaab0a5, 0x7f80),
        new(0x0020, 0xaab0a5, 0x79f0),
        new(0x0040, 0xaab279, 0x7d80),
        new(0x0040, 0xaab2b9, 0x7d80),
        new(0x0040, 0xaab2f9, 0x7d80),
        new(0x0040, 0xaab339, 0x7d80),
        new(0x0040, 0xaab279, 0x7d80),
        new(0x0040, 0xaab2b9, 0x7d80),
        new(0x0040, 0xaab2f9, 0x7d80),
        new(0x0040, 0xaab339, 0x7d80),
        new(0x0600, 0xafe200, 0x6d00),
        new(0x0040, 0xaab279, 0x7d80),
        new(0x0040, 0xaab2b9, 0x7d80),
        new(0x0040, 0xaab2f9, 0x7d80),
        new(0x0040, 0xaab339, 0x7d80),
        new(0x0040, 0xaab279, 0x7d80),
        new(0x0040, 0xaab2b9, 0x7d80),
        new(0x0040, 0xaab2f9, 0x7d80),
        new(0x0040, 0xaab339, 0x7d80),
        new(0x0040, 0xaab4f9, 0x7300),
        new(0x0040, 0xaab6f9, 0x7400),
        new(0x0040, 0xaab539, 0x7300),
        new(0x0040, 0xaab739, 0x7400),
        new(0x0040, 0xaab579, 0x7300),
        new(0x0040, 0xaab779, 0x7400),
        new(0x0040, 0xaab579, 0x7300),
        new(0x0040, 0xaab779, 0x7400),
        new(0x0040, 0xaab539, 0x7300),
        new(0x0040, 0xaab739, 0x7400),
        new(0x0040, 0xaab4f9, 0x7300),
        new(0x0040, 0xaab6f9, 0x7400),
        new(0x0040, 0xafc800, 0x7300),
        new(0x0040, 0xafca00, 0x7400),
        new(0x0040, 0xaab279, 0x7d80),
        new(0x0040, 0xaab2b9, 0x7d80),
        new(0x0040, 0xaab2f9, 0x7d80),
        new(0x0040, 0xaab339, 0x7d80),
        new(0x0040, 0xaab279, 0x7d80),
        new(0x0040, 0xaab2b9, 0x7d80),
        new(0x0040, 0xaab2f9, 0x7d80),
        new(0x0040, 0xaab339, 0x7d80),
    ];

    /// <summary>Each transfer's upload over the OBJ characters, in entry order (#1392).</summary>
    [PpuLayoutBoundary("codec: Torizo instruction-list VRAM transfer records' OBJ destinations ($AA, bank-$A0 $814B)")]
    internal static IReadOnlyList<RuntimeCharacterUpload> Uploads { get; } = [.. Entries.Select((definition, index) => RuntimeCharacterUpload.To(
        VramDmaDestination.At(GameplayVramLayout.ObjectCharacters, definition.DestinationWord - GameplayVramLayout.ObjectCharacters.Offset),
        definition.ByteCount, new TorizoTileTransferCharacters(index)))];
}
