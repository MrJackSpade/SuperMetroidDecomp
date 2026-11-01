namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The working buffers one thread reuses for every PNG it encodes with <see cref="RgbaPng.Encode"/> (#1490): the image's
/// rows as the file stores them before compression, and their compressed stream, each grown to the largest image
/// encoded so far. Not shared between threads.
/// </summary>
internal sealed class RgbaPngBuffers : IDisposable
{
    /// <summary>The rows of the image being encoded, each led by its filter byte.</summary>
    private byte[] scanlines = [];

    /// <summary>The compressed rows of the image being encoded.</summary>
    private readonly MemoryStream compressed = new();

    /// <summary>
    /// The first <paramref name="length"/> bytes of the row buffer, grown when an image needs more; they still hold the
    /// previous image's rows, so the caller writes every byte.
    /// </summary>
    internal Span<byte> Scanlines(int length)
    {
        if (scanlines.Length < length)
            scanlines = new byte[length];
        return scanlines.AsSpan(0, length);
    }

    /// <summary>The compressed-row stream, emptied for the next image and keeping its capacity.</summary>
    internal MemoryStream EmptyCompressed()
    {
        compressed.SetLength(0);
        return compressed;
    }

    /// <inheritdoc />
    public void Dispose() => compressed.Dispose();
}
