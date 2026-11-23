namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The counter cadences and phases of Phantoon's fight and death (#1606): his flash, fades, figure-eight reversal,
/// first eye-closed delay, flames, death pixelation and the Wrecked Ship's power returning.
/// </summary>
internal static partial class PhantoonCadenceDefinitions
{
    /// <summary>
    /// The phase of Phantoon's body's own update count on which his hurt flash shows: for two updates of every four,
    /// while his health colours show for the other two.
    /// </summary>
    internal static readonly UpdateAlternation HurtFlashShown = new(2);

    /// <summary>
    /// The updates on which Phantoon's colours take a fade step, in or out, counted on the frame counter at the period's
    /// even place.
    /// </summary>
    internal static readonly UpdateCadence FadeStep = new(2, 0);

    /// <summary>
    /// The phase of the frame counter on which Phantoon, starting a new figure eight, reverses it: on odd updates;
    /// on even ones he goes forward.
    /// </summary>
    internal static readonly UpdateAlternation FigureEightReverses = new(1);

    /// <summary>
    /// The phase of the frame counter that picks Phantoon's first eye-closed delay: four choices, each held in
    /// turn, indexing <see cref="PhantoonTimerDefinitions.EyeClosed"/>. Later rounds draw it at random.
    /// </summary>
    internal static readonly UpdateCycle FirstEyeClosedChoice = new(4, 2);

    /// <summary>
    /// The updates on which an orbiting flame, past its start delay, draws its orbit one pixel closer, counted on the enemy
    /// projectiles' update count at the period's odd place.
    /// </summary>
    internal static readonly UpdateCadence OrbitCloses = new(2, 1);

    /// <summary>
    /// The phase of the frame counter on which a rising flame drifts left: on odd updates; on even ones it drifts
    /// right.
    /// </summary>
    internal static readonly UpdateAlternation RisingFlameDriftsLeft = new(1);

    /// <summary>
    /// The updates on which the death pixelation over the dying Phantoon grows one size, counted on the frame counter at
    /// the period's first place.
    /// </summary>
    internal static readonly UpdateCadence DeathPixelationGrows = new(16, 0);

    /// <summary>
    /// The updates on which the Wrecked Ship's colours take a step back to powered once Phantoon is dead, counted on the
    /// frame counter at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence PowerReturnStep = new(4, 0);

    /// <summary>
    /// The phase of Phantoon's rage wave count on which a wave is an odd wave of eight flames (#1619): on its odd
    /// counts; the even waves, from the first, fire seven.
    /// </summary>
    /// <remarks>
    /// The count stays the decision (#2471): it is what ends his rage at its eighth wave, and the waves alternate on it, so
    /// a second record of which wave comes next would only repeat what the count already holds.
    /// </remarks>
    internal static readonly UpdateAlternation OddRageWave = new(1);

    /// <summary>
    /// The phase of the dying Phantoon's fade cycle count on which he fades back in (#1619): on its odd counts; on the
    /// even ones, from the first, he fades out.
    /// </summary>
    /// <remarks>
    /// The count stays the decision (#2471): it is what ends his dying fades at the tenth, and his fades alternate on it, so
    /// a second record of which way the next fade runs would only repeat what the count already holds.
    /// </remarks>
    internal static readonly UpdateAlternation DyingFadeIn = new(1);
}
