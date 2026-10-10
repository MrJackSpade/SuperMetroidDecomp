using System.Text;

/// <summary>
/// The Snes9x 1.60 snapshot a snapshot-start SMV begins from. Only the memory images the
/// one-time initial-state import reads are retained: cartridge SRAM. Work RAM comes from the
/// native capture's first input boundary, which completes the snapshot's frame.
/// </summary>
internal sealed class ReplaySnapshot
{
    private const string Signature = "#!s9xsnp:0011\n";

    private ReplaySnapshot(byte[] saveRam) => SaveRam = saveRam;

    /// <summary>Snes9x's whole SRAM buffer (<c>SRA:</c> block); the cartridge uses its first 8 KiB.</summary>
    public byte[] SaveRam { get; }

    /// <summary>
    /// Parses a decompressed version-11 snapshot: the signature line followed by blocks of a
    /// three-character name, a colon, a six-digit decimal length and that many bytes.
    /// </summary>
    public static ReplaySnapshot Parse(byte[] image)
    {
        if (image.Length < Signature.Length ||
            Encoding.ASCII.GetString(image, 0, Signature.Length) != Signature)
            throw new InvalidDataException("Movie start state is not a Snes9x version-11 snapshot.");
        var blocks = new Dictionary<string, byte[]>();
        int position = Signature.Length;
        while (position < image.Length)
        {
            const int HeaderLength = 11;
            if (position + HeaderLength > image.Length || image[position + 3] != ':')
                throw new InvalidDataException($"Malformed snapshot block header at byte {position}.");
            string name = Encoding.ASCII.GetString(image, position, 3);
            int length = int.Parse(Encoding.ASCII.GetString(image, position + 4, 6),
                System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture);
            position += HeaderLength;
            if (position + length > image.Length)
                throw new InvalidDataException($"Snapshot block {name} overruns the snapshot.");
            if (!blocks.TryAdd(name, image.AsSpan(position, length).ToArray()))
                throw new InvalidDataException($"Snapshot repeats block {name}.");
            position += length;
        }
        byte[] Block(string name, int length) =>
            blocks.TryGetValue(name, out byte[]? data) && data.Length == length ? data
                : throw new InvalidDataException($"Snapshot block {name} is missing or is not {length} bytes.");
        return new ReplaySnapshot(Block("SRA", 0x80000));
    }
}
