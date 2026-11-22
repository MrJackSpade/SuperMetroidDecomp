using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend;

/// <summary>A fixture's sprite frames document, as a cinematic's installed sprite frames are written: its version, then each frame's parts by name.</summary>
/// <param name="Version">The document's format version.</param>
/// <param name="Frames">Each frame's parts, by the frame's name.</param>
internal sealed record SpriteFramesFixtureDocument(int Version, Dictionary<string, SpriteVisualPart[]> Frames);
