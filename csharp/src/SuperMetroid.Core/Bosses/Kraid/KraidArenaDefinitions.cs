using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Where Kraid's fight plays and how long its stages last (#2216): his rise through the floor and the screen he holds
/// Samus on, where his parts sit on his body, his small form's lunge, his growth, his lints' and fingernails' paths, his
/// hurt flash, and his sinking death's sound, shake and drops.
/// </summary>
internal static class KraidArenaDefinitions
{
    /// <summary>
    /// The first pixel of the room's second screen: Samus is held left of it while Kraid rises, and a fingernail waiting
    /// for the top lint flies once the lint has passed it.
    /// </summary>
    internal const int SecondScreenX = RoomGridConversions.BlocksPerScreen * RoomGridConversions.BlockPixels;

    /// <summary>Where Kraid's small form stands: where he rises, and where his lunge's retreat returns him.</summary>
    internal const int FirstPhaseX = 176;

    /// <summary>Where Kraid's body starts, below the floor, before he rises.</summary>
    internal static readonly Position16<RoomPixels> RiseStart = new(FirstPhaseX, 592);

    /// <summary>The updates Kraid waits below the floor, with Samus held on the first screen, before he rises.</summary>
    internal const int RiseWaitUpdates = 300;

    /// <summary>The updates his head holds its first pose as he waits to rise.</summary>
    internal const int RiseHeadDelayUpdates = 64;

    /// <summary>The updates of the rise's first rocks, thrown up every sixteenth update.</summary>
    internal const int RiseSlowRocksUpdates = 120;

    /// <summary>The updates of the rise's faster rocks, thrown up every eighth update, before his body rises.</summary>
    internal const int RiseFastRocksUpdates = 96;

    /// <summary>The updates the room shakes from the start of the rise's rocks.</summary>
    internal const int RiseShakeUpdates = 496;

    /// <summary>The height Kraid's rising body rises above before his first phase starts.</summary>
    internal const int RisenY = 457;

