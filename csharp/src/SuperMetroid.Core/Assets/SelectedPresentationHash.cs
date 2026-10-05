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
    private readonly IncrementalHash hash;

    private SelectedPresentationHash(IncrementalHash hash) => this.hash = hash;

    public static string Create(string domain, Action<SelectedPresentationHash> append)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var content = new SelectedPresentationHash(hash);
        content.Append("domain", Encoding.UTF8.GetBytes(domain));
        append(content);
        return Convert.ToHexString(hash.GetHashAndReset());
    }

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

    public static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[]> frames) =>
        FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

    public static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort> frames) =>
        FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

    /// <summary>Preserves draw-run and word order while canonicalizing frame-key insertion order.</summary>
    public static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[][]> frames) =>
        Create(domain, content =>
        {
            foreach ((ushort pointer, ushort[][] runs) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", pointer);
                content.Append("runs", runs.Length);
                foreach (ushort[] run in runs)
                    content.AppendWords("words", run);
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

    public void Append(string label, ReadOnlySpan<byte> bytes)
    {
        byte[] name = Encoding.UTF8.GetBytes(label);
        AppendLength(name.Length);
        hash.AppendData(name);
        AppendLength(bytes.Length);
        hash.AppendData(bytes);
    }

    public void Append(string label, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        Append(label, bytes);
    }

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
    private void AppendLength(int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
