namespace SuperMetroid.Core.Hardware;

/// <summary>
/// What shows over the OBJ character table's characters as background characters (#1392): the Ceres Ridley arena points
/// BG1 and BG2 at the table, so its blocks show the uploads it records. An upload shows under the name of its source.
/// </summary>
/// <param name="uploads">The table's uploads.</param>
[PpuLayoutBoundary("codec: the OBJ character table's characters as background characters")]
public sealed class ObjPageCharacterArtwork(ObjCharacterUploads uploads) : IBgCharacterArtwork
{
    /// <inheritdoc />
    public BgArtworkShown Showing(int firstCharacter, int characterCount)
    {
        int firstByte = firstCharacter * ObjCharacterUploads.CharacterByteCount;
        int byteCount = characterCount * ObjCharacterUploads.CharacterByteCount;
        ObjCharacterUpload upload = Ensure.NotNull(uploads).LastOver(firstByte, byteCount)
            ?? throw new InvalidDataException($"No OBJ characters are uploaded over characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        if (upload.FirstByte > firstByte || upload.FirstByte + upload.ByteCount < firstByte + byteCount)
            throw new InvalidDataException($"{upload.Source} shows over only some of characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        return new BgArtworkShown(new BgArtworkIdentity($"{upload.Source}"), (firstByte - upload.FirstByte + upload.SourceOffset) / sizeof(ushort));
    }
}