    /// <summary>How far Kraid's body rises each update while it raises itself: half a pixel upward.</summary>
    internal static readonly SignedSixteenSixteen RisingBodyLift = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);

    /// <summary>The height the rocks thrown up during the rise start from.</summary>
    internal const int RisingRockY = 432;

    /// <summary>
    /// The lowest Samus can stand as Kraid's body pushes her out of it in his small form: the room's lower half.
    /// </summary>
    internal const int FirstPhaseEjectionLimitY = 324;

    /// <summary>The lowest Samus can stand as Kraid's body pushes her out of it once he has grown.</summary>
    internal const int GrownEjectionLimitY = 164;

    /// <summary>The height Kraid's growing body rises above before he comes in front of the room's sprites.</summary>
    internal const int GrownY = 296;

    /// <summary>
    /// The height of each lint above (negative) or below his body as Kraid thinks between attacks: the top lint above his
    /// head, the middle at his belly and the bottom at his knees.
    /// </summary>
    internal static readonly FrozenDictionary<KraidLintPart, int> LintHeights = new Dictionary<KraidLintPart, int>
    {
        [KraidLintPart.Top] = -20,
        [KraidLintPart.Middle] = 46,
        [KraidLintPart.Bottom] = 112,
    }.ToFrozenDictionary();

    /// <summary>
    /// How far left of the body's line each lint emerges: the top lint on it, the middle and bottom lints sixteen pixels
    /// further left.
    /// </summary>
    internal static readonly FrozenDictionary<KraidLintPart, int> LintXOffsets = new Dictionary<KraidLintPart, int>
    {
        [KraidLintPart.Top] = 0,
        [KraidLintPart.Middle] = -16,
        [KraidLintPart.Bottom] = -16,
    }.ToFrozenDictionary();

    /// <summary>Where Kraid's arm hangs from his body.</summary>
    internal static readonly Offset16<RoomPixels> ArmOffset = new(0, -44);

    /// <summary>Where Kraid's foot stands below his body.</summary>
    internal static readonly Offset16<RoomPixels> FootOffset = new(0, 100);

    /// <summary>The pixels Kraid's body moves sideways with each step cue of his foot.</summary>
    internal const int StepPixels = 3;

    /// <summary>The updates the foreground shakes as his body comes down on a step.</summary>
    internal const int StepShakeUpdates = 10;

    /// <summary>How far left his small form's lunge carries his body.</summary>
    internal const int LungeReachX = 92;

    /// <summary>
    /// The updates his small form's foot waits between lunges: from the end of the rise, and from each retreat's return.
    /// </summary>
    internal const int LungeDelayUpdates = 300;

    /// <summary>The updates his small form's foot rests at the lunge's reach before it walks back, the last included.</summary>
    internal const int LungeReachPauseUpdates = 1;

    /// <summary>The updates his head plays its animation, shaking the room, before he grows.</summary>
    internal const int GrowthHeadAnimationUpdates = 180;

    /// <summary>The updates the room shakes as Kraid starts to grow.</summary>
    internal const int GrowthShakeUpdates = 340;

    /// <summary>Where his grown body walks to first after growing.</summary>
    internal const int SecondPhaseStartX = 288;

    /// <summary>The updates his grown foot waits there before he walks to his starting destination.</summary>
    internal const int SecondPhaseStartDelayUpdates = 180;

    /// <summary>The updates his grown body thinks after that first walk before choosing the next.</summary>
    internal const int SecondPhaseFirstThinkUpdates = 180;

    /// <summary>
    /// The updates his head holds its pose once his mouth closes after an attack or a hurt roar, before the head
    /// animates again.
    /// </summary>
    internal const int MouthClosedHeadDelayUpdates = 90;

    /// <summary>The updates Kraid waits before glowing his eye again while reopens of his mouth are pending.</summary>
    internal const int EyeGlowDelayUpdates = 64;

    /// <summary>
    /// The updates each step of his hurt flash lasts (<see cref="KraidHurtFlashAnimations"/>), and the wait before its first,
    /// counting the hit's own update.
    /// </summary>
    internal const int HurtFlashStepUpdates = 2;

    /// <summary>
    /// The updates a fingernail waits before its first launch; once Kraid grows, each later fingernail waits this much
    /// longer than the one before.
    /// </summary>
    internal const int NailLaunchDelayUpdates = 64;

    /// <summary>How far below his arm a fingernail launches from his body's side.</summary>
    internal const int NailLaunchBelowArm = 128;

    /// <summary>Where a fingernail waiting for the top lint starts, at the arena's left.</summary>
    internal static readonly Position16<RoomPixels> AlternateNailStart = new(50, 240);

    /// <summary>The pixels a lint travels out of his body before it charges.</summary>
    internal const int LintEmergePixels = 32;

    /// <summary>The updates a lint charges, flashing, before it fires.</summary>
    internal const int LintChargeUpdates = 30;

    /// <summary>The X left of which a fired lint no longer touches Samus.</summary>
    internal const int LintIntangibleX = 56;

    /// <summary>The X left of which a fired lint is hidden and returns to his body.</summary>
    internal const int LintReturnX = 32;

    /// <summary>The updates a returned lint waits before it emerges again.</summary>
    internal const int LintReloadUpdates = 300;

    /// <summary>The updates before the sinking Kraid's first rumble.</summary>
    internal const int SinkFirstSoundUpdates = 43;

    /// <summary>The updates between the sinking Kraid's later rumbles.</summary>
    internal const int SinkSoundIntervalUpdates = 30;

    /// <summary>The updates the foreground shakes as Kraid starts to sink.</summary>
    internal const int SinkShakeUpdates = 256;

    /// <summary>The updates the room stays dark between hiding Kraid's body and fading the room back in (#1888).</summary>
    internal const int DeathDarkUpdates = 5;

    /// <summary>The height Kraid's sinking body reaches when he has sunk below the floor.</summary>
    internal const int SunkY = 608;

    /// <summary>
    /// The heights at which Kraid's sinking body crumbles a platform block (#1889, #2300), and the block each crumbles; he
    /// crumbles nothing at any other height.
    /// </summary>
    internal static readonly FrozenDictionary<int, KraidSinkStep> SinkCrumbles = new Dictionary<int, KraidSinkStep>
    {
        [304] = KraidSinkStep.CrumbleLeftPlatformLeft,
        [328] = KraidSinkStep.CrumbleRightPlatformMiddle,
        [352] = KraidSinkStep.CrumbleRightPlatformLeft,
        [384] = KraidSinkStep.CrumbleLeftPlatformRight,
        [408] = KraidSinkStep.CrumbleLeftPlatformMiddle,
        [432] = KraidSinkStep.CrumbleRightPlatformRight,
    }.ToFrozenDictionary();

    /// <summary>The height his ceiling rocks start falling from.</summary>
    internal const int CeilingRockY = 312;

    /// <summary>Where his spit rock leaves his body: sixteen pixels right of it and ninety-six up.</summary>
    internal static readonly Offset16<RoomPixels> SpitRockOffset = new(16, -96);

    /// <summary>The speed his spit rock rises at as it leaves his mouth: four pixels an update.</summary>
    internal static readonly SignedEightEight SpitRockRise = SignedEightEight.FromPixels(-4);

    /// <summary>The updates of one step of his thinking delay.</summary>
    internal const int ThinkingDelayStepUpdates = 64;

    /// <summary>The steps his thinking delay takes when its random roll is none.</summary>
    internal const int ThinkingDelayStepsOnNoRoll = 2;

    /// <summary>The drops Kraid's death scatters over his floor.</summary>
    internal const int DeathDropCount = 16;

    /// <summary>The corner of the band his death's drops scatter over.</summary>
    internal static readonly Position16<RoomPixels> DeathDropOrigin = new(128, 352);
}
