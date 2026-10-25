using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

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
        byte[] name = Encoding.UTF8.GetBytes(label);
        AppendLength(name.Length);
        hash.AppendData(name);
        AppendLength(bytes.Length);
        hash.AppendData(bytes);
    }

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

    /// <summary>Colors hash as their 15-bit CGRAM words, identical to <see cref="AppendWords"/> of the encoded words.</summary>
    public void AppendColors(string label, ReadOnlySpan<SuperMetroid.Core.Hardware.RgbColor> colors)
    {
        var bytes = new byte[checked(colors.Length * sizeof(ushort))];
        for (int index = 0; index < colors.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), colors[index].ToWord());
        Append(label, bytes);
    }

    /// <summary>Ordered color rows retain their boundaries, including empty rows; each row hashes its encoded words.</summary>
    public void AppendColorFrames(string label, IReadOnlyList<SuperMetroid.Core.Hardware.RgbColor[]> frames)
    {
        Append(label, frames.Count);
        foreach (SuperMetroid.Core.Hardware.RgbColor[] frame in frames)
            AppendColors("row", frame);
    }

    /// <summary>A name, such as a published artwork identity, hashes as its UTF-8 text.</summary>
    public void AppendName(string label, string name) => Append(label, Encoding.UTF8.GetBytes(name));

    /// <summary>Null fixture domains differ from installed content without storing a derived hash.</summary>
    public void AppendIdentity(string label, string? identity) =>
        Append(label, identity is null ? ReadOnlySpan<byte>.Empty : Convert.FromHexString(identity));

    /// <summary>Preserves part order, offsets, size, flips, priority, palette and character selection.</summary>
    public void AppendEnemyParts(ReadOnlySpan<SpritePart> parts)
    {
        Append("parts", parts.Length);
        foreach (SpritePart part in parts)
            part.AppendIdentity(this);
    }

    /// <summary>Hashes calculated parts with the identical ordering and framing as stored spans.</summary>
    public void AppendEnemyParts(SuperMetroid.Core.Enemies.Artwork.EnemySpritemapParts parts)
    {
        Append("parts", parts.Count);
        for (int index = 0; index < parts.Count; index++)
            parts[index].AppendIdentity(this);
    }
    /// <summary>Frames a field or payload length as a little-endian 32-bit count.</summary>
    private void AppendLength(int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
