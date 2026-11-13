namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-shot transition's timing, motion and sound.</summary>
internal static class EndingPostShotDefinitions
{
    /// <summary>The white wash starts at all its steps, full white, and takes one step off each frame.</summary>
    public const int WhiteComponent = WhiteWash.Steps;
    /// <summary>The shot queues the charged plasma-wave sound through the max-fifteen sound queue.</summary>
    public static readonly SoundEffectId ShotSound = SamusSound.ChargedPlasmaWave;
    /// <summary>The post-shot step queues the shot through the max-fifteen entry point.</summary>
    public static SoundRequestPriority ShotSoundLimit { get; } = SoundRequestPriority.Essential;
    /// <summary>Func126's post-credits turning-floor horizontal origin.</summary>
    public const int CenterX = 104;
    /// <summary>Func126's post-credits turning-floor vertical origin.</summary>
    public const int CenterY = 112;
    /// <summary>The post-shot background horizontal offset.</summary>
    public const int OffsetX = -4;
    /// <summary>The post-shot background vertical offset.</summary>
    public const int OffsetY = -8;
    /// <summary>The reward turning floor's starting zoom: nine floor pixels per screen pixel.</summary>
    public static FloorZoom InitialZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(9);
    /// <summary>The zoom narrows by a quarter floor pixel per screen pixel each frame, in 256ths.</summary>
    public const int ZoomStep = 64;
    /// <summary>The zoom the completed rotation stops at.</summary>
    public static FloorZoom MinimumZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(24);
    /// <summary>Func139 rotates eight angle units backwards each frame.</summary>
    public const int AngleStep = 8;
    /// <summary>The post-shot step delays the Samus fade by sixteen calls.</summary>
    public const int SamusFadeDelay = 16;
    /// <summary>Shared cinematic palette fades run thirty-two component steps.</summary>
    public const int FadeFrames = 32;
    /// <summary>Func141 waits 180 calls before switching to the white-flash display.</summary>
    public const int HoldFrames = 180;
}
