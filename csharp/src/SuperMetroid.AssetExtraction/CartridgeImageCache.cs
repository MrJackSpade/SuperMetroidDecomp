using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Process-wide immutable cartridge images keyed by SHA-256. Every import address space with the
/// same ROM content shares one image, and a restored debugger graph re-attaches its image by digest
/// instead of carrying a private multi-megabyte copy. Images no address space references are collected.
/// </summary>
internal static class CartridgeImageCache
{
    private static readonly ConcurrentDictionary<string, WeakReference<byte[]>> images = new(StringComparer.Ordinal);

    /// <summary>The shared image equal to <paramref name="content"/>, creating it on first use.</summary>
    internal static byte[] Share(ReadOnlySpan<byte> content, out byte[] sha256)
    {
        sha256 = SHA256.HashData(content);
        string key = Convert.ToHexString(sha256);
        while (true)
        {
            if (images.TryGetValue(key, out WeakReference<byte[]>? entry))
            {
                if (entry.TryGetTarget(out byte[]? live))
                    return live;
                // A collected image is replaced; a concurrent replacement is retried, never duplicated.
                byte[] replacement = content.ToArray();
                if (images.TryUpdate(key, new WeakReference<byte[]>(replacement), entry))
                    return replacement;
                continue;
            }
            byte[] image = content.ToArray();
            if (images.TryAdd(key, new WeakReference<byte[]>(image)))
                return image;
        }
    }

    /// <summary>The live shared image with this digest; restoring a graph never invents ROM content.</summary>
    internal static byte[] Resolve(byte[] sha256)
    {
        string key = Convert.ToHexString(sha256);
        if (images.TryGetValue(key, out WeakReference<byte[]>? entry) && entry.TryGetTarget(out byte[]? image))
            return image;
        throw new InvalidDataException(
            $"A restored cartridge import space names ROM SHA-256 {key}, which no address space in this process has loaded.");
    }
}

/// <summary>Host content a restored debugger graph re-attaches after its saved fields are set.</summary>
public interface IRestoredSharedContent
{
    /// <summary>Reconnects deserialized state to process-owned immutable content after its serialized fields have been restored.</summary>
    void ReattachSharedContent();
}
