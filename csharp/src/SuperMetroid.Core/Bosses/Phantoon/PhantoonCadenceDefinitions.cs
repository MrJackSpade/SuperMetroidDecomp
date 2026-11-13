namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The counter cadences and phases of Phantoon's fight and death (#1606): his flash, fades, figure-eight reversal,
/// first eye-closed delay, flames, death mosaic and the Wrecked Ship's power returning.
/// </summary>
internal static class PhantoonCadenceDefinitions
{
    /// <summary>
    /// The phase of Phantoon's body's own update count on which his hurt flash shows: set for two updates of every four,
    /// while his health colours show for the other two.
    /// </summary>
    internal static readonly UpdateCounterField HurtFlashShown = new(1, 1);

    /// <summary>
    /// The updates on which Phantoon's colours take a fade step, in or out: every second update of the frame counter, on
    /// its even place.
    /// </summary>
    internal static readonly UpdateCadence FadeStepEverySecondUpdate = new(2, 0);

    /// <summary>
    /// The phase of the frame counter on which Phantoon, starting a new figure eight, reverses it: set on odd updates;
    /// on even ones he goes forward.
    /// </summary>
    internal static readonly UpdateCounterField FigureEightReverses = new(0, 1);

    /// <summary>
    /// The phase of the frame counter that picks Phantoon's first eye-closed delay: four choices, each held for two
    /// updates, indexing <see cref="PhantoonTimerDefinitions.EyeClosed"/>. Later rounds draw it at random.
    /// </summary>
    internal static readonly UpdateCounterField FirstEyeClosedChoice = new(1, 2);

    /// <summary>
    /// The updates on which an orbiting flame, past its start delay, draws its orbit one pixel closer: every second update
    /// of the enemy projectiles' frame counter of 0 through 65535, on its odd place.
    /// </summary>
    internal static readonly UpdateCadence OrbitClosesEverySecondUpdate = new(2, 1);

    /// <summary>
    /// The phase of the frame counter on which a rising flame drifts left: set on odd updates; on even ones it drifts
    /// right.
    /// </summary>
    internal static readonly UpdateCounterField RisingFlameDriftsLeft = new(0, 1);

    /// <summary>
    /// The updates on which the death mosaic over the dying Phantoon grows one size: every sixteenth update of the frame
    /// counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence DeathMosaicGrowsEverySixteenthUpdate = new(16, 0);

    /// <summary>
    /// The updates on which the Wrecked Ship's colours take a step back to powered once Phantoon is dead: every fourth
    /// update of the frame counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence PowerReturnStepEveryFourthUpdate = new(4, 0);

    /// <summary>
    /// The phase of Phantoon's rage wave count on which a wave is an odd wave of eight flames (#1619): set on its odd
    /// counts; the even waves, from the first, fire seven.
    /// </summary>
    internal static readonly UpdateCounterField OddRageWave = new(0, 1);

    /// <summary>
    /// The phase of the dying Phantoon's fade cycle count on which he fades back in (#1619): set on its odd counts; on the
    /// even ones, from the first, he fades out.
    /// </summary>
    internal static readonly UpdateCounterField DyingFadeIn = new(0, 1);
}
