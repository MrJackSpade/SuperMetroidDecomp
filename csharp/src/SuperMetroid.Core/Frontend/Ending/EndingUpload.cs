namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One recorded ending transfer (#1358): its kind, the key that distinguishes transfers of that kind, and the words a
/// tilemap transfer wrote. A later transfer with the same kind and key replaces it, since it covers exactly the same VRAM words.
/// </summary>
/// <param name="Kind">The transfer.</param>
/// <param name="Key">The scene, chunk, fragment or tilemap the transfer names; zero when its kind has only one.</param>
/// <param name="Words">The tilemap words the transfer wrote, or null for a transfer of installed artwork.</param>
internal sealed record EndingUpload(EndingUploadKind Kind, int Key, ushort[]? Words)
{
    /// <summary>Whether <paramref name="later"/> covers exactly the VRAM words this transfer wrote.</summary>
    internal bool IsReplacedBy(EndingUpload later) => Kind == Ensure.NotNull(later).Kind && Key == later.Key;
}
