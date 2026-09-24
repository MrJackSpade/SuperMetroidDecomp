using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Indexed four-bit artwork for Crocomire's two melting images. The native erase order,
/// distortion, timing and VRAM destinations remain in the enemy mechanics.
/// </summary>
public sealed class CrocomireMeltingArtwork
{
    private readonly RoomCharacterAtlas first;
    private readonly RoomCharacterAtlas second;

    private CrocomireMeltingArtwork(RoomCharacterAtlas first, RoomCharacterAtlas second)
    {
        this.first = first;
        this.second = second;
    }

    /// <summary>Compiles both installed PNG sheets, rejecting edits outside their used bytes.</summary>
    public static CrocomireMeltingArtwork Load(Stream firstPng, Stream secondPng)
    {
        ArgumentNullException.ThrowIfNull(firstPng);
        ArgumentNullException.ThrowIfNull(secondPng);
        RoomCharacterAtlas first = RoomCharacterAtlas.Load(firstPng,
            CrocomireMeltingArtworkFormat.FirstByteCount);
        RoomCharacterAtlas second = RoomCharacterAtlas.Load(secondPng,
            CrocomireMeltingArtworkFormat.SecondByteCount);
        ValidatePadding(first, CrocomireMeltingTransferDefinitions.Passes[0], "first");
        ValidatePadding(second, CrocomireMeltingTransferDefinitions.Passes[1], "second");
        return new(first, second);
    }

    /// <summary>
    /// Replaces only the byte range written by the native overlapping copies. The rest of
    /// the scratch allocation retains its prior value, including across the second pass.
    /// </summary>
    internal void CopyPassTo(ushort headerOffset, Span<byte> scratch)
    {
        CrocomireMeltingPass pass = CrocomireMeltingTransferDefinitions.Header(headerOffset);
        ReadOnlySpan<byte> image = (headerOffset ==
            CrocomireMeltingTransferDefinitions.FirstHeaderOffset ? first : second).Transfer.Span;
        int used = UsedByteCount(pass);
        if (scratch.Length < used)
            throw new InvalidDataException("Crocomire melting scratch image is too small.");
        image[..used].CopyTo(scratch);
    }

    internal static int UsedByteCount(CrocomireMeltingPass pass)
    {
        int end = 0;
        foreach (CrocomireMeltingCopy copy in pass.Copies.Span)
            end = Math.Max(end, copy.DestinationWord - 0x4000 + (pass.WordsToCopy + 1) * 2);
        return end;
    }

    private static void ValidatePadding(RoomCharacterAtlas atlas, CrocomireMeltingPass pass,
        string name)
    {
        ReadOnlySpan<byte> bytes = atlas.Transfer.Span;
        int used = UsedByteCount(pass);
        if (bytes[used..].IndexOfAnyExcept((byte)0) >= 0)
            throw new InvalidDataException(
                $"Crocomire {name} melt PNG contains pixels outside the native scratch image.");
    }
}

/// <summary>Stable filenames and exact four-bit PNG transfer geometry for both melts.</summary>
public static class CrocomireMeltingArtworkFormat
{
    public const string FirstFileName = "crocomire-melt-first.png";
    public const string SecondFileName = "crocomire-melt-second.png";
    public const int FirstByteCount = 0x0e20;
    public const int SecondByteCount = 0x1020;
}
