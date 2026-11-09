using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The finished pictures of one frame (#1392, #1538).</summary>
/// <param name="Name">The frame's name, which names its pictures.</param>
/// <param name="Sources">Each picture's composition.</param>
/// <param name="Changes">How each encounter whose looks change the frame changes it, in encounter order; none when none does.</param>
/// <param name="Blank">
/// The names of the frame's pictures that show nothing (#1513): every character the frame reads is blank in them, so they
/// are on no sheet and the frame draws nothing while it shows them.
/// </param>
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, IReadOnlyList<FrameLookChange> Changes,
    IReadOnlyList<string> Blank)
{
    /// <summary>
    /// Where a rotting corpse lies in each of the frame's pictures that shows one, by picture name, which the game draws
    /// whole through the corpse's own rows as its rot leaves them (#1539); empty for a frame that shows no rotting corpse.
    /// </summary>
    public IReadOnlyDictionary<string, CorpsePicturePlacement> Corpses { get; init; } = new Dictionary<string, CorpsePicturePlacement>(StringComparer.Ordinal);
}
