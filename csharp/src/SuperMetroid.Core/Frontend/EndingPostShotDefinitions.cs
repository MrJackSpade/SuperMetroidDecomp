using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B post-shot transition identities and native staging layout.</summary>
internal static class EndingPostShotDefinitions
{
    /// <summary>E48A selects BG1SC=$4E, with its upper tilemap at VRAM word $4C00.</summary>
    public const ushort FinalTextTilemapWord = 0x4c00;
    /// <summary>E504 copies the text staging map into the second screen at word $5000.</summary>
    public const ushort FinalLowerTilemapWord = 0x5000;
    /// <summary>BG1SC=$4E selects a 32-column, 64-row map.</summary>
    public const int FinalTextMapHeight = 64;
    /// <summary>E48A clears the copyright's first staging-map row at word 384.</summary>
    public const int ClearedCopyrightStart = 384;
    /// <summary>E48A clears two 32-word copyright rows.</summary>
    public const int ClearedCopyrightWords = 64;
    /// <summary>E48A adds maximum five-bit white; E504 decreases it each frame.</summary>
    public const byte WhiteComponent = 31;
    /// <summary>F604 queues library-one $22 through the max-fifteen sound queue.</summary>
    public static readonly SoundEffectId ShotSound = new(SoundEffectLibrary.Library1, 0x22);
    /// <summary>F604's sound queue capacity argument.</summary>
    public const byte SoundQueueLimit = 15;
    /// <summary>Func126's post-credits Mode-7 horizontal origin.</summary>
    public const short CenterX = 104;
    /// <summary>Func126's post-credits Mode-7 vertical origin.</summary>
    public const short CenterY = 112;
    /// <summary>F604's post-shot BG horizontal offset.</summary>
    public const short OffsetX = -4;
    /// <summary>F604's post-shot BG vertical offset.</summary>
    public const short OffsetY = -8;
    /// <summary>$99:E089: compressed Super Metroid logo tiles, decompressed to $7E:6000.</summary>
    public const int LogoTiles = 0x99e089;
    /// <summary>$99:ECC4: compressed logo map, decompressed to $7E:8000.</summary>
    public const int LogoMap = 0x99ecc4;
    /// <summary>$8B:E45A: six eight-byte upload records (size, long source, padding, destination word).</summary>
    public const int UploadTable = 0x8be45a;
    /// <summary>$7F:D000: subtitle characters within the font decompressed at $7F:C000.</summary>
    public const int SubtitleSource = 0x7fd000;
    /// <summary>Subtitle characters start $1000 bytes into the font staging buffer.</summary>
    public const int SubtitleFontOffset = 0x1000;
    /// <summary>$7E:6000: logo tile staging start, replacing the former shooting-sheet buffer.</summary>
    public const int LogoTileSource = 0x7e6000;
    /// <summary>$7E:8000: logo map staging start.</summary>
    public const int LogoMapSource = 0x7e8000;
    /// <summary>Func142 has six transfer records.</summary>
    public const int UploadCount = 6;
    /// <summary>Each Func142 record occupies eight bytes including bank padding.</summary>
    public const int UploadRecordBytes = 8;
    /// <summary>Func126 initializes the reward Mode-7 scale to nine times unity.</summary>
    public const int InitialScale = 2304;
    /// <summary>Func139 reduces scale by $40 per frame.</summary>
    public const int ScaleStep = 64;
    /// <summary>Func139 clamps the completed rotation to $18.</summary>
    public const int MinimumScale = 24;
    /// <summary>Func139 rotates eight angle units backwards each frame.</summary>
    public const int AngleStep = 8;
    /// <summary>F604 delays the Samus fade by sixteen calls.</summary>
    public const int SamusFadeDelay = 16;
    /// <summary>Shared cinematic palette fades run thirty-two component steps.</summary>
    public const int FadeFrames = 32;
    /// <summary>Func141 waits 180 calls before switching to the white-flash Mode-1 display.</summary>
    public const int HoldFrames = 180;
    /// <summary>F604 fades the shooting-sheet palette at CGRAM $C0.</summary>
    public const int ShootingPaletteStart = 192;
    /// <summary>F604/Func140 fade the visible Samus palette at CGRAM $F0.</summary>
    public const int SamusPaletteStart = 240;
    /// <summary>Each reward OBJ palette contains sixteen colors.</summary>
    public const int PaletteColors = 16;
}
