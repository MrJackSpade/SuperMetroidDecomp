using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>One explosion map, which shares the OBJ page's last characters.</summary>
/// <param name="Fragment">The explosion map loaded.</param>
internal sealed partial record EndingExplosionMapTransfer(EndingExplosionPicture Fragment) : EndingTransfer;
