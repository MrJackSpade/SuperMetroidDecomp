using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Canonical identity of decoded, selected presentation data, not its path or file encoding.
/// Length framing keeps labels, dimensions, and payload boundaries unambiguous.
/// </summary>
internal sealed class SelectedPresentationHash
{
    /// <summary>The running SHA-256 digest of length-framed selected content.</summary>
    private readonly IncrementalHash hash;

    /// <summary>Wraps a fresh incremental digest for one selected-content identity.</summary>
    private SelectedPresentationHash(IncrementalHash hash) => this.hash = hash;

    /// <summary>Hashes a domain label followed by the caller's canonical selected-data fields.</summary>
    public static string Create(string domain, Action<SelectedPresentationHash> append)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var content = new SelectedPresentationHash(hash);
        content.Append("domain", Encoding.UTF8.GetBytes(domain));
        append(content);
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>Hashes shared bytes and source-keyed transfer payloads in numeric source order.</summary>
    public static string FromTransfers<T>(string domain, IReadOnlyDictionary<int, T> sources,
        Func<T, ReadOnlyMemory<byte>> transfer, ReadOnlyMemory<byte> shared = default) =>
        Create(domain, content =>
        {
            content.Append("shared", shared.Span);
            foreach ((int source, T selected) in sources.OrderBy(pair => pair.Key))
            {
                content.Append("source", source);
                content.Append("transfer", transfer(selected).Span);
            }
        });

    /// <summary>Hashes shared bytes and name-keyed transfer payloads in ordinal name order.</summary>
    public static string FromNamedTransfers<T>(string domain, IReadOnlyDictionary<string, T> sources,
        Func<T, ReadOnlyMemory<byte>> transfer, ReadOnlyMemory<byte> shared = default) =>
        Create(domain, content =>
        {
            content.Append("shared", shared.Span);
            foreach ((string source, T selected) in sources.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                content.Append("source", Encoding.UTF8.GetBytes(source));
                content.Append("transfer", transfer(selected).Span);
            }
        });

    /// <summary>Adds a UTF-8 field label and byte payload with explicit lengths to prevent boundary ambiguity.</summary>
    public void Append(string label, ReadOnlySpan<byte> bytes)
    {
        Ensure.NotNull(label);
        // Labels are short literals hashed once per field: encoding them on the stack keeps hashing a large catalog
        // from allocating a label array per field (#1430). The bytes hashed are the same UTF-8 encoding.
        int maximum = Encoding.UTF8.GetMaxByteCount(label.Length);
        Span<byte> buffer = maximum <= LabelBufferBytes ? stackalloc byte[LabelBufferBytes] : new byte[maximum];
        int length = Encoding.UTF8.GetBytes(label, buffer);
        AppendLength(length);
        hash.AppendData(buffer[..length]);
        AppendLength(bytes.Length);
        hash.AppendData(bytes);
    }

    /// <summary>The bytes of a label encoded on the stack; longer labels are encoded on the heap.</summary>
    private const int LabelBufferBytes = 256;

    /// <summary>Adds a signed integer field in four-byte little-endian form.</summary>
    public void Append(string label, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        Append(label, bytes);
    }

    /// <summary>Adds a sequence of 16-bit values in little-endian order.</summary>
    public void AppendWords(string label, ReadOnlySpan<ushort> words)
    {
        var bytes = new byte[checked(words.Length * sizeof(ushort))];
        for (int index = 0; index < words.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), words[index]);
        Append(label, bytes);
    }

    /// <summary>
    /// Block appearances hash as each block number followed by the mirroring's name, so the hash names what a block
    /// shows rather than how a file or the cartridge stores it.
    /// </summary>
    public void AppendAppearances(string label, ReadOnlySpan<RoomBlockAppearance> appearances)
    {
        Append(label, appearances.Length);
        foreach (RoomBlockAppearance appearance in appearances)
        {
            Append("block", appearance.Block);
            AppendName("mirroring", appearance.Mirroring.ToString());
        }
    }

    /// <summary>Colors hash as their 15-bit CGRAM words, identical to <see cref="AppendWords"/> of the encoded words.</summary>
    public void AppendColors(string label, ReadOnlySpan<RgbColor> colors)
    {
        var bytes = new byte[checked(colors.Length * sizeof(ushort))];
        for (int index = 0; index < colors.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), colors[index].ToWord());
        Append(label, bytes);
    }

    /// <summary>Ordered color rows retain their boundaries, including empty rows; each row hashes its encoded words.</summary>
    public void AppendColorFrames(string label, IReadOnlyList<RgbColor[]> frames)
    {
        Append(label, frames.Count);
        foreach (RgbColor[] frame in frames)
            AppendColors("row", frame);
    }

    /// <summary>A name, such as a published artwork identity, hashes as its UTF-8 text.</summary>
    public void AppendName(string label, string name) => Append(label, Encoding.UTF8.GetBytes(name));

    /// <summary>Null fixture domains differ from installed content without storing a derived hash.</summary>
    public void AppendIdentity(string label, string? identity) =>
        Append(label, identity is null ? ReadOnlySpan<byte>.Empty : Convert.FromHexString(identity));

    /// <summary>Frames a field or payload length as a little-endian 32-bit count.</summary>
    private void AppendLength(int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
