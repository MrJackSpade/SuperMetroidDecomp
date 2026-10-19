namespace SuperMetroid.Core.Assets;

/// <summary>An owned background character set's identity; never a VRAM address.</summary>
/// <param name="Kind">The kind of character set.</param>
/// <param name="Variant">Distinguishes several sets of one kind; zero for single sets.</param>
public readonly record struct BgArtworkId(BgArtworkKind Kind, int Variant = 0);
