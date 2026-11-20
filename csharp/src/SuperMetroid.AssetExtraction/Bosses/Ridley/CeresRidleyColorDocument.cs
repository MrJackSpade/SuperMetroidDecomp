using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Editable JSON payload of native Ridley/Baby palette rows; colors use independent integer RGB5 channels, and row indices describe paint phases rather than animation durations.</summary>
public sealed record CeresRidleyColorDocument
{
    /// <summary>Schema revision; the reader accepts only <see cref="SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork.CeresRidleyColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>32 startup colors corresponding to $A6:E16F, including both transparent entries of the door/container and Baby OBJ palettes.</summary>
    public required PaletteRgb5[] Start { get; init; }
    /// <summary>Sixteen rows of three eye colors corresponding to $A6:E2AA, ordered from full brightness toward black.</summary>
    public required PaletteRgb5[][] EyeFade { get; init; }
    /// <summary>Sixteen rows of eleven body colors corresponding to $A6:E30A, ordered from black toward full brightness and used in both BG and OBJ domains.</summary>
    public required PaletteRgb5[][] BodyFade { get; init; }
    /// <summary>Three rows of fourteen health colors corresponding to $A6:E46A, shared by Ceres shot-count and Norfair HP palette selection.</summary>
    public required PaletteRgb5[][] Health { get; init; }
    /// <summary>Sixteen three-color self-destruct text phases corresponding to $A6:C1DF.</summary>
    public PaletteRgb5[][]? Alarm { get; init; }
    /// <summary>Fifteen retreat BG colors corresponding to $A6:A9E3, excluding palette entry zero and copied to CGRAM entries 81..95.</summary>
    public required PaletteRgb5[] RetreatBg { get; init; }
    /// <summary>Eight retreat colors corresponding to $A6:AA01, copied identically into BG and OBJ palettes; edits remain independent of matching startup colors.</summary>
    public required PaletteRgb5[] RetreatShared { get; init; }
    /// <summary>The Baby/container's fifteen draw colors for each shade, corresponding to $A6:E1F1.</summary>
    public CeresBabyShadeColorsDocument? BabyShades { get; init; }
}
