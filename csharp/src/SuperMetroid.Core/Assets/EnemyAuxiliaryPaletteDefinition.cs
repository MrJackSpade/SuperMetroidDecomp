namespace SuperMetroid.Core.Assets;

/// <summary>Native source address and fixed frame and color dimensions for one auxiliary palette family.</summary>
internal readonly record struct EnemyAuxiliaryPaletteDefinition(
    EnemyAuxiliaryPalette Id, int SourceAddress, int FrameCount, int ColorCount, int NativeFrameStrideColors);
