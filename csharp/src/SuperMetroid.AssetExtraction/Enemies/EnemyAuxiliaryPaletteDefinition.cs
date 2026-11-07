using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One auxiliary palette family: how many colour frames it has and how many colours each frame holds.</summary>
/// <param name="Id">The family.</param>
/// <param name="FrameCount">The family's colour frames.</param>
/// <param name="ColorCount">The colours one frame holds.</param>
internal readonly record struct EnemyAuxiliaryPaletteDefinition(EnemyAuxiliaryPalette Id, int FrameCount, int ColorCount);
