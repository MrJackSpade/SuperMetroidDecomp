namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One fixed native takeoff page; only its character pixels are editable.</summary>
/// <param name="SourceAddress">The page's cartridge source, which names its installed characters.</param>
/// <param name="Asset">The installed page.</param>
internal readonly record struct GunshipLiftoffTransferDefinition(
    int SourceAddress, VramAssetId Asset);
