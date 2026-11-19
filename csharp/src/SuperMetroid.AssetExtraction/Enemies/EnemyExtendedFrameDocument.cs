using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The extended enemy frames' compositions as the cartridge packs them, import data only the import composes the enemy
/// sheets' pictures from (#1638).
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the extended enemy frames' packed sprite parts, which only the import reads to compose the enemy sheets' pictures; EnemyTileArtworkFiles.Extract writes it to the enemy tiles' import data, which the installer deletes before it publishes.")]
public sealed record EnemyExtendedFrameDocument
{
    /// <summary>Gets the extended-frame schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets every named visual frame composition defined by this schema revision.</summary>
    public required Dictionary<string, EnemyExtendedVisualComponent[]> Frames { get; init; }
    /// <summary>Visual-only frame selection; native timers and hitboxes remain fixed.</summary>
    public Dictionary<string, string>? DisplayFrames { get; init; }
}
