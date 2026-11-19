using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Reads and writes a <see cref="PauseAtlas"/> in an installed document by its installed key (#2520).</summary>
internal sealed class PauseAtlasJsonConverter() : InstalledKeyJsonConverter<PauseAtlas>(PauseAtlasKeys.Installed);
