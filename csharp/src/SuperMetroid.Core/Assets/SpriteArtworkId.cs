namespace SuperMetroid.Core.Assets;

/// <summary>
/// Owned identity of one sprite artwork sheet. It names installed artwork by what it is, never by the
/// cartridge address or VRAM slot it was once transferred from.
/// </summary>
/// <param name="Kind">The artwork family.</param>
/// <param name="Variant">Selects one sheet within a family (an enemy species, a pose pair); zero for single-sheet families.</param>
public readonly record struct SpriteArtworkId(SpriteArtworkKind Kind, int Variant = 0);
