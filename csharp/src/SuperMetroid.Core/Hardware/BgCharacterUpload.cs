namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Artwork bound over the background characters (#1392): what it is, the VRAM word it shows from and its bytes. A room's
/// load binds its tileset this way, and extraction pictures each block over each upload that can show under it.
/// </summary>
/// <param name="Identity">The artwork bound.</param>
/// <param name="FirstWord">The first VRAM word it shows at.</param>
/// <param name="Bytes">Its bytes, low byte first, two per word.</param>
public sealed record BgCharacterUpload(BgArtworkIdentity Identity, int FirstWord, ReadOnlyMemory<byte> Bytes)
{
    /// <summary>The VRAM words the upload covers.</summary>
    public int WordCount => Bytes.Length / sizeof(ushort);

    /// <summary>Binds the upload.</summary>
    public void BindTo(BgArtworkBindings bindings) => Ensure.NotNull(bindings).Bind(Identity, FirstWord, Bytes.Span);
}
