namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The random choices of Mother Brain's fight, her ascent and the baby metroid's scenes: holds, dust, the hand beam's
/// scatter, her attack choices and the chances her late phases take on the random sample (#1619).
/// </summary>
internal static class MotherBrainRandomDefinitions
{
    /// <summary>The roll most of her chances read: the sample modulo 4096, 0 through 4095.</summary>
    private static readonly RandomSampleField RollOf4096 = new(0, 12);

    /// <summary>The roll her posture and phase-three attack chances read: the sample modulo 256, 0 through 255.</summary>
    private static readonly RandomSampleField RollOf256 = new(0, 8);

    /// <summary>The baby metroid's ambient cry on its way to Samus: the top 96 of the 4096 rolls, 4000 and over, cry.</summary>
    internal static readonly RandomChance BabyMetroidCries = RandomChance.Highest(96, RollOf4096);

    /// <summary>The Shitroid's hover: the top 32 of the 4096 rolls, 4064 and over, restart its hover.</summary>
    internal static readonly RandomChance ShitroidHoverRestarts = RandomChance.Highest(32, RollOf4096);

    /// <summary>Whether a dust cloud of Mother Brain's ascent is the short big cloud: taken when the sample divided by 256 is odd.</summary>
    internal static readonly RandomSampleField AscentDustBig = new(8, 1);

    /// <summary>
    /// The phase-two neutral hold: the lowest fifteen of the sixteen values of the sample's top four bits, every sample
    /// below 61440, hold the phase-two neutral pose again.
    /// </summary>
    internal static readonly RandomChance NeutralPhase2Holds = RandomChance.Lowest(15, new RandomSampleField(12, 4));

    /// <summary>The phase-three neutral hold: the lowest 3776 of the 4096 rolls hold the neutral pose again.</summary>
    internal static readonly RandomChance NeutralPhase3Holds = RandomChance.Lowest(3776, RollOf4096);

    /// <summary>The dying drool: the top 32 of the 4096 rolls, 4064 and over, burst into more drool.</summary>
    internal static readonly RandomChance DroolBursts = RandomChance.Highest(32, RollOf4096);

    /// <summary>
    /// The finish-off attack while Samus is drained: the lowest 4000 of the 4096 rolls attack with nothing, so her body may
    /// only change posture.
    /// </summary>
    internal static readonly RandomChance FinishOffHoldsBack = RandomChance.Lowest(4000, RollOf4096);

    /// <summary>
    /// The finish-off attack's bomb: the top 16 of the 4096 rolls, 4080 and over, choose a bomb over two onion rings while
    /// Samus is above the wounded threshold.
    /// </summary>
    internal static readonly RandomChance FinishOffBombs = RandomChance.Highest(16, RollOf4096);

    /// <summary>
    /// Whether Mother Brain finishing Samus off or reviving changes posture: the top 64 of the 256 rolls, 192 and over, lean
    /// her down from standing or stand her up from leaning.
    /// </summary>
    internal static readonly RandomChance PostureChanges = RandomChance.Highest(64, RollOf256);

    /// <summary>Phase three's attack: the lowest 128 of the 256 rolls drop a bomb, the rest fire four onion rings.</summary>
    internal static readonly RandomChance ThirdPhaseBombs = RandomChance.Lowest(128, RollOf256);

    /// <summary>A rotting corpse's dust offset, 0 through 31 pixels added to the brain's X less sixteen: the sample modulo 32.</summary>
    internal static readonly RandomSampleField CorpseDustOffset = new(0, 5);

    /// <summary>Whether phase three's forward inch waits two updates longer: taken when the sample divided by 2 is odd.</summary>
    internal static readonly RandomSampleField ForwardInchLongerDelay = new(1, 1);

    /// <summary>A fired hand beam's scatter speed, 0 through 7 whole pixels: the sample divided by 256, modulo 8.</summary>
    internal static readonly RandomSampleField HandBeamScatterSpeed = new(8, 3);

    /// <summary>The roll her second-phase attack choice reads: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField AttackRoll = new(0, 8);

    /// <summary>
    /// Her second-phase attack's pressing half: the top 128 of the 256 attack rolls give airborne Samus four onion rings at
    /// once, and grounded Samus a bomb walk when no bomb of hers is live; the rest leave airborne Samus the laser and
    /// grounded Samus the attack her distance chooses.
    /// </summary>
    internal static readonly RandomChance AttackPresses = RandomChance.Highest(128, AttackRoll);

