using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>What one sprite sheet composes: its frames in sheet order and the sprite rows' colours.</summary>
/// <param name="Frames">The frames in sheet order.</param>
/// <param name="SpriteColours">The eight sprite rows' colours, sixteen each.</param>
/// <param name="PaletteBits">
/// The palette row the family's draws OR into each part's own, as an enemy's graphics-set row; zero for families whose
/// parts name their rows outright.
/// </param>
/// <param name="PartsKeepOwnRows">
/// Whether a part that names its own row keeps it under a draw's palette override, as frontend spritemaps place only
/// their palette-inheriting parts in the draw's palette; enemies' draws OR their bits into every part instead.
/// </param>
internal sealed record SpriteSheetSources(IReadOnlyList<SheetFrameSource> Frames, Bgr555[] SpriteColours, int PaletteBits = 0,
    bool PartsKeepOwnRows = false);
