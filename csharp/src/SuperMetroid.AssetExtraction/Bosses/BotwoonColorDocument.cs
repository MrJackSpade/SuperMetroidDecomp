using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Editable JSON schema for Botwoon's eight health-dependent sprite-palette images.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: Botwoon's health-band colour rows, which only the import reads to pre-colour his shades; EnemyTileArtworkFiles.Extract writes it to the enemy tiles' import data, which the installer deletes before it publishes.")]
public sealed record BotwoonColorDocument
{
    /// <summary>Gets the Botwoon color schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets eight ordered rows of sixteen RGB5 colors corresponding to native <c>$B3:971B-$981A</c>.</summary>
    public required PaletteRgb5[][] Health { get; init; }
}
