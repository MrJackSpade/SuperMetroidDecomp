using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Reads and writes a <see cref="EnemyAuxiliaryPalette"/> in an installed document by its installed key (#2520).</summary>
internal sealed class EnemyAuxiliaryPaletteJsonConverter() : InstalledKeyJsonConverter<EnemyAuxiliaryPalette>(EnemyAuxiliaryPaletteKeys.Installed);
