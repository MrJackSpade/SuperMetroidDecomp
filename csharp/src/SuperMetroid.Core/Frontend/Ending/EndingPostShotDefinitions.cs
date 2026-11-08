namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-shot transition's timing, motion, sound and text layout.</summary>
internal static class EndingPostShotDefinitions
{
    /// <summary>The final text background plane E48A shows is 32 cells wide and this many cells tall: two screens stacked.</summary>
    public const int FinalTextMapHeight = 64;
    /// <summary>E48A starts the white wash at all its steps, full white; E504 takes one step off each frame.</summary>
    public const int WhiteComponent = WhiteWash.Steps;
    /// <summary>The shot queues the charged plasma-wave sound through the max-fifteen sound queue.</summary>
    public static readonly SoundEffectId ShotSound = Library1Sound.ChargedPlasmaWave;
    /// <summary>F604 queues the shot through the max-fifteen entry point.</summary>
    public static SoundRequestPriority ShotSoundLimit { get; } = SoundRequestPriority.Essential;
    /// <summary>Func126's post-credits turning-floor horizontal origin.</summary>
    public const int CenterX = 104;
    /// <summary>Func126's post-credits turning-floor vertical origin.</summary>
    public const int CenterY = 112;
    /// <summary>F604's post-shot background horizontal offset.</summary>
    public const int OffsetX = -4;
    /// <summary>F604's post-shot background vertical offset.</summary>
    public const int OffsetY = -8;
    /// <summary>Func126 initializes the reward turning-floor scale to nine times unity.</summary>
    public const int InitialScale = 2304;
    /// <summary>The shrink step reduces scale by 64 per frame.</summary>
    public const int ScaleStep = 64;
    /// <summary>The shrink step clamps the completed rotation to 24.</summary>
    public const int MinimumScale = 24;
    /// <summary>Func139 rotates eight angle units backwards each frame.</summary>
    public const int AngleStep = 8;
    /// <summary>F604 delays the Samus fade by sixteen calls.</summary>
    public const int SamusFadeDelay = 16;
    /// <summary>Shared cinematic palette fades run thirty-two component steps.</summary>
    public const int FadeFrames = 32;
    /// <summary>Func141 waits 180 calls before switching to the white-flash display.</summary>
    public const int HoldFrames = 180;
}
