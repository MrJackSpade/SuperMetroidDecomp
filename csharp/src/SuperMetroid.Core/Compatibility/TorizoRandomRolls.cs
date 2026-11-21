namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The parts of the shared random number the Torizos' choices read where the game read neither the number's ordinary
/// roll, its second roll nor its range (#2563): Bomb Torizo's drool, Golden Torizo's eggs and medium-range attack, a
/// turning drool's angle, the eye beam's spread and the form a drool takes. A decision of several places is a
/// RandomClearBitsChance, taken when every one of them reads zero, so the number of places sets its odds.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number each of the Torizos' random choices reads, exactly as the game read it")]
internal static class TorizoRandomRolls
{
    /// <summary>
    /// Bomb Torizo drools while hurt when the number's second, seventh, ninth and sixteenth places all read zero: one time
    /// in sixteen.
    /// </summary>
    internal static RandomClearBitsChance LowHealthDrool { get; } = RandomClearBitsChance.WhenAllClear(new RandomSampleField(1, 1), new RandomSampleField(6, 1), new RandomSampleField(8, 1), new RandomSampleField(15, 1));

    /// <summary>
    /// Golden Torizo releases its eggs at low health when the number's second and ninth places both read zero: one time in
    /// four.
    /// </summary>
    internal static RandomClearBitsChance LowHealthEggs { get; } = RandomClearBitsChance.WhenAllClear(new RandomSampleField(1, 1), new RandomSampleField(8, 1));

    /// <summary>
    /// Golden Torizo's medium-range attack is taken when the number's fifth and ninth places both read zero: one time in
    /// four.
    /// </summary>
    internal static RandomClearBitsChance MediumRangeAttack { get; } = RandomClearBitsChance.WhenAllClear(new RandomSampleField(4, 1), new RandomSampleField(8, 1));

    /// <summary>
    /// A turning Torizo's drool angle, any of the 256 steps of a turn: the number's second through ninth places.
    /// </summary>
    internal static RandomSampleField DroolTurningAngle { get; } = new(1, 8);

    /// <summary>
    /// Golden Torizo's eye-beam angle spread, 0 through 15 steps of the turn: the number's second through fifth places.
    /// </summary>
    internal static RandomSampleField EyeBeamSpread { get; } = new(1, 4);

    /// <summary>
    /// The roll of eight a low-health drool's form is chosen by: the number's third through fifth places.
    /// </summary>
    internal static RandomSampleField LowHealthDroolForm { get; } = new(2, 3);
}
