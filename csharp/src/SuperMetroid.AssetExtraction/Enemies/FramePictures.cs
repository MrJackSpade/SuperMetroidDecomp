using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The pictures of one frame (#1392, #1513).</summary>
/// <param name="Name">The frame's name, which names its pictures.</param>
/// <param name="Sources">Each picture's composition.</param>
/// <param name="Variations">How the frame follows each encounter artwork sequence that changes it; none when none does.</param>
/// <param name="Blank">
/// The names of the frame's pictures in which every character it reads is blank, as the room's load or an encounter's
/// artwork sequence leaves them (#1513): it shows nothing then, so they are on no sheet and the frame draws nothing while
/// it shows them.
/// </param>
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, FrameArtworkVariation[] Variations,
    IReadOnlyList<string> Blank)
{
    /// <summary>
    /// Where a rotting corpse lies in each of the frame's pictures that shows one, by picture name, which the game draws
    /// through the corpse's rows as its rot leaves them (#1531); empty for a frame that shows no rotting corpse.
    /// </summary>
    public IReadOnlyDictionary<string, CorpsePictureLayout> Corpses { get; init; } = new Dictionary<string, CorpsePictureLayout>(StringComparer.Ordinal);
}
