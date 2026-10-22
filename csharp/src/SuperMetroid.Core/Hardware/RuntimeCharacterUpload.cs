namespace SuperMetroid.Core.Hardware;

/// <summary>
/// An upload over the OBJ characters an encounter makes after its room loads (#1392): the artwork it shows and the table
/// bytes it shows it in. A pre-coloured frame drawn over those bytes has a picture for each upload that can be there.
/// </summary>
/// <param name="Source">The artwork uploaded.</param>
/// <param name="FirstByte">The OBJ character table byte it shows from.</param>
/// <param name="ByteCount">The bytes it shows.</param>
public sealed record RuntimeCharacterUpload(ObjCharacterSource Source, int FirstByte, int ByteCount)
{
    /// <summary>The upload of <paramref name="byteCount"/> bytes of <paramref name="source"/> to <paramref name="destination"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The upload does not lie wholly inside the OBJ character table.</exception>
    public static RuntimeCharacterUpload To(VramDmaDestination destination, int byteCount, ObjCharacterSource source)
    {
        int firstByte = ObjCharacterUploads.TableByte(destination);
        if (firstByte < 0 || byteCount <= 0 || firstByte + byteCount > ObjCharacterUploads.TableByteCount)
            throw new ArgumentOutOfRangeException(nameof(destination), destination.Word,
                $"{byteCount} bytes at VRAM word ${destination.Word:X4} do not lie inside the OBJ character table.");
        return new(Ensure.NotNull(source), firstByte, byteCount);
    }
}
