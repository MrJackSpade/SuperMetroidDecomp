using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>What one sprite sheet composes: its frames in sheet order and the sprite rows' colours.</summary>
/// <param name="Frames">The frames in sheet order.</param>
/// <param name="SpriteColours">The eight sprite rows' colours, sixteen each.</param>
internal sealed record SpriteSheetSources(IReadOnlyList<SheetFrameSource> Frames, Bgr555[] SpriteColours);
