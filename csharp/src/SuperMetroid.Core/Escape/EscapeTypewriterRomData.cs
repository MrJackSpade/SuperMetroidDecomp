namespace SuperMetroid.Core.Escape;

/// <summary>Shared $A6:C2A7 byte-stream commands and the Zebes text setup.</summary>
public static class EscapeTypewriterRomData
{
    /// <summary>$A9:B2E3 glyph base passed to the shared typewriter.</summary>
    public const ushort ZebesTileBase = 0x2610;
    /// <summary>$A6:C31D ASCII uppercase base subtracted when calculating a glyph tile.</summary>
    public const byte FirstLetter = 65;
    /// <summary>$A6:C310 exclamation mark is remapped to the glyph after Z.</summary>
    public const byte ExclamationGlyph = 91;
    /// <summary>$A6:C36D non-Ceres key click in sound library three.</summary>
    public static readonly SoundEffectId ZebesClick = Library3Sound.IntroTypewriter;
    /// <summary>$A6:C371 queues library-two $45 for every other Ceres typewriter glyph.</summary>
    public static readonly SoundEffectId CeresClick =
        Library2Sound.CeresTypewriter;
}
