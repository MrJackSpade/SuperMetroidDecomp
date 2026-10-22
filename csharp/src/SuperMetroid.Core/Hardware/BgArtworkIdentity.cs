namespace SuperMetroid.Core.Hardware;

/// <summary>Names the owned artwork a background binding shows: a catalog entry, an installed asset or a source (#1350).</summary>
/// <param name="Name">The artwork's identity.</param>
public readonly record struct BgArtworkIdentity(string Name)
{
    /// <summary>
    /// The installed artwork a queued or direct transfer from <paramref name="sourceAddress"/> shows: animated tiles' frames
    /// and other transfers named by where the cartridge keeps them.
    /// </summary>
    public static BgArtworkIdentity Transfer(int sourceAddress) => new($"artwork ${sourceAddress:X6}");
}
