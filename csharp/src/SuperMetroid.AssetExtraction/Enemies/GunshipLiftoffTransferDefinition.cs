using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One fixed takeoff page; only its character pixels are editable.</summary>
/// <param name="Asset">The installed page.</param>
internal readonly record struct GunshipLiftoffTransferDefinition(InstalledArtworkId Asset);
