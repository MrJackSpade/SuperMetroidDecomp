namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Selected visual warning program consumed by the shared escape typewriter, without cartridge bytecode or editable timing commands.</summary>
/// <param name="Id">Ceres or Zebes identity used to rebind content to saved playback state.</param>
/// <param name="Lines">Ordered text and VRAM placements; loaded catalogs expose a read-only compiled sequence, while this record constructor itself does not copy or validate a supplied list.</param>
public sealed record EscapeTypewriterProgram(
    EscapeTypewriterProgramId Id,
    IReadOnlyList<EscapeTypewriterLine> Lines);
