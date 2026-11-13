namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The random choices of Ridley in Ceres and Norfair: its attacks, timers, fireball jitter and tail.</summary>
internal static class RidleyRandomDefinitions
{
    /// <summary>Whether the resting Ceres baby jumps on silently rather than crying: the sample's bit zero.</summary>
    internal static readonly RandomSampleField CeresBabySilentJump = new(0, 1);

    /// <summary>Ceres Ridley's fireball target jitter, 0 through 7 pixels on the side its sample's sign picks: the sample's bits 0-2.</summary>
    internal static readonly RandomSampleField FireballJitter = new(0, 3);

    /// <summary>Norfair Ridley's fireball attack timer, 0 through 63 updates added to 128: the sample's bits 0-5.</summary>
    internal static readonly RandomSampleField FireballAttackTimer = new(0, 6);

    /// <summary>Which of the four pogo launches Norfair Ridley takes: the sample's bits 0-1.</summary>
    internal static readonly RandomSampleField PogoChoice = new(0, 2);

    /// <summary>Norfair Ridley's pogo descent timer, 0 through 31 updates added to 32: the sample's bits 0-4.</summary>
    internal static readonly RandomSampleField PogoDescentTimer = new(0, 5);

    /// <summary>Which of the eight attacks of its situation Norfair Ridley chooses: the sample's bits 0-2.</summary>
    internal static readonly RandomSampleField AttackChoice = new(0, 3);

    /// <summary>Ceres Ridley's new tail curliness, 0 through 15 added to eight: the sample's bits 0-3.</summary>
    internal static readonly RandomSampleField TailCurliness = new(0, 4);

    /// <summary>
    /// Norfair Ridley fireballing out of his pogo (#1619): the top 128 of the 256 rolls of the sample's bits 0-7, half of
    /// them, start the fireballs.
    /// </summary>
    internal static readonly RandomChance PogoFireballs = RandomChance.Highest(128, new RandomSampleField(0, 8));

    /// <summary>
    /// Ridley's unrequested tail whip (#1619): the top 16 of the 256 rolls of the sample's bits 0-7, 240 and over, whip.
    /// </summary>
    internal static readonly RandomChance TailWhips = RandomChance.Highest(16, new RandomSampleField(0, 8));

    /// <summary>
    /// Ceres Ridley changing his tail's curliness (#1619): only the top one of the 256 rolls of the sample's bits 8-15,
    /// 255, changes it.
    /// </summary>
    internal static readonly RandomChance TailCurlinessChanges = RandomChance.Highest(1, new RandomSampleField(8, 8));

    /// <summary>
    /// Whether Norfair Ridley's next pogo bounce heads away from Samus (#1619): the lowest 1365 of the 65536 samples, about
    /// one in 48, turn him round if he heads toward her and keep his heading if he heads away; every other sample sends him
    /// toward her.
    /// </summary>
    internal static readonly RandomChance PogoBounceHeadsAwayFromSamus = RandomChance.Lowest(1365, RandomSampleField.Whole);
}
