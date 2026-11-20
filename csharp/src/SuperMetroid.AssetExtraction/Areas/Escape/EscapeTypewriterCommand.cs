namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The commands of an escape typewriter program between its lines' character bytes; the cartridge's words for them are
/// <see cref="EscapeTypewriterControlWords"/>'s.
/// </summary>
internal enum EscapeTypewriterCommand
{
    /// <summary>The program ends.</summary>
    End,

    /// <summary>A new line starts at the VRAM tilemap word that follows.</summary>
    LineStart,
}
