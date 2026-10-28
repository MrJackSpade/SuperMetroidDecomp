using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A tilemap transfer of the tiles the credits or the post-shot lower screen wrote.</summary>
/// <param name="Kind">The tilemap transfer.</param>
/// <param name="Tiles">The tiles the transfer wrote.</param>
internal sealed record EndingTilemapUpload(EndingUploadKind Kind, BackgroundTile[] Tiles) : EndingUpload(Kind)
{
    /// <summary>A later transfer of the same tilemap covers this one's tiles, whatever tiles it writes.</summary>
    internal override bool IsReplacedBy(EndingUpload later) =>
        Ensure.NotNull(later) is EndingTilemapUpload tilemap && tilemap.Kind == Kind;
}
