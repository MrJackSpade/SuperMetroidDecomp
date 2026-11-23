namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The random choices of Ridley in Ceres and Norfair: its attacks, timers, fireball jitter and tail.</summary>
internal static partial class RidleyRandomDefinitions
{
    /// <summary>The resting Ceres baby jumping on silently rather than crying (#2410): one time in two.</summary>
    internal static readonly RandomChance CeresBabySilentJump = RandomChance.Highest(1, RandomSampleField.OneOf(2));

    /// <summary>Ceres Ridley's fireball target jitter, 0 through 7 pixels on the side its sample's sign picks.</summary>
    internal static readonly RandomSampleField FireballJitter = RandomSampleField.OneOf(8);

    /// <summary>The updates Norfair Ridley's pogo attack lasts at most beyond its fewest (<see cref="NorfairRidleyMotionDefinitions.PogoLeastUpdates"/>), 0 through 63.</summary>
    internal static readonly RandomSampleField PogoTimer = RandomSampleField.OneOf(64);

    /// <summary>The draw choosing which of his health band's four pogo launches Norfair Ridley takes, one of four in turn.</summary>
    internal static readonly RandomSampleField PogoChoice = RandomSampleField.OneOf(4);

    /// <summary>The updates the first leg of Norfair Ridley's wall sweep lasts at most beyond its fewest (<see cref="NorfairRidleyMotionDefinitions.WallSweepFirstLegLeastUpdates"/>), 0 through 31.</summary>
    internal static readonly RandomSampleField WallSweepTimer = RandomSampleField.OneOf(32);

    /// <summary>
    /// Norfair Ridley's ordinary attack starting his pogo rather than his swoop (#2334): half the samples pogo.
    /// </summary>
    internal static readonly RandomChance OrdinaryAttackPogos = RandomChance.Lowest(4, RandomSampleField.OneOf(8));

    /// <summary>
    /// Norfair Ridley lunging at a damage-boosting Samus rather than starting his pogo (#2334): three samples in four
    /// lunge.
    /// </summary>
    internal static readonly RandomChance DamageBoostingAttackLunges = RandomChance.Lowest(6, RandomSampleField.OneOf(8));

    /// <summary>Ceres Ridley's new tail curliness, 0 through 15 added to eight.</summary>
    internal static readonly RandomSampleField TailCurliness = RandomSampleField.OneOf(16);

    /// <summary>
    /// Norfair Ridley fireballing during his wall sweep (#1619): the top 128 of the 256 values of the sample's lowest field,
    /// half of them, start the fireballs.
    /// </summary>
    internal static readonly RandomChance WallSweepFireballs = RandomChance.Highest(128, RandomSampleField.OneOf(256));

    /// <summary>
    /// Ridley's unrequested tail whip (#1619): the top 16 of the 256 values of the sample's lowest field, 240 and over, whip.
    /// </summary>
    internal static readonly RandomChance TailWhips = RandomChance.Highest(16, RandomSampleField.OneOf(256));

    /// <summary>
    /// Ceres Ridley changing his tail's curliness (#1619): one random number in 256, from the top of the range, changes
    /// it.
    /// </summary>
    internal static readonly RandomChance TailCurlinessChanges = RandomChance.Highest(1, RandomSampleField.RangePart(256));

    /// <summary>
    /// Whether Norfair Ridley's next pogo bounce heads away from Samus (#1619): about one in 48 turns him round if he
    /// heads toward her and keeps his heading if he heads away; every other draw sends him toward her.
    /// </summary>
    internal static readonly RandomChance PogoBounceHeadsAwayFromSamus = RandomChance.Lowest(1365, RandomSampleField.Whole);
}
