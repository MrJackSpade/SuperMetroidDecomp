namespace SuperMetroid.Core.Assets;

/// <summary>Defines the editable gameplay starting and common-sprite RGB5 colors.</summary>
/// <param name="Version">Document schema revision.</param>
/// <param name="Initial">Complete 256-color initial CGRAM image.</param>
/// <param name="CommonSprites">Sixteen shared gameplay OBJ colors.</param>
public sealed record GameplayBasePaletteDocument(int Version, PaletteRgb5[] Initial,
    PaletteRgb5[] CommonSprites);