    /// <summary>
    /// The attack the 256 attack rolls choose for grounded Samus within 32 pixels of her brain's height: the lowest 16 the
    /// neutral pose, the next 16 four onion rings, the next 176 the laser and the top 48 a bomb.
    /// </summary>
    internal static readonly WeightedRandomChoice<MotherBrainSecondPhaseAttack> NearAttack = new(
        AttackRoll,
        new(16, MotherBrainSecondPhaseAttack.Neutral),
        new(16, MotherBrainSecondPhaseAttack.FourOnionRings),
        new(176, MotherBrainSecondPhaseAttack.Laser),
        new(48, MotherBrainSecondPhaseAttack.Bomb));

    /// <summary>
    /// The attack the 256 attack rolls choose for grounded Samus farther from her brain's height: a quarter each, in order,
    /// for the neutral pose, four onion rings, the laser and a bomb.
    /// </summary>
    internal static readonly WeightedRandomChoice<MotherBrainSecondPhaseAttack> FarAttack = new(
        AttackRoll,
        new(64, MotherBrainSecondPhaseAttack.Neutral),
        new(64, MotherBrainSecondPhaseAttack.FourOnionRings),
        new(64, MotherBrainSecondPhaseAttack.Laser),
        new(64, MotherBrainSecondPhaseAttack.Bomb));

    /// <summary>
    /// Her second phase's next move at 4500 health or more: the lowest 4096 of the 65536 samples, one in sixteen, try an
    /// attack; the rest walk.
    /// </summary>
    internal static readonly RandomChance AttacksAtFullStrength = RandomChance.Lowest(4096, RandomSampleField.Whole);

    /// <summary>Her second phase's next move below 4500 health: the lowest 8192 of the 65536 samples, one in eight, walk.</summary>
    internal static readonly RandomChance WalksWhenWounded = RandomChance.Lowest(8192, RandomSampleField.Whole);

    /// <summary>
    /// Her second phase's next move below 4500 health: the top 24576 of the 65536 samples, three in eight, try an attack;
    /// the samples between walking and attacking fire the hand beam.
    /// </summary>
    internal static readonly RandomChance AttacksWhenWounded = RandomChance.Highest(24576, RandomSampleField.Whole);

    /// <summary>Her bomb walk: the top 128 of the 65536 samples skip the walk back and go straight to the bomb.</summary>
    internal static readonly RandomChance BombWalkStaysPut = RandomChance.Highest(128, RandomSampleField.Whole);

    /// <summary>
    /// Her bomb walk's target: the lowest 24576 of the 65536 samples walk her back only to X 96; the rest of the samples that
    /// walk take her back to X 64.
    /// </summary>
    internal static readonly RandomChance BombWalkShortRetreat = RandomChance.Lowest(24576, RandomSampleField.Whole);

    /// <summary>
    /// The first-phase fake death's tube dust: the lowest 16384 of the 65536 samples, a quarter, show smoke; the rest a small
    /// explosion.
    /// </summary>
    internal static readonly RandomChance FakeDeathDustSmokes = RandomChance.Lowest(16384, RandomSampleField.Whole);

    /// <summary>
    /// The escape door's dust after her death: the lowest 16384 of the 65536 samples, a quarter, show smoke; the rest a small
    /// explosion.
    /// </summary>
    internal static readonly RandomChance EscapeDoorDustSmokes = RandomChance.Lowest(16384, RandomSampleField.Whole);

    /// <summary>
    /// The program a mixed death explosion plays, as its projectile parameter, an index into
    /// <see cref="MotherBrainDeathExplosionDefinitions.Programs"/>, read from the sample's top three bits: their lowest two
    /// values, every sample below 16384, the small explosion (zero), the next five smoke (one) and the top one, every sample
    /// from 57344, the big explosion (two).
    /// </summary>
    internal static readonly WeightedRandomChoice<int> MixedDeathExplosionProgram = new(
        new RandomSampleField(13, 3),
        new(2, 0),
        new(5, 1),
        new(1, 2));

    /// <summary>A fired hand beam's scatter turn, in 256ths of a turn: the sample modulo 256.</summary>
    internal static readonly RandomSampleField HandBeamScatterTurn = new(0, 8);

    /// <summary>A turret's wait before it chooses another direction, raised to its minimum: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField TurretRotationWait = new(0, 8);

    /// <summary>A turret's pause between bullets, raised to its minimum: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField TurretFiringWait = new(0, 8);
}
