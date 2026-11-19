using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Defines the editable gameplay starting and common-sprite RGB5 colors.</summary>
/// <param name="Version">Document schema revision.</param>
/// <param name="Initial">Complete 256-color initial CGRAM image.</param>
/// <param name="CommonSprites">Sixteen shared gameplay OBJ colors.</param>
[SuppressMessage("Design", "SME6463", Justification = "Import data: gameplay's starting and common-sprite colours, which only the import reads to pre-colour the sheets that show them; GameplayBasePaletteFiles.Extract writes it to the gameplay base palette's import data, which the installer deletes before it publishes.")]
public sealed record GameplayBasePaletteDocument(int Version, PaletteRgb5[] Initial,
    PaletteRgb5[] CommonSprites);
