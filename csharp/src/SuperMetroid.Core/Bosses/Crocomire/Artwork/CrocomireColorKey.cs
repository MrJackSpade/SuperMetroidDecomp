
namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>A colour within one of Crocomire's palette bands.</summary>
/// <param name="Band">The band.</param>
/// <param name="Color">The color.</param>
internal readonly record struct CrocomireColorKey(CrocomireColorCatalog.Band Band, int Color);
