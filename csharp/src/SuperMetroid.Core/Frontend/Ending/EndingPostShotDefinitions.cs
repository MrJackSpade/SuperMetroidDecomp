namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-shot transition's timing, motion and sound.</summary>
internal static class EndingPostShotDefinitions
{
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
    /// <summary>The zoom narrows by a quarter floor pixel per screen pixel each update.</summary>
    public static FloorZoomStep ZoomStep { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(1, 4);
    /// <summary>The zoom the completed rotation stops at.</summary>
    public static FloorZoom MinimumZoom { get; } = FloorZoom.FromFloorPixelsPerScreenPixel(3, 32);
    /// <summary>How far the shot's rotation turns backwards each update, in steps of its angle.</summary>
    public const int AngleStep = 8;
    /// <summary>How long the post-shot step delays the Samus fade behind the shot's.</summary>
    public const int SamusFadeDelay = 16;
    /// <summary>How many steps the post-credits fades run.</summary>
    public const int FadeFrames = 32;
    /// <summary>The hold before the white-flash display.</summary>
    public const int HoldFrames = 180;


    /// <summary>The updates each step of the shot picture's and Samus's fades takes: one.</summary>
    public const int FadeUpdatesPerStep = 1;

    /// <summary>The shot's update the shot picture's fade takes its first step on: its first.</summary>
    public const int ShotFadeFirstStepUpdate = 1;
}
