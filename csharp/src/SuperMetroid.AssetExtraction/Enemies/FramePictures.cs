using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The pictures of one frame (#1392, #1513).</summary>
/// <param name="Name">The frame's name, which names its pictures.</param>
/// <param name="Sources">Each picture's composition.</param>
/// <param name="Variations">How the frame follows each encounter artwork sequence that changes it; none when none does.</param>
/// <param name="RotCorpse">The kind of corpse whose rot the frame shows, which names its pictures (#1406); none when it shows no rot.</param>
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, FrameArtworkVariation[] Variations, string? RotCorpse);
