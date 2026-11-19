namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The fixed behavior definitions of ordinary Metroids.</summary>
internal static class MetroidBehaviorDefinitions
{
    /// <summary>
    /// The two-pixel moves a Metroid a Power Bomb shakes loose makes, one an update, in the order it makes them (#2343):
    /// right, down, left and up.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] EscapeMoves = [new(2, 0), new(0, 2), new(-2, 0), new(0, -2)];

    /// <summary>
    /// The cry a chasing Metroid plays, chosen from eight rolls of its random number (#2510): the drain sound, the rising
    /// squeal and the Metroid's shot sound, the three again, then the squeal and the shot sound, so the drain is played for
    /// two rolls and the other two for three each.
    /// </summary>
    internal static readonly WeightedRandomChoice<SoundEffectId> Cry = new(new RandomSampleField(0, 3),
        new(1, EnemySound.MetroidDrain), new(1, EnemySound.RisingSqueal), new(1, EnemySound.ShotMetroid),
        new(1, EnemySound.MetroidDrain), new(1, EnemySound.RisingSqueal), new(1, EnemySound.ShotMetroid),
        new(1, EnemySound.RisingSqueal), new(1, EnemySound.ShotMetroid));

    /// <summary>How close on each axis, in pixels and exclusive, a closing Metroid comes to its aim before it latches on.</summary>
    internal const int LatchDistance = 8;

    /// <summary>How far above Samus's centre a Metroid aims, in pixels.</summary>
    internal const int AimAboveSamus = 8;

    /// <summary>A closing Metroid's speed on an axis for each unit of the distance it reckons (<see cref="MetroidClosingDistance"/>).</summary>
    internal const int ClosingSpeedPerDistance = 32;

    /// <summary>
    /// How fast a Metroid a body attack throws off moves away from Samus on each axis for each pixel between them: a
    /// quarter of a pixel an update, a quarter of their distance an update (#2339).
    /// </summary>
    internal static readonly SignedSixteenSixteen BodyAttackThrowPerDistance = SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>
    /// How fast a shot Metroid recoils from where the shots it took came from on each axis for each pixel between them:
    /// an eighth of a pixel an update, an eighth of their distance an update (#2339).
    /// </summary>
    internal static readonly SignedSixteenSixteen ShotRecoilPerDistance = SignedSixteenSixteen.FromPixelFraction(1, 8);

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

    /// <summary>Gravity Suit Samus's drain fraction, an energy taken each time it exceeds what the drain holds over: three sixteenths.</summary>
    internal static readonly EnergyAmount GravityDrainFraction = EnergyAmount.FromEnergyFraction(3, 16);

    /// <summary>Varia Suit Samus's drain fraction: six sixteenths.</summary>
    internal static readonly EnergyAmount VariaDrainFraction = EnergyAmount.FromEnergyFraction(3, 8);

    /// <summary>Suitless Samus's drain fraction: twelve sixteenths.</summary>
    internal static readonly EnergyAmount PowerSuitDrainFraction = EnergyAmount.FromEnergyFraction(3, 4);

    /// <summary>How long a Metroid whose ice threshold is exhausted stays frozen, in updates.</summary>
    internal const int FrozenDuration = 400;

    /// <summary>How long a latched Metroid a Power Bomb shakes loose takes to escape Samus, in updates.</summary>
    internal const int EscapeDuration = 4;

    /// <summary>The fastest a chasing Metroid moves on each axis, in whole pixels an update.</summary>
    internal const int FastestChase = 3;

    /// <summary>The fastest a homing Metroid flies on each axis: three pixels an update.</summary>
    internal static readonly SignedSixteenSixteen FastestHoming = SignedSixteenSixteen.FromWholePixels(FastestChase);

    /// <summary>How many drops a frozen Metroid leaves when it dies, each chosen at random.</summary>
    internal const int FrozenDeathDropCount = 5;
}
