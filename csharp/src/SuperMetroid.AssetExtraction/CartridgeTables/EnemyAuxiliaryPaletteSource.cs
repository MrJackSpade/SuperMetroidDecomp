using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge stores one auxiliary colour family's rows (#1412).</summary>
/// <param name="FirstRow">The first colour word of the family's first row.</param>
/// <param name="RowStrideColors">The colour words one stored row spans, which may exceed the colours the family uses.</param>
internal readonly record struct EnemyAuxiliaryPaletteSource(SnesAddress FirstRow, int RowStrideColors);
