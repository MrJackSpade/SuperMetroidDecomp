namespace SuperMetroid.Core.Hardware;

/// <summary>Resolves the installed artwork identities that OBJ character uploads name.</summary>
public interface IObjCharacterSourceResolver
{
    /// <summary>The complete bytes of an installed VRAM asset.</summary>
    /// <param name="asset">The asset.</param>
    ReadOnlyMemory<byte> ResolveAsset(VramAssetId asset);
}
