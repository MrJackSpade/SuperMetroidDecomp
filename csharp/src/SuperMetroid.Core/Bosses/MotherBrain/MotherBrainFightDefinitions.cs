using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where Mother Brain's fight plays and what it starts from (#2216): her health at each stage, where her body stands and
/// rises, the limits of her walks, the wall sealed behind Samus, her body's push and the touch that hurts, where her mouth,
/// laser, bomb and breath leave her head, her drool, and the stages of her drain by the Baby.
/// </summary>
internal static class MotherBrainFightDefinitions
{
    /// <summary>The health of her brain in the glass, her first phase.</summary>
    internal const int FirstPhaseHealth = 3000;

    /// <summary>The health her brain takes as her second phase sets up, before she rises.</summary>
    internal const int SecondPhaseHealth = 18000;

    /// <summary>
    /// The health her brain takes once the Baby has drained her to grey, for her third phase: twice her second phase's.
    /// </summary>
    internal const int ThirdPhaseHealth = 2 * SecondPhaseHealth;

    /// <summary>The health at or above which her second phase chooses its moves at full strength.</summary>
    internal const int FullStrengthHealth = 4500;

    /// <summary>Where her body stands: where the fake death's main tube lands it and where it rises from.</summary>
    internal static readonly Position16<RoomPixels> StandingBodyPosition = new(59, 279);

    /// <summary>The pixels her body rises with each step of her rise through the floor.</summary>
    internal const int RiseStepPixels = 2;

    /// <summary>The height her rising body stops at, standing on the floor.</summary>
    internal const int RisenY = 188;

    /// <summary>The X left of which her body's walk backward stops: the back of her arena.</summary>
    internal const int BackLimitX = 48;

    /// <summary>The X at or right of which her body's walk forward stops: the front of her arena.</summary>
    internal const int FrontLimitX = 128;

    /// <summary>
    /// Where her walks to the back of the room aim, past <see cref="BackLimitX"/>: before her hand beam, as the drained
    /// stagger reels back, and as she retreats once it ends.
    /// </summary>
    internal const int BackOfRoomX = 40;

    /// <summary>Where the drained stagger's forward walks aim.</summary>
    internal const int StaggerFrontX = 72;

    /// <summary>Where a bomb walk's short retreat aims.</summary>
    internal const int BombWalkShortRetreatX = 96;

    /// <summary>Where a bomb walk's long retreat aims.</summary>
    internal const int BombWalkLongRetreatX = 64;

    /// <summary>The updates she waits after firing a bomb.</summary>
    internal const int AfterBombUpdates = 44;

    /// <summary>How far below her head the attack choice measures Samus's height from.</summary>
    internal const int AttackMeasureBelowHead = 4;

    /// <summary>The height difference below which her second phase takes its near attacks.</summary>
    internal const int NearAttackHeight = 32;

    /// <summary>The X left of which Samus, once the glass is gone and the brain dead, ends the first phase.</summary>
    internal const int FirstPhaseExitSamusX = 236;

    /// <summary>Where the dust puffs show as the first phase's end seals the wall behind Samus, in the order they burst.</summary>
    internal static IReadOnlyList<Position16<RoomPixels>> SealedWallDust { get; } = [new(248, 72), new(248, 152)];

    /// <summary>The wall blocks the first phase's end fills behind Samus, in the order they fill.</summary>
    internal static IReadOnlyList<RoomObjectSpawnRequest<MotherBrainRoomMutation>> SealedWall { get; } =
    [
        new(new(15, 4), MotherBrainRoomMutation.FillWall),
        new(new(15, 9), MotherBrainRoomMutation.FillWall),
    ];

    /// <summary>
    /// How far her body pushes Samus away on touching her: right by her overlap, at least this far, and down by this much.
    /// </summary>
    internal static readonly Offset16<RoomPixels> BodyPush = new(4, 4);

    /// <summary>The height above which her body's push sends Samus downward.</summary>
    internal const int BodyPushDownAboveY = 192;

    /// <summary>How far right of her body Samus must be for her body's touch to hurt.</summary>
    internal const int HurtingTouchRightOfBody = 24;

    /// <summary>Where her hand is on her body, from which the hand beam charges.</summary>
    internal static readonly Offset16<RoomPixels> HandOffset = new(64, -48);

    /// <summary>Where her mouth is on her head: her onion rings wait there and aim from there.</summary>
    internal static readonly Offset16<RoomPixels> MouthOffset = new(10, 16);

    /// <summary>Where her beams leave her head: her mouth laser fires from it, travelling right, and her rainbow beam aims from it.</summary>
    internal static readonly Offset16<RoomPixels> BeamOffset = new(16, 4);

    /// <summary>Where her bomb leaves her head.</summary>
    internal static readonly Offset16<RoomPixels> BombOffset = new(12, 16);

    /// <summary>Where her purple breath leaves her head.</summary>
    internal static readonly Offset16<RoomPixels> BreathOffset = new(6, 16);

    /// <summary>The height her brain rests at on the floor: where the main tube lands it and where her severed brain falls to.</summary>
    internal const int BrainFloorY = 196;

    /// <summary>The walk credit she keeps after a walk forward or the third phase's walk limit: half a step.</summary>
    internal const int AfterStepCredit = MotherBrainWalkCredit.StepCredit / 2;

    /// <summary>The updates her brain shakes once a head animation or her rainbow beam starts its shake.</summary>
    internal const int BrainShakeUpdates = 50;

    /// <summary>The Baby attack count at which her attack cry is left out: the eleventh.</summary>
    internal const int SilentBabyAttackCount = 11;

    /// <summary>The gravity her drool gains each update as it falls.</summary>
    internal static readonly SignedEightEight DroolGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12);

    /// <summary>The height her falling drool splashes at, on the floor.</summary>
    internal const int DroolFloorY = 215;

    /// <summary>How far above the floor her drool's splash shows.</summary>
    internal const int DroolSplashRise = 4;

    /// <summary>The updates she reels, taken aback, before regaining her balance as the Baby drains her.</summary>
    internal const int RegainBalanceUpdates = 48;

    /// <summary>The stagger stage at which her rainbow beam runs out as the Baby drains her.</summary>
    internal const int BeamRunsOutStage = 6;

    /// <summary>The updates she waits, crouched, before turning grey.</summary>
    internal const int BeforeGreyUpdates = 64;

    /// <summary>The updates each step of her turn to grey lasts.</summary>
    internal const int GreyStepUpdates = 16;

    /// <summary>The steps of her turn to grey; the step after them finishes it.</summary>
    internal const int GreySteps = 8;
}
