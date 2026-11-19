using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Editable four-row Magdollite glow schema, containing only animated OBJ colors rather than the complete sixteen-color native palette rows.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the Magdollite glow's colour rows, which only the import reads to pre-colour its glow shades; EnemyTileArtworkFiles.Extract writes it to the enemy tiles' import data, which the installer deletes before it publishes.")]
public sealed record MagdollitePaletteCycleDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="MagdollitePaletteCycleFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Four ordered rows of four nonnull RGB5 colors, each channel 0..31, corresponding to colors 9..12 of the native rows beginning at $A8:AC1C; stock rows rotate the $AC2E-$AC34 glow colors left.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}
