namespace SuperMetroid.Core.Assets;

/// <summary>
/// One part of a sprite sheet frame (#1392): its pre-coloured part image, placed within the frame so that it draws at the
/// frame's own origin. A frame the screen's object limit cuts short shows its first parts this way, each read by the
/// loader the frame's origin chose, as the cartridge's loader keeps them.
/// </summary>
/// <param name="Image">The part image, its origin moved so that it lands where the part lies in the frame.</param>
/// <param name="Offset">The part's place from the frame's origin.</param>
public sealed record SpriteSheetFramePart(SpriteSheetFrame Image, Offset16<ImagePixels> Offset);
