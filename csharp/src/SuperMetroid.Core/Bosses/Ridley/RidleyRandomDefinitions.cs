namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The random choices of Ridley in Ceres and Norfair: its attacks, timers, fireball jitter and tail.</summary>
internal static class RidleyRandomDefinitions
{
    /// <summary>The resting Ceres baby jumping on silently rather than crying (#2410): an odd sample, one in two.</summary>
    internal static readonly RandomChance CeresBabySilentJump = RandomChance.Highest(1, new RandomSampleField(0, 1));

    /// <summary>Ceres Ridley's fireball target jitter, 0 through 7 pixels on the side its sample's sign picks.</summary>
    internal static readonly RandomSampleField FireballJitter = new(0, 3);

    /// <summary>The updates Norfair Ridley's pogo attack lasts at most beyond its fewest (<see cref="NorfairRidleyMotionDefinitions.PogoLeastUpdates"/>), 0 through 63.</summary>
    internal static readonly RandomSampleField PogoTimer = new(0, 6);

    /// <summary>Which of the four pogo launches Norfair Ridley takes.</summary>
    internal static readonly RandomSampleField PogoChoice = new(0, 2);

    /// <summary>The updates the first leg of Norfair Ridley's wall sweep lasts at most beyond its fewest (<see cref="NorfairRidleyMotionDefinitions.WallSweepFirstLegLeastUpdates"/>), 0 through 31.</summary>
    internal static readonly RandomSampleField WallSweepTimer = new(0, 5);

    /// <summary>
    /// Norfair Ridley's ordinary attack starting his pogo rather than his swoop (#2334): half the samples pogo.
    /// </summary>
    internal static readonly RandomChance OrdinaryAttackPogos = RandomChance.Lowest(4, new RandomSampleField(0, 3));

    /// <summary>
    /// Norfair Ridley lunging at a damage-boosting Samus rather than starting his pogo (#2334): three samples in four
    /// lunge.
    /// </summary>
    internal static readonly RandomChance DamageBoostingAttackLunges = RandomChance.Lowest(6, new RandomSampleField(0, 3));

    /// <summary>Ceres Ridley's new tail curliness, 0 through 15 added to eight.</summary>
    internal static readonly RandomSampleField TailCurliness = new(0, 4);

    /// <summary>
    /// Norfair Ridley fireballing during his wall sweep (#1619): the top 128 of the 256 values of the sample's lowest field,
    /// half of them, start the fireballs.
    /// </summary>
    internal static readonly RandomChance WallSweepFireballs = RandomChance.Highest(128, new RandomSampleField(0, 8));

    /// <summary>
    /// Ridley's unrequested tail whip (#1619): the top 16 of the 256 values of the sample's lowest field, 240 and over, whip.
    /// </summary>
    internal static readonly RandomChance TailWhips = RandomChance.Highest(16, new RandomSampleField(0, 8));

    /// <summary>
    /// Ceres Ridley changing his tail's curliness (#1619): one random number in 256, from the top of the range, changes
    /// it.
    /// </summary>
    internal static readonly RandomChance TailCurlinessChanges = RandomChance.Highest(1, new RandomSampleField(8, 8));

    /// <summary>
    /// Whether Norfair Ridley's next pogo bounce heads away from Samus (#1619): the lowest 1365 of the 65536 samples, about
    /// one in 48, turn him round if he heads toward her and keep his heading if he heads away; every other sample sends him
    /// toward her.
    /// </summary>
    internal static readonly RandomChance PogoBounceHeadsAwayFromSamus = RandomChance.Lowest(1365, RandomSampleField.Whole);
}
