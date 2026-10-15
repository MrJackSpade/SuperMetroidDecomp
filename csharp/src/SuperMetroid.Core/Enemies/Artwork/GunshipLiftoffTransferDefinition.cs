namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One fixed native takeoff upload; only its character pixels are editable.</summary>
internal readonly record struct GunshipLiftoffTransferDefinition(
    int SourceAddress, ushort DestinationWord, VramAssetId Asset);
