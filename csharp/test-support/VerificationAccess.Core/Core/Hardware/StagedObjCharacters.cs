namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The bytes an owned staging surface (a corpse's rotting tiles, Mother Brain's corpse buffer) held when its upload ran.
/// No installed artwork names them, so the upload keeps them.
/// </summary>
public sealed record StagedObjCharacters : SpriteArtworkSource
{
    /// <summary>The uploaded bytes.</summary>
    private readonly byte[] _bytes;

    /// <summary>Keeps a copy of <paramref name="bytes"/>, as the upload read them.</summary>
    /// <param name="bytes">The uploaded bytes.</param>
    public StagedObjCharacters(ReadOnlySpan<byte> bytes) => _bytes = bytes.ToArray();

    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> Resolve(IInstalledArtworkContent resolver) => _bytes;

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{_bytes.Length} staged bytes";
}
