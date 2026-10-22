using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen (#1358): its palette and the graphics transfers its scenes have made since the last full clear,
/// which the capture replays over the bound artwork. A transfer covering exactly an earlier one's words replaces it, so the
/// record stays as small as the distinct transfers it holds.
/// </summary>
internal sealed class EndingScreen
{
    /// <summary>The transfers since the last full clear, oldest first.</summary>
    internal readonly List<EndingUpload> uploads = [];

    /// <summary>The replayed memory image, until a transfer or a rebind changes it.</summary>
    [NonSerialized]
    private byte[]? image;

    /// <summary>The ending's colors, including partial fades.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>Clears every transfer, as a full VRAM clear does.</summary>
    public void Clear()
    {
        uploads.Clear();
        image = null;
    }

    /// <summary>Records a transfer of installed artwork.</summary>
    public void Record(EndingUploadKind kind, int key) => Record(new EndingUpload(kind, key, null));

    /// <summary>Records a tilemap transfer of <paramref name="words"/>, copying them as they are now.</summary>
    public void Record(EndingUploadKind kind, int key, ReadOnlySpan<ushort> words) => Record(new EndingUpload(kind, key, words.ToArray()));

    /// <summary>How many of the logo's character transfers the post-shot has made.</summary>
    internal int LogoTileTransfers => uploads.Count(upload => upload.Kind == EndingUploadKind.PostShotTransfer &&
        EndingPostShotUploadDefinitions.Get(upload.Key).Source == EndingPostShotUploadSource.LogoTiles);

    /// <summary>Forgets the replayed image after the bound artwork changes.</summary>
    public void Invalidate() => image = null;

    /// <summary>The memory image the transfers leave over <paramref name="artwork"/>.</summary>
    internal byte[] Image(EndingScreenArtwork artwork) => image ??= EndingScreenImage.Compose(uploads, artwork);

    /// <summary>Appends <paramref name="upload"/>, dropping the earlier transfer whose words it covers.</summary>
    private void Record(EndingUpload upload)
    {
        uploads.RemoveAll(earlier => earlier.IsReplacedBy(upload));
        uploads.Add(upload);
        image = null;
    }
}
