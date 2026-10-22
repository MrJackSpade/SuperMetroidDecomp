namespace SuperMetroid.Core.Hardware;

/// <summary>
/// What shows over the OBJ character table's VRAM words as background characters (#1392): the Ceres Ridley arena points
/// BG1 and BG2 at the table, so its blocks show the uploads it records. An upload shows under the name of its source.
/// </summary>
/// <param name="uploads">The table's uploads.</param>
[PpuLayoutBoundary("codec: the OBJ character table's VRAM words as background characters")]
public sealed class ObjPageCharacterArtwork(ObjCharacterUploads uploads) : IBgCharacterArtwork
{
    /// <inheritdoc />
    public BgArtworkShown Showing(int firstWord, int wordCount)
    {
        int firstByte = (firstWord - GameplayVramLayout.ObjectCharacters.Offset) * sizeof(ushort);
        int byteCount = wordCount * sizeof(ushort);
        ObjCharacterUpload upload = Ensure.NotNull(uploads).LastOver(firstByte, byteCount)
            ?? throw new InvalidDataException($"No OBJ characters are uploaded over VRAM words ${firstWord:X4}..${firstWord + wordCount - 1:X4}.");
        if (upload.FirstByte > firstByte || upload.FirstByte + upload.ByteCount < firstByte + byteCount)
            throw new InvalidDataException($"{upload.Source} shows over only some of VRAM words ${firstWord:X4}..${firstWord + wordCount - 1:X4}.");
        return new BgArtworkShown(new BgArtworkIdentity($"{upload.Source}"), (firstByte - upload.FirstByte + upload.SourceOffset) / sizeof(ushort));
    }
}
