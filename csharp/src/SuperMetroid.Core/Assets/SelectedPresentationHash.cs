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
    /// <summary>Incremental SHA-256 state receiving length-framed labels and payloads.</summary>
    private readonly IncrementalHash hash;

    /// <summary>Continues appending canonical content to an existing incremental digest.</summary>
    /// <param name="hash">SHA-256 state owned and finalized by the creating operation.</param>
    private SelectedPresentationHash(IncrementalHash hash) => this.hash = hash;

    /// <summary>Creates a SHA-256 identity scoped by a domain label and caller-supplied content fields.</summary>
    /// <param name="domain">Stable name separating this hash from other presentation domains.</param>
    /// <param name="append">Writes the selected content fields into the canonical hash stream.</param>
    /// <returns>Uppercase hexadecimal SHA-256 of the framed domain and appended data.</returns>
    public static string Create(string domain, Action<SelectedPresentationHash> append)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var content = new SelectedPresentationHash(hash);
        content.Append("domain", Encoding.UTF8.GetBytes(domain));
        append(content);
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>Hashes a shared transfer and source-keyed transfers in key order, independent of dictionary insertion order.</summary>
    /// <param name="domain">Stable name separating this catalog's identity.</param>
    /// <param name="sources">Selected transfer sources keyed by their canonical integer identifiers.</param>
    /// <param name="transfer">Extracts the byte span contributed by each selected source.</param>
    /// <param name="shared">Additional shared bytes included before the keyed transfers.</param>
    /// <returns>Uppercase hexadecimal SHA-256 of the framed shared and keyed transfer data.</returns>
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

    /// <summary>Frame lookup order is irrelevant; ordered parts remain significant for OAM drawing.</summary>
    public static string FromCompositions(string domain, IReadOnlyDictionary<ushort, SpriteComposition> frames) =>
        Create(domain, content =>
        {
            foreach ((ushort pointer, SpriteComposition frame) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", pointer);
                frame.AppendIdentity(content);
            }
        });

    /// <summary>Appends a labeled byte payload with length prefixes for both label and content.</summary>
    /// <param name="label">Field name identifying the payload in the canonical stream.</param>
    /// <param name="bytes">Payload bytes to include in the digest.</param>
    public void Append(string label, ReadOnlySpan<byte> bytes)
    {
        byte[] name = Encoding.UTF8.GetBytes(label);
        AppendLength(name.Length);
        hash.AppendData(name);
        AppendLength(bytes.Length);
        hash.AppendData(bytes);
    }

    /// <summary>Appends a labeled 32-bit integer encoded in little-endian order.</summary>
    /// <param name="label">Field name identifying the integer in the canonical stream.</param>
    /// <param name="value">Integer value to encode and hash.</param>
    public void Append(string label, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        Append(label, bytes);
    }

    /// <summary>Appends labeled 16-bit words after encoding each word in little-endian byte order.</summary>
    /// <param name="label">Field name identifying the word sequence in the canonical stream.</param>
    /// <param name="words">Ordered tile or transfer words to encode and hash.</param>
    public void AppendWords(string label, ReadOnlySpan<ushort> words)
    {
        var bytes = new byte[checked(words.Length * sizeof(ushort))];
        for (int index = 0; index < words.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), words[index]);
        Append(label, bytes);
    }

    /// <summary>Ordered animation rows retain their boundaries, including empty rows.</summary>
    public void AppendWordFrames(string label, IReadOnlyList<ushort[]> frames)
    {
        Append(label, frames.Count);
        foreach (ushort[] frame in frames)
            AppendWords("row", frame);
    }

    /// <summary>Null fixture domains differ from installed content without storing a derived hash.</summary>
    public void AppendIdentity(string label, string? identity) =>
        Append(label, identity is null ? ReadOnlySpan<byte>.Empty : Convert.FromHexString(identity));

    /// <summary>Preserves OAM part order, offsets, size, flips, priority, palette and tile selection.</summary>
    public void AppendEnemyParts(ReadOnlySpan<SuperMetroid.Core.Hardware.EnemySpritemapPart> parts)
    {
        Append("parts", parts.Length);
        foreach (var part in parts)
        {
            Append("x-and-size", part.X.Raw);
            Append("y", part.Y);
            Append("attributes", part.Attributes.Raw);
        }
    }

    /// <summary>Hashes calculated OBJ records with the identical ordering and framing as stored spans.</summary>
    public void AppendEnemyParts(SuperMetroid.Core.Hardware.EnemySpritemapParts parts)
    {
        Append("parts", parts.Count);
        for (int index = 0; index < parts.Count; index++)
        {
            var part = parts[index];
            Append("x-and-size", part.X.Raw);
            Append("y", part.Y);
            Append("attributes", part.Attributes.Raw);
        }
    }
    /// <summary>Writes a 32-bit little-endian byte count to delimit the next framed component.</summary>
    /// <param name="value">Nonnegative number of bytes in the component being framed.</param>
    private void AppendLength(int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
