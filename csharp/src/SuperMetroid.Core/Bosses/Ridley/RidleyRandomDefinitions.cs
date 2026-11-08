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

    /// <summary>Norfair Ridley's roll for fireballing out of his pogo, taken at 128 or more: the sample's low byte.</summary>
    internal static readonly RandomSampleField PogoFireballRoll = new(0, 8);

    /// <summary>Ridley's roll for a tail whip, taken at 240 or more: the sample's low byte.</summary>
    internal static readonly RandomSampleField TailWhipRoll = new(0, 8);

    /// <summary>Ceres Ridley's roll for changing his tail's curliness, taken only at 255: the sample's upper byte.</summary>
    internal static readonly RandomSampleField TailCurlinessRoll = new(8, 8);
}
