namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Selected visual warning program consumed by the shared escape typewriter, without cartridge bytecode or editable timing commands.</summary>
/// <param name="Id">Ceres or Zebes identity used to rebind content to saved playback state.</param>
/// <param name="SourceAddress">Full native SNES source identity, $A6:C450 or $A6:C49C; retained for cursor/debugger compatibility, not read during installed-program playback.</param>
/// <param name="Lines">Ordered text and VRAM placements; loaded catalogs expose a read-only compiled sequence, while this record constructor itself does not copy or validate a supplied list.</param>
public sealed record EscapeTypewriterProgram(
    EscapeTypewriterProgramId Id,
    int SourceAddress,
    IReadOnlyList<EscapeTypewriterLine> Lines);
