namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Compiled fixed behavior definitions for ordinary Metroids.</summary>
internal static class MetroidBehaviorDefinitions
{
    /// <summary>
    /// The two-pixel moves a Metroid a Power Bomb shakes loose makes, one an update, in the order it makes them (#2343):
    /// right, down, left and up.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] EscapeMoves = [new(2, 0), new(0, 2), new(-2, 0), new(0, -2)];

    /// <summary>
    /// The cry a chasing Metroid plays for each of the eight outcomes of <see cref="CrySlot"/>, in outcome order: the
    /// drain sound for two of them, and the Mochtroid's and the Metroid's shot sounds for three each.
    /// </summary>
    internal static readonly SoundEffectId[] RandomCries =
    [
        EnemySound.MetroidDrain, EnemySound.RisingSqueal, EnemySound.ShotMetroid, EnemySound.MetroidDrain,
        EnemySound.RisingSqueal, EnemySound.ShotMetroid, EnemySound.RisingSqueal, EnemySound.ShotMetroid,
    ];

    /// <summary>The random field that picks which of the eight <see cref="RandomCries"/> a chasing Metroid plays.</summary>
    internal static readonly RandomSampleField CrySlot = new(0, 3);

    /// <summary>How close on each axis, in pixels and exclusive, a closing Metroid comes to its aim before it latches on.</summary>
    internal const int LatchDistance = 8;

    /// <summary>How far above Samus's centre a Metroid aims, in pixels.</summary>
    internal const int AimAboveSamus = 8;

    /// <summary>A closing Metroid's speed on an axis for each unit of the distance it reckons (<see cref="MetroidClosingDistance"/>).</summary>
    internal const int ClosingSpeedPerDistance = 32;

    /// <summary>How long a frozen Metroid flashes each update it strains against the ice, in updates.</summary>
    internal const int EscapeFlashDuration = 2;

    /// <summary>How long a Metroid struck by an ice beam flashes, in updates.</summary>
    internal const int IceFlashDuration = 4;

    /// <summary>How much longer than its own hurt flash a frozen Metroid flashes when a shot hurts it, in updates.</summary>
    internal const int FrozenHurtFlashExtension = 8;

    /// <summary>
    /// The least energy Samus must have for a draining Metroid or Mochtroid to sound the drain: the sound stops below
    /// it.
    /// </summary>
    internal const int DrainSoundLeastHealth = 30;

    /// <summary>A sixteenth of an energy, in the parts the drain counts an energy in.</summary>
    private const int SixteenthHealth = FractionalEnergyDrain.PartsPerEnergy / 16;

    /// <summary>Gravity Suit Samus's drain fraction, an energy taken each time it exceeds what the drain holds over: three sixteenths.</summary>
    internal const int GravityDrainFraction = 3 * SixteenthHealth;

    /// <summary>Varia Suit Samus's drain fraction: six sixteenths.</summary>
    internal const int VariaDrainFraction = 6 * SixteenthHealth;

    /// <summary>Suitless Samus's drain fraction: twelve sixteenths.</summary>
    internal const int PowerSuitDrainFraction = 12 * SixteenthHealth;

    /// <summary>How long a Metroid whose ice threshold is exhausted stays frozen, in updates.</summary>
    internal const int FrozenDuration = 400;

    /// <summary>How long a latched Metroid a Power Bomb shakes loose takes to escape Samus, in updates.</summary>
    internal const int EscapeDuration = 4;

    /// <summary>The fastest a chasing Metroid moves on each axis, in whole pixels an update.</summary>
    internal const int FastestChase = 3;

    /// <summary>How many drops a frozen Metroid leaves when it dies, each chosen at random.</summary>
    internal const int FrozenDeathDropCount = 5;
}
