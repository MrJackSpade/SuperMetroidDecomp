namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The counter cadences and phases of Mother Brain's fight, the Baby Metroid's rescue and the Baby Metroid's drain (#1606):
/// their shakes, flashes, sounds, rise, posture changes and the rainbow beam's colours and ammunition drain.
/// </summary>
internal static partial class MotherBrainCadenceDefinitions
{
    /// <summary>
    /// The phase of the Baby Metroid's own update count that steps its shake: four steps, each held in turn,
    /// indexing its shaking offsets while it latches onto Mother Brain or Samus or recoils from the fatal blow.
    /// </summary>
    internal static readonly UpdateCycle BabyShakeStep = new(4, 2);

    /// <summary>
    /// The phase of the Baby Metroid's hit timer that steps its doubled shake while it is struck: four steps, each held in
    /// turn, indexing its shaking offsets.
    /// </summary>
    internal static readonly UpdateCycle BabyHitShakeStep = new(4, 2);

    /// <summary>
    /// The phase of the Baby Metroid's own update count on which the dying Baby Metroid is shown: on odd updates, so
    /// it flickers, hidden on every even one.
    /// </summary>
    internal static readonly UpdateAlternation BabyDeathFlickerShown = new(1);

    /// <summary>
    /// The enemy passes on which the Baby Metroid healing Samus queues the energy sound, counted on the enemy-pass counter
    /// at the period's first place, while Samus has <see cref="MotherBrainHealthSounds.MinimumEnergy"/> or more.
    /// </summary>
    internal static readonly UpdateCadence BabyHealSound = new(8, 0);

    /// <summary>The updates on which Mother Brain rises from her tank, counted on the frame counter at the period's first place.</summary>
    internal static readonly UpdateCadence AscentStep = new(4, 0);

    /// <summary>
    /// The enemy passes on which Mother Brain's rotting corpse queues its dust's liquid-damage sound with the dust,
    /// counted on the enemy-pass counter at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence CorpseDustSound = new(8, 0);

    /// <summary>
    /// The updates on which Mother Brain finishing Samus off may stand up or lean down, counted on her body's own update
    /// count at the period's first place, when her attack roll does not attack.
    /// </summary>
    internal static readonly UpdateCadence FinishOffPosture = new(32, 0);

    /// <summary>
    /// The phase of Mother Brain's hurt flash timer on which her brain shows the white hurt colours: on its odd
    /// counts. A second hit during a flash restarts it with the same phase, so a white update stays white.
    /// </summary>
    internal static readonly UpdateAlternation HurtFlashWhite = new(1);

    /// <summary>
    /// The updates on which Mother Brain's body and the background behind it are hidden while her body fades out in
    /// death, counted on her body's own update count at the period's even place, so they flicker.
    /// </summary>
    internal static readonly UpdateCadence DeathFadeHidesBody = new(2, 0);

    /// <summary>
    /// The phase of Mother Brain's hurt flash timer that steps her neck's shake: four steps, each held in turn,
    /// indexing her shake offsets.
    /// </summary>
    internal static readonly UpdateCycle NeckShakeStep = new(4, 2);

    /// <summary>
    /// The phase of Mother Brain's brain shake count that steps her brain's shake (#1619): four steps, each held in
    /// turn, indexing her shake offsets. The count is her brain's own shake timer while it runs, or else her hurt flash
    /// timer, or else her shake timer.
    /// </summary>
    internal static readonly UpdateCycle BrainShakeStep = new(4, 2);

    /// <summary>
    /// The phase of Mother Brain's body's own update count on which the rainbow beam steps its colours: for two
    /// updates of every four, while the colours hold for the other two.
    /// </summary>
    internal static readonly UpdateAlternation RainbowColoursStep = new(2);

    /// <summary>
    /// The enemy passes on which the rainbow beam drains Samus's missiles and super missiles, counted on the enemy-pass
    /// counter at the period's first place. Her power bombs drain on every pass.
    /// </summary>
    internal static readonly UpdateCadence RainbowMissileDrain = new(4, 0);

    /// <summary>
    /// The phase of the Baby Metroid's own update count that steps its shake above the victim it drains or over Samus: four
    /// steps, each held in turn, indexing its shake offsets.
    /// </summary>
    internal static readonly UpdateCycle BabyMetroidShakeStep = new(4, 2);

}
