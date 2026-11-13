namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The counter cadences and phases of Mother Brain's fight, the Baby Metroid's rescue and the Shitroid's drain (#1606):
/// their shakes, flashes, sounds, rise, posture changes and the rainbow beam's colours and ammunition drain.
/// </summary>
internal static class MotherBrainCadenceDefinitions
{
    /// <summary>
    /// The phase of the Baby Metroid's own update count that steps its shake: four steps, each held for two updates,
    /// indexing its shaking offsets while it latches onto Mother Brain or Samus or recoils from the fatal blow.
    /// </summary>
    internal static readonly UpdateCounterField BabyShakeStep = new(1, 2);

    /// <summary>
    /// The phase of the Baby Metroid's hit-flash timer that steps its doubled shake while it is struck: four steps, each
    /// held for two updates, indexing its shaking offsets.
    /// </summary>
    internal static readonly UpdateCounterField BabyHitShakeStep = new(1, 2);

    /// <summary>
    /// The phase of the Baby Metroid's hit-flash timer on which it shows its hurt flash: set for two updates of every
    /// four, while its own colours show for the other two.
    /// </summary>
    internal static readonly UpdateCounterField BabyHurtFlashShown = new(1, 1);

    /// <summary>
    /// The phase of the Baby Metroid's own update count on which the dying Baby Metroid is shown: set on odd updates, so
    /// it flickers, hidden on every even one.
    /// </summary>
    internal static readonly UpdateCounterField BabyDeathFlickerShown = new(0, 1);

    /// <summary>
    /// The enemy passes on which the Baby Metroid healing Samus queues the energy sound: every eighth pass of the
    /// enemy-pass counter, on its first place, while Samus has <see cref="MotherBrainHealthSounds.MinimumEnergy"/> or more.
    /// </summary>
    internal static readonly UpdateCadence BabyHealSoundEveryEighthEnemyPass = new(8, 0);

    /// <summary>The updates on which Mother Brain rises from her tank: every fourth update of the frame counter, on its first place.</summary>
    internal static readonly UpdateCadence AscentStepEveryFourthUpdate = new(4, 0);

    /// <summary>
    /// The enemy passes on which Mother Brain's rotting corpse queues its dust's liquid-damage sound with the dust: every
    /// eighth pass of the enemy-pass counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence CorpseDustSoundEveryEighthEnemyPass = new(8, 0);

    /// <summary>
    /// The updates on which Mother Brain finishing Samus off may stand up or lean down: every 32nd update of her body's
    /// own update count, on its first place, when her attack roll does not attack.
    /// </summary>
    internal static readonly UpdateCadence FinishOffPostureEveryThirtySecondUpdate = new(32, 0);

    /// <summary>
    /// The phase of Mother Brain's hurt flash timer on which her brain shows the white hurt colours: set on its odd
    /// counts. A second hit during a flash restarts it with the same phase, so a white update stays white.
    /// </summary>
    internal static readonly UpdateCounterField HurtFlashWhite = new(0, 1);

    /// <summary>
    /// The updates on which Mother Brain's body and the background behind it are hidden while her body fades out in
    /// death: every second update of her body's own update count, on its even place, so they flicker.
    /// </summary>
    internal static readonly UpdateCadence DeathFadeHidesBodyEverySecondUpdate = new(2, 0);

    /// <summary>
    /// The phase of Mother Brain's hurt flash timer that steps her neck's shake: four steps, each held for two updates,
    /// indexing her shake offsets.
    /// </summary>
    internal static readonly UpdateCounterField NeckShakeStep = new(1, 2);

    /// <summary>
    /// The phase of Mother Brain's brain shake count that steps her brain's shake (#1619): four steps, each held for two
    /// updates, indexing her shake offsets. The count is her brain's own shake timer while it runs, or else her hurt flash
    /// timer, or else her shake timer.
    /// </summary>
    internal static readonly UpdateCounterField BrainShakeStep = new(1, 2);

    /// <summary>
    /// The phase of Mother Brain's body's own update count on which the rainbow beam steps its colours: set for two
    /// updates of every four, while the colours hold for the other two.
    /// </summary>
    internal static readonly UpdateCounterField RainbowColoursStep = new(1, 1);

    /// <summary>
    /// The enemy passes on which the rainbow beam drains Samus's missiles and super missiles: every fourth pass of the
    /// enemy-pass counter, on its first place. Her power bombs drain on every pass.
    /// </summary>
    internal static readonly UpdateCadence RainbowMissileDrainEveryFourthEnemyPass = new(4, 0);

    /// <summary>
    /// The phase of the Shitroid's own update count that steps its shake above the victim it drains or over Samus: four
    /// steps, each held for two updates, indexing its shake offsets.
    /// </summary>
    internal static readonly UpdateCounterField ShitroidShakeStep = new(1, 2);
}
