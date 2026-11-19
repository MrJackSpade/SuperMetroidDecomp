using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// A boss body's named frames as the cartridge writes them, import data only the import composes the boss body sheet's
/// pictures from (#1638); no collision or instruction-program data.
/// </summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: Phantoon's, Draygon's, Crocomire's and Mother Brain's body frames as the cartridge's packed tile writes, which only the import reads to compose the boss body sheets' pictures; EnemyTileArtworkFiles.Extract writes it to the enemy tiles' import data, which the installer deletes before it publishes.")]
public sealed record EnemyBg2FrameDocument
{
    /// <summary>Gets the owning Phantoon or Draygon frame schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets every required named frame as an ordered sequence of horizontal BG2 writes.</summary>
    public required Dictionary<string, EnemyBg2WriteDocument[]> Frames { get; init; }
}
