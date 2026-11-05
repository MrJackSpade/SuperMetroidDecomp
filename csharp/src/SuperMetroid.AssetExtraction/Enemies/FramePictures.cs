using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The pictures of one frame (#1392, #1513).</summary>
/// <param name="Name">The frame's name, which names its pictures.</param>
/// <param name="Sources">Each picture's composition.</param>
/// <param name="Variations">How the frame follows each encounter artwork sequence that changes it; none when none does.</param>
/// <param name="RotCorpse">The kind of corpse whose rot the frame shows, which names its pictures (#1406); none when it shows no rot.</param>
/// <param name="Blank">
/// The names of the frame's pictures in which every character it reads is blank, as the room's load, an encounter's artwork
/// sequence or a corpse's rot leaves them (#1513): it shows nothing then, so they are on no sheet and the frame draws
/// nothing while it shows them.
/// </param>
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, FrameArtworkVariation[] Variations, string? RotCorpse,
    IReadOnlyList<string> Blank);
