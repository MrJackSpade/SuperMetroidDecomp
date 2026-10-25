namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A tilemap transfer of the words the credits or the post-shot lower screen wrote.</summary>
/// <param name="Kind">The tilemap transfer.</param>
/// <param name="Words">The tilemap words the transfer wrote.</param>
internal sealed record EndingTilemapUpload(EndingUploadKind Kind, ushort[] Words) : EndingUpload(Kind)
{
    /// <summary>A later transfer of the same tilemap covers this one's words, whatever words it writes.</summary>
    internal override bool IsReplacedBy(EndingUpload later) =>
        Ensure.NotNull(later) is EndingTilemapUpload tilemap && tilemap.Kind == Kind;
}
