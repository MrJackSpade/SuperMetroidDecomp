namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Lower Norfair Ridley flies and fights (#2171, #2232): his pogo bounces, how he lifts a grabbed Samus, how hard
/// each of his flights seeks its target, how long each attack and his death last and how fast his swoop flies.
/// </summary>
internal static class NorfairRidleyMotionDefinitions
{
    /// <summary>The sideways speed a pogo bounce starts from when Ridley has none: three quarters of a pixel an update.</summary>
    internal static readonly SignedEightEight PogoRestartSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(192);

    /// <summary>The fastest Ridley falls on a pogo bounce: six pixels an update.</summary>
    internal static readonly SignedEightEight PogoMaximumFallSpeed = SignedEightEight.FromPixels(6);

    /// <summary>The least upward speed Ridley lifts a grabbed Samus with: two pixels an update.</summary>
    internal static readonly SignedEightEight GrabLiftMinimumSpeed = SignedEightEight.FromPixels(2);

    /// <summary>The updates between the steps of his arena's reveal: a step every third update.</summary>
    internal const int ArenaRevealStepUpdates = 3;

    /// <summary>The seek divisor of his entry into the arena.</summary>
    internal const int ArenaEntryDivisor = 2;

    /// <summary>The seek divisor of his flight to the swoop's start.</summary>
    internal const int SwoopStartDivisor = 15;

    /// <summary>The seek divisor of his flights to the pogo's side and height.</summary>
    internal const int PogoSetupDivisor = 16;

    /// <summary>The seek divisor of his flights carrying Samus and releasing her.</summary>
    internal const int CarryDivisor = 16;

    /// <summary>The updates of his hover, on the last of which he chooses his next attack wherever he is.</summary>
    internal const int HoverUpdates = 129;

    /// <summary>The updates his swoop's dive, aiming down, lasts; on the last he begins his sweep.</summary>
    internal const int SwoopDiveUpdates = 33;

    /// <summary>The updates his swoop's sweep, aiming sideways, lasts; on the last his tail whips and he turns up.</summary>
    internal const int SwoopSweepUpdates = 21;

    /// <summary>The updates his swoop's turn up lasts; on the last he begins his climb.</summary>
    internal const int SwoopTurnUpdates = 17;

    /// <summary>The updates his swoop's climb lasts; on the last he turns toward the half of the arena he is not in.</summary>
    internal const int SwoopClimbUpdates = 33;

    /// <summary>The updates his swoop's recovery lasts; on the last he lunges at a grabbable Samus or chooses his next attack.</summary>
    internal const int SwoopRecoveryUpdates = 33;

    /// <summary>How far his swoop's heading turns each update of its dive: an eighth of a step.</summary>
    internal static readonly RotationAngle SwoopDiveTurn = RotationAngle.FromSteps(0, 32);

    /// <summary>The heading his swoop's dive turns toward, facing right: two steps round from straight down.</summary>
    internal static readonly RotationAngle SwoopDiveHeading = RotationAngle.FromSteps(2);

    /// <summary>How far his swoop's heading turns each update of its sweep: a step and a quarter.</summary>
    internal static readonly RotationAngle SwoopSweepTurn = RotationAngle.FromSteps(1, 64);

    /// <summary>How far his swoop's heading turns each update of its turn up: two steps.</summary>
    internal static readonly RotationAngle SwoopTurnUpTurn = RotationAngle.FromSteps(2);

    /// <summary>How far his swoop's heading turns each update of its climb: four steps.</summary>
    internal static readonly RotationAngle SwoopClimbTurn = RotationAngle.FromSteps(4);

    /// <summary>The updates the pose he shows holds before his turn toward the arena's other half shows its first.</summary>
    internal const int TurnStartUpdates = 2;

    /// <summary>The speed his swoop's dive moves toward: four and a half pixels an update.</summary>
    internal static readonly SignedEightEight SwoopDiveSpeed = SignedEightEight.FromParts(4, 128);

    /// <summary>The speed his swoop's sweep moves toward: five pixels an update.</summary>
    internal static readonly SignedEightEight SwoopSweepSpeed = SignedEightEight.FromPixels(5);

    /// <summary>The speed his swoop's turn up and climb move toward: three pixels an update.</summary>
    internal static readonly SignedEightEight SwoopClimbSpeed = SignedEightEight.FromPixels(3);

    /// <summary>The speed his swoop's recovery moves toward: one and three quarter pixels an update.</summary>
    internal static readonly SignedEightEight SwoopRecoverySpeed = SignedEightEight.FromParts(1, 192);

    /// <summary>The updates of each leg of his wall sweep, after its first.</summary>
    internal const int WallSweepLegUpdates = 129;

    /// <summary>The fewest updates of his wall sweep's first leg, to which a random roll adds.</summary>
    internal const int WallSweepFirstLegLeastUpdates = 33;

    /// <summary>The updates of his settling at the pogo's height before his first bounce.</summary>
    internal const int PogoSettleUpdates = 33;

    /// <summary>The fewest updates of his pogo, to which a random roll adds.</summary>
    internal const int PogoLeastUpdates = 129;

    /// <summary>The pogo bounces after which he breathes fireballs, and the count starts again.</summary>
    internal const int PogoBouncesPerFireballing = 2;

    /// <summary>The updates of his flight carrying Samus to his carry's anchor.</summary>
    internal const int CarryToAnchorUpdates = 33;

    /// <summary>The updates of his carry's rise, on the last of which he releases Samus.</summary>
    internal const int CarryRiseUpdates = 33;

    /// <summary>The updates of his flight away after releasing Samus.</summary>
    internal const int CarryReleaseUpdates = 65;

    /// <summary>The pixels a carried Samus's offset from his claw closes, on each axis, each update.</summary>
    internal const int GrabOffsetDecay = 4;

    /// <summary>
    /// The hurt updates a held Samus lags behind his claw before his hurt reaction places her again; each ordinary update
    /// takes <see cref="HurtCarryLagRecovery"/> off the count.
    /// </summary>
    internal const int HurtCarryLag = 8;

    /// <summary>How much of the hurt carry lag each ordinary update takes off.</summary>
    internal const int HurtCarryLagRecovery = 4;

    /// <summary>
    /// The health below which his attacks take no account of where Samus stands or whether he can grab her: below it,
    /// half his choices pogo and half swoop.
    /// </summary>
    internal const int WoundedAttackHealth = 14400;

    /// <summary>The updates of his death roar.</summary>
    internal const int DeathRoarUpdates = 33;

    /// <summary>The updates of his death's explosions, on the last of which he breaks up.</summary>
    internal const int DeathExplosionUpdates = 161;

    /// <summary>The updates from one of his death's small explosions to the next.</summary>
    internal const int DeathExplosionIntervalUpdates = 5;

    /// <summary>The updates of the wait after his breakup.</summary>
    internal const int BreakupWaitUpdates = 33;

    /// <summary>The updates of his death's end, on the last of which his drops scatter and his body goes.</summary>
    internal const int DeathFinishUpdates = 257;
}
