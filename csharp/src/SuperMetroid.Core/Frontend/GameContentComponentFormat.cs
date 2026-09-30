namespace SuperMetroid.Core.Frontend;

/// <summary>Bounded, extensible component table shared by state and recording headers.</summary>
public static class GameContentComponentFormat
{
    /// <summary>Maximum number of independently named presentation catalogs in an artifact.</summary>
    public const int MaximumComponentCount = 64;
    /// <summary>Maximum ASCII bytes in a stable, path-independent catalog name.</summary>
    public const int MaximumNameByteCount = 80;
    /// <summary>Exact SHA-256 digest size; no asset bytes are persisted by this table.</summary>
    public const int DigestByteCount = 32;

    /// <summary>Validates before any output is written, including duplicate normalized names.</summary>
    public static void Validate(IReadOnlyDictionary<string, byte[]> components)
    {
        ArgumentNullException.ThrowIfNull(components);
        if (components.Count > MaximumComponentCount)
            throw new InvalidDataException("Content identity has too many named components.");
        foreach ((string name, byte[] digest) in components)
        {
            ValidateName(name);
            if (digest is null || digest.Length != DigestByteCount)
                throw new InvalidDataException($"Content component {name} requires a 32-byte SHA-256 digest.");
        }
    }

    /// <summary>Writes a canonical, bounded table. Only catalog IDs and hashes are included.</summary>
    public static void Write(Stream destination, IReadOnlyDictionary<string, byte[]> components)
    {
        ArgumentNullException.ThrowIfNull(destination);
        Validate(components);
        using var writer = new BinaryWriter(destination, System.Text.Encoding.ASCII, leaveOpen: true);
        writer.Write(components.Count);
        foreach ((string name, byte[] digest) in components.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(name);
            writer.Write(bytes.Length);
            writer.Write(bytes);
            writer.Write(digest);
        }
    }

    /// <summary>Reads without allocating from untrusted counts or unrestricted string lengths.</summary>
    public static IReadOnlyDictionary<string, byte[]> Read(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var reader = new BinaryReader(source, System.Text.Encoding.ASCII, leaveOpen: true);
        int count = reader.ReadInt32();
        if ((uint)count > MaximumComponentCount)
            throw new InvalidDataException("Content identity has an invalid component count.");
        var components = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        for (int index = 0; index < count; index++)
        {
            int length = reader.ReadInt32();
            if (length <= 0 || length > MaximumNameByteCount)
                throw new InvalidDataException("Content identity has an invalid component-name length.");
            byte[] bytes = new byte[length];
            source.ReadExactly(bytes);
            // ASCII's replacement fallback must not turn a malformed byte into a valid name.
            if (bytes.Any(value => value > 127))
                throw new InvalidDataException("Content component names must be ASCII identifiers.");
            string name = System.Text.Encoding.ASCII.GetString(bytes);
            ValidateName(name);
            byte[] digest = new byte[DigestByteCount];
            source.ReadExactly(digest);
            if (!components.TryAdd(name, digest))
                throw new InvalidDataException($"Content identity duplicates component {name}.");
        }
        return components;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > MaximumNameByteCount ||
            name.Any(value => !(char.IsAsciiLetterOrDigit(value) || value is '-' or '_')))
            throw new InvalidDataException("Content component names must be bounded ASCII identifiers, not paths.");
    }
}
