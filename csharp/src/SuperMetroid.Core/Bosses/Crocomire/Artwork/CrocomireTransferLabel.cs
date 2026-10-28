namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>One of Crocomire's colour transfers and the label its content identity hashes it under.</summary>
/// <param name="Band">The transfer.</param>
/// <param name="Label">The label the transfer's colours are hashed under.</param>
internal readonly record struct CrocomireTransferLabel(CrocomireColorCatalog.Band Band, string Label);
