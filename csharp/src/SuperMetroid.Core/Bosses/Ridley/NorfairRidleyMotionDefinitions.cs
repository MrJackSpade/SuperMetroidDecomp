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

    /// <summary>The countdown between the steps of his arena's reveal: a step every third update.</summary>
    internal const int ArenaRevealStepHold = 2;

    /// <summary>The seek divisor of his entry into the arena.</summary>
    internal const int ArenaEntryDivisor = 2;

    /// <summary>The seek divisor of his flight to the swoop's start.</summary>
    internal const int SwoopStartDivisor = 15;

    /// <summary>The seek divisor of his flights to the pogo's side and height.</summary>
    internal const int PogoSetupDivisor = 16;

    /// <summary>The seek divisor of his flights carrying Samus and releasing her.</summary>
    internal const int CarryDivisor = 16;

    /// <summary>The countdown of his hover, after which he chooses his next attack wherever he is.</summary>
    internal const int HoverUpdates = 128;

    /// <summary>The countdown of his flight to the swoop's start, before the swoop's dive.</summary>
    internal const int SwoopSetupUpdates = 10;

    /// <summary>The countdown of his swoop's dive, aiming down.</summary>
    internal const int SwoopDiveUpdates = 32;

    /// <summary>The countdown of his swoop's sweep, aiming sideways, at whose end his tail whips.</summary>
    internal const int SwoopSweepUpdates = 20;

    /// <summary>The countdown of his swoop's turn up.</summary>
    internal const int SwoopTurnUpdates = 16;

    /// <summary>The countdown of his swoop's climb.</summary>
    internal const int SwoopClimbUpdates = 32;

    /// <summary>The countdown of his swoop's recovery, after which he lunges at a grabbable Samus.</summary>
    internal const int SwoopRecoveryUpdates = 32;

    /// <summary>The speed his swoop's dive moves toward: four and a half pixels an update.</summary>
    internal static readonly SignedEightEight SwoopDiveSpeed = SignedEightEight.FromParts(4, 128);

    /// <summary>The speed his swoop's sweep moves toward: five pixels an update.</summary>
    internal static readonly SignedEightEight SwoopSweepSpeed = SignedEightEight.FromPixels(5);

    /// <summary>The speed his swoop's turn up and climb move toward: three pixels an update.</summary>
    internal static readonly SignedEightEight SwoopClimbSpeed = SignedEightEight.FromPixels(3);

    /// <summary>The speed his swoop's recovery moves toward: one and three quarter pixels an update.</summary>
    internal static readonly SignedEightEight SwoopRecoverySpeed = SignedEightEight.FromParts(1, 192);

    /// <summary>The countdown of each leg of his wall sweep, after its first.</summary>
    internal const int WallSweepLegUpdates = 128;

    /// <summary>The least countdown of his wall sweep's first leg, to which a random roll adds.</summary>
    internal const int WallSweepFirstLegMinimum = 32;

    /// <summary>The countdown of his settling at the pogo's height before his first bounce.</summary>
    internal const int PogoSettleUpdates = 32;

    /// <summary>The least countdown of his pogo, to which a random roll adds.</summary>
    internal const int PogoMinimumUpdates = 128;

    /// <summary>The pogo bounces after which he breathes fireballs, and the count starts again.</summary>
    internal const int PogoBouncesPerFireballing = 2;

    /// <summary>The countdown of his flight carrying Samus to his carry's anchor.</summary>
    internal const int CarryToAnchorUpdates = 32;

    /// <summary>The countdown of his carry's rise, after which he releases Samus.</summary>
    internal const int CarryRiseUpdates = 32;

    /// <summary>The countdown of his flight away after releasing Samus.</summary>
    internal const int CarryReleaseUpdates = 64;

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

    /// <summary>The countdown of his death roar.</summary>
    internal const int DeathRoarUpdates = 32;

    /// <summary>The countdown of his death's explosions, after which he breaks up.</summary>
    internal const int DeathExplosionUpdates = 160;

    /// <summary>The updates between his death's small explosions, less the one each spawns on.</summary>
    internal const int DeathExplosionInterval = 4;

    /// <summary>The countdown of the wait after his breakup.</summary>
    internal const int BreakupWaitUpdates = 32;

    /// <summary>The countdown of his death's end, after which his drops scatter and his body goes.</summary>
    internal const int DeathFinishUpdates = 256;
}
