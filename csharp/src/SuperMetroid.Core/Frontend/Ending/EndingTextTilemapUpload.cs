namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A result-panel staging tilemap uploaded to one text tilemap.</summary>
/// <param name="Tilemap">The text tilemap the words were uploaded to.</param>
/// <param name="Words">The tilemap words the transfer wrote.</param>
internal sealed record EndingTextTilemapUpload(EndingTextTilemap Tilemap, ushort[] Words) : EndingUpload(EndingUploadKind.TextTilemap)
{
    /// <summary>A later upload to the same text tilemap covers this one's words, whatever words it writes.</summary>
    internal override bool IsReplacedBy(EndingUpload later) =>
        Ensure.NotNull(later) is EndingTextTilemapUpload text && text.Tilemap == Tilemap;
}
