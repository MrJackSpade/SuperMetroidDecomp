namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed artwork content by name (#1422): the characters, or the tilemap row, an <see cref="InstalledArtworkId"/>
/// names in the installation. Saved state carries the names, never the content.
/// </summary>
public interface IInstalledArtworkContent
{
    /// <summary>The complete installed content <paramref name="artwork"/> names.</summary>
    /// <param name="artwork">The installed artwork.</param>
    ReadOnlyMemory<byte> Resolve(InstalledArtworkId artwork);
}
