using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One frame a sprite sheet composes: its installed name, its parts and the characters they index.</summary>
/// <param name="Name">The frame's name on the sheet.</param>
/// <param name="Frame">The parts.</param>
/// <param name="Characters">The characters the parts index.</param>
internal sealed record SheetFrameSource(string Name, SpriteFrame Frame, SpriteArtwork Characters);
