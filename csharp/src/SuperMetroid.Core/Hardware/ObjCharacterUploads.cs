namespace SuperMetroid.Core.Hardware;

/// <summary>
/// What the gameplay OBJ character table shows, as an ordered record of uploads: which owned artwork each uploaded
/// range shows, in upload order. Enemies, enemy projectiles, room sprite objects and the Ceres elevator draw from the
/// page it composes to instead of from emulated VRAM. Installed artwork is named, never copied, so saved state carries
/// only the identities; bytes that came from game memory are kept, because no installed artwork names them.
/// </summary>
[PpuLayoutBoundary("rendering view: the gameplay OBJ character table's uploads")]
public sealed class ObjCharacterUploads
{
    /// <summary>Bytes in the OBJ character table: 512 four-bpp characters of 32 bytes.</summary>
    public const int TableByteCount = 512 * 32;

    /// <summary>The uploads in order; a later upload shows over an earlier one where they overlap.</summary>
    private readonly List<ObjCharacterUpload> _uploads = [];

    /// <summary>The uploads in order.</summary>
    public IReadOnlyList<ObjCharacterUpload> Uploads => _uploads;

    /// <summary>Records <paramref name="byteCount"/> bytes of <paramref name="source"/> shown from table byte <paramref name="firstByte"/>.</summary>
    /// <param name="firstByte">Table byte the shown range starts at.</param>
    /// <param name="byteCount">Bytes shown.</param>
    /// <param name="source">The owned artwork or memory bytes uploaded.</param>
    /// <param name="sourceOffset">Source byte the shown range starts at.</param>
    /// <exception cref="ArgumentOutOfRangeException">The range does not lie inside the table.</exception>
    public void Record(int firstByte, int byteCount, ObjCharacterSource source, int sourceOffset = 0)
    {
        Ensure.NotNull(source);
        if (firstByte < 0 || byteCount <= 0 || firstByte + byteCount > TableByteCount)
            throw new ArgumentOutOfRangeException(nameof(firstByte), firstByte, $"{byteCount} bytes at {firstByte} leave the OBJ character table.");
        if (sourceOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceOffset), sourceOffset, "A source offset is a byte index.");
        // An upload wholly covered by a later one no longer shows anywhere; dropping it keeps the record bounded.
        _uploads.RemoveAll(earlier => earlier.FirstByte >= firstByte && earlier.FirstByte + earlier.ByteCount <= firstByte + byteCount);
        _uploads.Add(new ObjCharacterUpload(firstByte, byteCount, source, sourceOffset));
    }

    /// <summary>The table byte an upload to <paramref name="destination"/> starts at, which lies outside the table for one that misses it.</summary>
    public static int TableByte(VramDmaDestination destination) =>
        (destination.Word - GameplayVramLayout.ObjectCharacters.Offset) * sizeof(ushort);

    /// <summary>Records an encounter's upload over the characters after its room loads (#1392).</summary>
    public void Record(RuntimeCharacterUpload upload) =>
        Record(Ensure.NotNull(upload).FirstByte, upload.ByteCount, upload.Source);

    /// <summary>The upload that shows last over any of <paramref name="byteCount"/> bytes from <paramref name="firstByte"/>, if any has.</summary>
    public ObjCharacterUpload? LastOver(int firstByte, int byteCount)
    {
        for (int index = _uploads.Count - 1; index >= 0; index--)
        {
            ObjCharacterUpload upload = _uploads[index];
            if (upload.FirstByte < firstByte + byteCount && firstByte < upload.FirstByte + upload.ByteCount)
                return upload;
        }
        return null;
    }

    /// <summary>
    /// Records the part of one VRAM upload that lands in the OBJ character table. Uploads that miss the table are
    /// ignored, as the table does not show them.
    /// </summary>
    /// <param name="destination">The upload's VRAM destination.</param>
    /// <param name="byteCount">Bytes uploaded.</param>
    /// <param name="source">The owned artwork or memory bytes uploaded.</param>
    /// <exception cref="InvalidDataException">The upload reaches the table with a non-linear word increment.</exception>
    public void RecordUpload(VramDmaDestination destination, int byteCount, ObjCharacterSource source)
    {
        int uploadStart = TableByte(destination);
        int shownStart = Math.Max(uploadStart, 0);
        int shownEnd = Math.Min(uploadStart + byteCount, TableByteCount);
        if (shownStart >= shownEnd)
            return;
        if (destination.WordIncrement != 1)
            throw new InvalidDataException(
                $"A VRAM upload to word ${destination.Word:X4} reaches the OBJ characters with increment {destination.WordIncrement}; only linear uploads record characters.");
        Record(shownStart, shownEnd - shownStart, source, shownStart - uploadStart);
    }
}
