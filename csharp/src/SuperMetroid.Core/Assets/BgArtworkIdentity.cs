
namespace SuperMetroid.Core.Assets;

/// <summary>Names the owned artwork a background binding shows: a catalog entry, an installed asset or a source (#1350).</summary>
/// <param name="Name">The artwork's identity.</param>
public readonly record struct BgArtworkIdentity(string Name)
{

    /// <summary>The installed VRAM asset <paramref name="asset"/>, as the VRAM queue binds it.</summary>
    public static BgArtworkIdentity Asset(InstalledArtworkId asset) => new($"asset {asset}");
}
