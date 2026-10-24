using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The pictures of one frame (#1392).</summary>
/// <param name="Name">The frame's name, which names its pictures.</param>
/// <param name="Sources">Each picture's composition.</param>
/// <param name="Layers">The frame's layers, front first, each with the uploaded regions it reads; none when it is drawn whole.</param>
/// <param name="RotCorpse">The kind of corpse whose rot the frame shows, which names its pictures (#1406); none when it shows no rot.</param>
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, CharacterRange[][] Layers, string? RotCorpse);
