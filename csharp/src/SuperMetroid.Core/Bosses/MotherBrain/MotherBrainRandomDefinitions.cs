namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The random choices of Mother Brain's fight, her ascent and the baby metroid's scenes: holds, dust, the hand beam's
/// scatter, her attack choices and the chances her late phases take on the random sample (#1619).
/// </summary>
internal static class MotherBrainRandomDefinitions
{
    /// <summary>The roll most of her chances read.</summary>
    private static readonly RandomSampleField RollOf4096 = RandomSampleField.OneOf(4096);

    /// <summary>The roll her posture and phase-three attack chances read.</summary>
    private static readonly RandomSampleField RollOf256 = RandomSampleField.OneOf(256);

    /// <summary>The baby metroid's ambient cry on its way to Samus: the top 96 of the 4096 rolls, 4000 and over, cry.</summary>
    internal static readonly RandomChance BabyMetroidCries = RandomChance.Highest(96, RollOf4096);

    /// <summary>The Baby Metroid's hover: the top 32 of the 4096 rolls, 4064 and over, restart its hover.</summary>
    internal static readonly RandomChance BabyMetroidHoverRestarts = RandomChance.Highest(32, RollOf4096);

    /// <summary>
    /// A dust cloud of Mother Brain's ascent being the short big cloud rather than the small one (#2470): an even choice of
    /// the random number, one in two.
    /// </summary>
    internal static readonly RandomChance AscentDustIsBig = RandomChance.Highest(1, RandomSampleField.SecondOf(2));

    /// <summary>
    /// The phase-two neutral hold: the lowest fifteen of the sixteen values of the sample divided into sixteenths, every sample
    /// below 61440, hold the phase-two neutral pose again.
    /// </summary>
    internal static readonly RandomChance NeutralPhase2Holds = RandomChance.Lowest(15, RandomSampleField.RangePart(16));

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

    /// <summary>
    /// The chance her third phase attacks on an update she stands free to attack: the upper half of the samples, one in two.
    /// </summary>
    internal static readonly RandomChance ThirdPhaseAttacks = RandomChance.Highest(RandomSampleField.Whole.Outcomes / 2, RandomSampleField.Whole);

    /// <summary>
    /// The chance each update of her attacks on the Baby that she fires a volley: the upper half of the samples, one in
    /// two.
    /// </summary>
    internal static readonly RandomChance BabyVolleyFires = RandomChance.Highest(RandomSampleField.Whole.Outcomes / 2, RandomSampleField.Whole);

    /// <summary>A rotting corpse's dust offset, 0 through 31 pixels added to the brain's X less sixteen.</summary>
    internal static readonly RandomSampleField CorpseDustOffset = RandomSampleField.OneOf(32);

    /// <summary>
    /// Phase three's forward inch waiting longer, at her medium walk rather than her fast one (#2470): an even
    /// choice of the random number, one in two.
    /// </summary>
    internal static readonly RandomChance ForwardInchWaitsLonger = RandomChance.Highest(1, MotherBrainRandomRolls.ForwardInchWait);

    /// <summary>A fired hand beam's scatter speed, 0 through 7 whole pixels.</summary>
    internal static readonly RandomSampleField HandBeamScatterSpeed = RandomSampleField.SecondOf(8);

    /// <summary>The roll her second-phase attack choice reads: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField AttackRoll = RandomSampleField.OneOf(256);

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
    /// Her second phase's next move at full strength (<see cref="MotherBrainFightDefinitions.FullStrengthHealth"/> or more):
    /// one in sixteen tries an attack; the rest walk.
    /// </summary>
    internal static readonly RandomChance AttacksAtFullStrength = RandomChance.Lowest(4096, RandomSampleField.Whole);

    /// <summary>Her second phase's next move below full strength: one in eight walks.</summary>
    internal static readonly RandomChance WalksWhenWounded = RandomChance.Lowest(8192, RandomSampleField.Whole);

    /// <summary>
    /// Her second phase's next move below full strength: three in eight try an attack; the rest that do not walk fire the
    /// hand beam.
    /// </summary>
    internal static readonly RandomChance AttacksWhenWounded = RandomChance.Highest(24576, RandomSampleField.Whole);

    /// <summary>Her bomb walk: one in 512 skips the walk back and goes straight to the bomb.</summary>
    internal static readonly RandomChance BombWalkStaysPut = RandomChance.Highest(128, RandomSampleField.Whole);

    /// <summary>
    /// Her bomb walk's target: three in eight walk her back only to her short retreat; the rest that walk take her back
    /// to her long retreat.
    /// </summary>
    internal static readonly RandomChance BombWalkShortRetreat = RandomChance.Lowest(24576, RandomSampleField.Whole);

    /// <summary>
    /// The first-phase fake death's tube dust: one in four shows smoke; the rest a small explosion.
    /// </summary>
    internal static readonly RandomChance FakeDeathDustSmokes = RandomChance.Lowest(16384, RandomSampleField.Whole);

    /// <summary>
    /// The escape door's dust after her death: one in four shows smoke; the rest a small explosion.
    /// </summary>
    internal static readonly RandomChance EscapeDoorDustSmokes = RandomChance.Lowest(16384, RandomSampleField.Whole);

    /// <summary>
    /// The animation a mixed death explosion plays (<see cref="MotherBrainDeathExplosionDefinitions.Animations"/>): every
    /// sample below 16384, a quarter, the small explosion; every sample from 57344, an eighth, the big explosion; and
    /// every other the smoke.
    /// </summary>
    internal static readonly WeightedRandomChoice<MotherBrainDeathExplosionAnimation> MixedDeathExplosionAnimation = new(
        RandomSampleField.RangePart(8),
        new(2, MotherBrainDeathExplosionAnimation.SmallExplosion),
        new(5, MotherBrainDeathExplosionAnimation.Smoke),
        new(1, MotherBrainDeathExplosionAnimation.BigExplosion));

    /// <summary>A fired hand beam's scatter turn, in steps of the turn.</summary>
    internal static readonly RandomSampleField HandBeamScatterTurn = RandomSampleField.OneOf(256);

    /// <summary>A turret's wait before it chooses another direction, raised to its minimum: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField TurretRotationWait = RandomSampleField.OneOf(256);

    /// <summary>A turret's pause between bullets, raised to its minimum: 0 through 255 from the sample.</summary>
    internal static readonly RandomSampleField TurretFiringWait = RandomSampleField.OneOf(256);
}
