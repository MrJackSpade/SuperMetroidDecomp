namespace SuperMetroid.Core.Game;

/// <summary>Shared $A6:C2A7 byte-stream commands and the Zebes text setup.</summary>
public static class EscapeTypewriterRomData
{
    /// <summary>$A6:C49C Zebes self-destruct message, installed by C23F.</summary>
    public const int ZebesText = 0xa6c49c;
    /// <summary>$A9:B2E3 glyph base passed to the shared typewriter.</summary>
    public const ushort ZebesTileBase = 0x2610;
    /// <summary>$A6:C2B7 zero word ends the text stream.</summary>
    public const ushort End = 0;
    /// <summary>$A6:C2D6 word one sets the following word as character delay.</summary>
    public const ushort Delay = 1;
    /// <summary>$A6:C2E8 word thirteen sets the following word as VRAM destination.</summary>
    public const ushort Destination = 13;
    /// <summary>$A6:C31D ASCII uppercase base subtracted when calculating a glyph tile.</summary>
    public const byte FirstLetter = 65;
    /// <summary>$A6:C310 exclamation mark is remapped to the glyph after Z.</summary>
    public const byte ExclamationGlyph = 91;
    /// <summary>$A6:C36D non-Ceres key click in sound library three.</summary>
    public const ushort ZebesClick = 0x0d;
    /// <summary>$A6:C23F escape message foreground colors are copied from these two entries.</summary>
    public const int ColorSource = 125;
    /// <summary>$A6:C242 the two message foreground colors land at these entries.</summary>
    public const int ColorDestination = 157;
}
