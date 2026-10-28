namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>The control words of an escape typewriter program between its lines' character bytes.</summary>
internal enum EscapeTypewriterCommand
{
    /// <summary>$A6:C2B7: the program ends.</summary>
    End = 0,

    /// <summary>$A6:C2E8: a new line starts at the VRAM tilemap word that follows.</summary>
    LineStart = 13,
}
