using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A result-panel staging tilemap uploaded to one text tilemap.</summary>
/// <param name="Tilemap">The text tilemap the tiles were uploaded to.</param>
/// <param name="Tiles">The tiles the transfer wrote.</param>
internal sealed record EndingTextTilemapUpload(EndingTextTilemap Tilemap, BackgroundTile[] Tiles) : EndingUpload(EndingUploadKind.TextTilemap)
{
    /// <summary>A later upload to the same text tilemap covers this one's tiles, whatever tiles it writes.</summary>
    internal override bool IsReplacedBy(EndingUpload later) =>
        Ensure.NotNull(later) is EndingTextTilemapUpload text && text.Tilemap == Tilemap;
}
