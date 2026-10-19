namespace SuperMetroid.Core.Hardware;

/// <summary>An installed VRAM asset shown in the OBJ character table.</summary>
/// <param name="Asset">The asset.</param>
public sealed record AssetObjCharacters(VramAssetId Asset) : ObjCharacterSource
{
    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) => Ensure.NotNull(resolver).ResolveAsset(Asset);

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"asset {Asset}";
}
