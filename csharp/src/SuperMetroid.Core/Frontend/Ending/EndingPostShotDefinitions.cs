namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-shot transition's timing, motion and sound.</summary>
internal static class EndingPostShotDefinitions
{
    /// <summary>The steps of the white wash it starts with, full white; it takes one step off each frame.</summary>
    public const int WhiteWashStart = WhiteWash.Steps;
    /// <summary>The charged plasma-wave sound the shot queues.</summary>
    public static readonly SoundEffectId ShotSound = SamusSound.ChargedPlasmaWave;
    /// <summary>The priority the post-shot step queues the shot's sound at: essential, so it joins any sound queue that is not full.</summary>
    public static SoundRequestPriority ShotSoundPriority { get; } = SoundRequestPriority.Essential;
    /// <summary>The post-credits turning floor's horizontal origin.</summary>
    public const int CenterX = 104;
    /// <summary>The post-credits turning floor's vertical origin.</summary>
    public const int CenterY = 112;
    /// <summary>The post-shot background horizontal offset.</summary>
    public const int OffsetX = -4;
    /// <summary>The post-shot background vertical offset.</summary>
    public const int OffsetY = -8;
    /// <summary>The reward turning floor's starting zoom: nine floor pixels per screen pixel.</summary>
    public static FloorZoom InitialZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(9);
    /// <summary>The zoom narrows by a quarter floor pixel per screen pixel each frame, in 256ths.</summary>
    public static FloorZoomStep ZoomStep { get; } = FloorZoomStep.FromTwoHundredFiftySixths(64);
    /// <summary>The zoom the completed rotation stops at.</summary>
    public static FloorZoom MinimumZoom { get; } = FloorZoom.FromTwoHundredFiftySixths(24);
    /// <summary>The shot's rotation turns eight angle units backwards each frame.</summary>
    public const int AngleStep = 8;
    /// <summary>The post-shot step delays the Samus fade by sixteen calls.</summary>
    public const int SamusFadeDelay = 16;
    /// <summary>The post-credits fades run thirty-two steps.</summary>
    public const int FadeFrames = 32;
    /// <summary>The hold waits 180 calls before switching to the white-flash display.</summary>
    public const int HoldFrames = 180;
}
