using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Reads and writes a <see cref="GameplayMessageTextAlignment"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class GameplayMessageTextAlignmentJsonConverter() : InstalledKeyJsonConverter<GameplayMessageTextAlignment>(GameplayMessageTextAlignmentKeys.Installed);
