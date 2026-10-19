using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Composes the gameplay OBJ character table's uploads into the page enemy-format sprites draw from: each upload's
/// resolved bytes over a blank table, in upload order. The page is recomposed only when an upload was recorded or an
/// upload's artwork resolves to different bytes, so a steady room keeps publishing one page.
/// </summary>
public sealed class ObjCharacterTablePage
{
    /// <summary>The last composed page.</summary>
    private SpriteArtwork? page;

    /// <summary>The upload record and the resolved source of each upload the cached page was composed from.</summary>
    private (ObjCharacterUpload[] Uploads, ReadOnlyMemory<byte>[] Sources)? composedFrom;

    /// <summary>The page <paramref name="uploads"/> show with their artwork as <paramref name="resolver"/> resolves it now.</summary>
    /// <param name="uploads">The OBJ character table's uploads.</param>
    /// <param name="resolver">Resolves installed artwork identities to their current bytes.</param>
    /// <exception cref="InvalidDataException">An upload's source is shorter than the range it shows.</exception>
    public SpriteArtwork Page(ObjCharacterUploads uploads, IObjCharacterSourceResolver resolver)
    {
        Ensure.NotNull(uploads);
        Ensure.NotNull(resolver);
        ObjCharacterUpload[] current = [.. uploads.Uploads];
        var resolved = new ReadOnlyMemory<byte>[current.Length];
        for (int index = 0; index < current.Length; index++)
            resolved[index] = current[index].Source.Resolve(resolver);
        if (page is not null && composedFrom is { } previous &&
            previous.Uploads.SequenceEqual(current) && SameSources(previous.Sources, resolved))
            return page;

        var table = new byte[ObjCharacterUploads.TableByteCount];
        for (int index = 0; index < current.Length; index++)
        {
            ObjCharacterUpload upload = current[index];
            ReadOnlyMemory<byte> bytes = resolved[index];
            if (upload.SourceOffset + upload.ByteCount > bytes.Length)
                throw new InvalidDataException(
                    $"OBJ upload {upload.Source} has {bytes.Length} bytes; it shows {upload.ByteCount} from byte {upload.SourceOffset}.");
            bytes.Span.Slice(upload.SourceOffset, upload.ByteCount).CopyTo(table.AsSpan(upload.FirstByte));
        }
        page = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
        composedFrom = (current, resolved);
        return page;
    }

    /// <summary>Whether two resolutions name the same memory, so the page composed from one shows the other.</summary>
    private static bool SameSources(ReadOnlyMemory<byte>[] composed, ReadOnlyMemory<byte>[] current)
    {
        if (composed.Length != current.Length)
            return false;
        for (int index = 0; index < composed.Length; index++)
        {
            if (!MemoryMarshal.TryGetArray(composed[index], out ArraySegment<byte> before) ||
                !MemoryMarshal.TryGetArray(current[index], out ArraySegment<byte> after) ||
                !ReferenceEquals(before.Array, after.Array) || before.Offset != after.Offset || before.Count != after.Count)
                return false;
        }
        return true;
    }
}
