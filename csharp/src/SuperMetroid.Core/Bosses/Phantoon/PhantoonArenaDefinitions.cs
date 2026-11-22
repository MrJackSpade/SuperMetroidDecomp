namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Where Phantoon's fight plays and how long its phases wait (#2254): his one-screen room, where his flames leave and
/// circle him, how his casual flames bounce, his intro's waits, his rounds' waits, where he rises for his rage, how his
/// swoop and gaze measure Samus, his rain's flames, and his death's band, fades, explosions, drops and power delay.
/// </summary>
internal static partial class PhantoonArenaDefinitions
{
    /// <summary>The width and height of his one-screen room, outside which his enraged flames go.</summary>
    internal const int RoomSize = RoomGridConversions.BlocksPerScreen * RoomGridConversions.BlockPixels;

    /// <summary>The middle of his room across: his flame rain starts from the side of it he is on.</summary>
    internal const int MiddleX = RoomSize / 2;

    /// <summary>The point below his body his intro, rage and spiral flames circle, and where a spiral flame starts.</summary>
    internal static readonly Offset16<RoomPixels> FlameCircleCentre = new(0, 16);

    /// <summary>Where his casual and enraged flames leave his body: his mouth.</summary>
    internal static readonly Offset16<RoomPixels> MouthOffset = new(0, 32);

    /// <summary>How far a casual or rain flame's landing pose sits below where it struck the floor.</summary>
    internal const int FlameLandingDrop = 8;

    /// <summary>The upward speed a casual flame rises from its landing with: three pixels an update.</summary>
    internal static readonly SignedEightEight CasualRiseSpeed = SignedEightEight.FromPixels(-3);

    /// <summary>The sideways speed a rising casual flame drifts at, left or right: half a pixel an update.</summary>
    internal static readonly SignedEightEight CasualDriftSpeed = SignedEightEight.FromPixelFraction(1, 2);

    /// <summary>The floor strikes after which a bouncing casual flame comes to rest.</summary>
    internal const int CasualBounces = 3;

    /// <summary>The upward speed a casual flame's first bounce restarts it at: two pixels an update.</summary>
    internal static readonly SignedEightEight CasualFirstBounceSpeed = SignedEightEight.FromPixels(-2);

    /// <summary>The upward speed a casual flame's later bounce restarts it at: a pixel an update.</summary>
    internal static readonly SignedEightEight CasualLaterBounceSpeed = SignedEightEight.FromPixels(-1);

    /// <summary>The updates between the intro's flames as they appear round his body.</summary>
    internal const int StartingFlameIntervalUpdates = 30;

    /// <summary>The updates after the intro's last flame before the flames start circling.</summary>
    internal const int StartingFlamesActivateUpdates = 30;

    /// <summary>The updates the intro's flames circle before they are gone.</summary>
    internal const int StartingFlamesOrbitUpdates = 240;

    /// <summary>The updates his wavy fade-in runs before its wave starts shrinking.</summary>
    internal const int WavyFadeInUpdates = 120;

    /// <summary>The updates after his wavy fade-in before his first round's pattern is picked.</summary>
    internal const int FirstRoundPickUpdates = 30;

    /// <summary>The updates of each round's pattern before his figure-eight starts, as his pattern is picked.</summary>
    internal const int PatternPickUpdates = 60;

    /// <summary>The updates he stays solid before a swoop his vulnerable window's hits requested.</summary>
    internal const int BeforeSwoopUpdates = 60;

    /// <summary>The most updates a vulnerable window may have left once a hit has requested his swoop.</summary>
    internal const int SwoopRequestTimerCap = 16;

    /// <summary>The updates his swoop lasts once his round's damage is reached during it: it ends on the next update.</summary>
    internal const int RoundEndingSwoopUpdates = 1;

    /// <summary>The updates his swoop lasts.</summary>
    internal const int SwoopUpdates = 360;

    /// <summary>The speed, on each axis, his swoop starts with: four pixels an update right and down.</summary>
    internal static readonly SignedEightEight SwoopStartSpeed = SignedEightEight.FromPixels(4);

    /// <summary>The pixels his swoop's target sweeps each update, out to its turn and back.</summary>
    internal const int SwoopTargetStep = 2;

    /// <summary>The height his fatal swoop aims for.</summary>
    internal const int DeathSwoopY = 112;

    /// <summary>How far above Samus his swoop aims.</summary>
    internal const int SwoopAboveSamus = 48;

    /// <summary>The updates he stays hidden after a fade-out before he picks where he reappears.</summary>
    internal const int HiddenAfterFadeOutUpdates = 120;

    /// <summary>The updates his eye tracks Samus, open, while his flame rain falls.</summary>
    internal const int FlameRainTrackingUpdates = 90;

    /// <summary>The updates he stays hidden before he rises at the top of his room for his rage.</summary>
    internal const int BeforeRageUpdates = 120;

    /// <summary>Where he appears for his rage: at the top centre of his room.</summary>
    internal static readonly Position16<RoomPixels> RageStart = new(128, 32);

    /// <summary>The updates before his rage's first wave.</summary>
    internal const int FirstRageWaveUpdates = 4;

    /// <summary>
    /// How near level with or above or below him Samus must be, in pixels, for his eye to look straight at her rather than
    /// diagonally.
    /// </summary>
    internal const int EyeGazeStraightReach = 32;

    /// <summary>The flames of one flame rain.</summary>
    internal const int RainFlameCount = 8;

    /// <summary>The updates the rain's first flame waits before it falls, and each later flame waits longer than the last.</summary>
    internal const int RainFallDelayStep = 8;

    /// <summary>The height the rain's flames start falling from.</summary>
    internal const int RainStartY = 40;

    /// <summary>The leftmost X of the band his fatal swoop reaches before he starts to die.</summary>
    internal const int DeathBandLeftX = 96;

    /// <summary>The X right of the band his fatal swoop reaches before he starts to die.</summary>
    internal const int DeathBandRightX = 160;

    /// <summary>The fades of his dying flicker: five fade-outs and five fade-ins.</summary>
    internal const int DyingFadeCycles = 10;

    /// <summary>The updates before his first dying explosion.</summary>
    internal const int FirstDyingExplosionUpdates = 15;

    /// <summary>
    /// The updates of the wait, once he is gone, before the Wrecked Ship's power starts coming on: the power may start on
    /// the wait's last.
    /// </summary>
    internal const int PowerReturnDelayUpdates = 61;

    /// <summary>The drops his death scatters over the arena.</summary>
    internal const int DeathDropCount = 16;

    /// <summary>The corner of the band his death's drops scatter over.</summary>
    internal static readonly Position16<RoomPixels> DeathDropOrigin = new(64, 96);
}
