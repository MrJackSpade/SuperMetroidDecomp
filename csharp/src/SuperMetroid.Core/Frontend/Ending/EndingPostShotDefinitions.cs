namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-shot transition's timing, motion, sound and text layout.</summary>
internal static class EndingPostShotDefinitions
{
    /// <summary>The final text background plane E48A shows is 32 cells wide and this many cells tall: two screens stacked.</summary>
    public const int FinalTextMapHeight = 64;
    /// <summary>E48A clears the copyright's first staging-map row at word 384.</summary>
    public const int ClearedCopyrightStart = 384;
    /// <summary>E48A clears two 32-word copyright rows.</summary>
    public const int ClearedCopyrightWords = 64;
    /// <summary>E48A adds maximum five-bit white; E504 decreases it each frame.</summary>
    public const byte WhiteComponent = 31;
    /// <summary>F604 queues library-one $22 through the max-fifteen sound queue.</summary>
    public static readonly SoundEffectId ShotSound = Library1Sound.ChargedPlasmaWave;
    /// <summary>F604 queues the shot through the max-fifteen entry point.</summary>
    public static SoundQueueLimit ShotSoundLimit { get; } = SoundQueueLimit.Max15;
    /// <summary>Func126's post-credits Mode-7 horizontal origin.</summary>
    public const short CenterX = 104;
    /// <summary>Func126's post-credits Mode-7 vertical origin.</summary>
    public const short CenterY = 112;
    /// <summary>F604's post-shot BG horizontal offset.</summary>
    public const short OffsetX = -4;
    /// <summary>F604's post-shot BG vertical offset.</summary>
    public const short OffsetY = -8;
    /// <summary>The bytes into the ending font's characters the subtitle's characters start at.</summary>
    public const int SubtitleFontOffset = 0x1000;
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
}
