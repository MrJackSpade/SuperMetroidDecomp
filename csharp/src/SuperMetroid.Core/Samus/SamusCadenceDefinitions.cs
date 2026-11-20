namespace SuperMetroid.Core.Samus;

/// <summary>
/// The counter cadences and phases of Samus's movement, abilities, sounds and drawing (#1606): her block scans' order,
/// Crystal Flash's drain, her liquid damage sounds and bubbles, her reserve refill sound, and her flickers and echoes.
/// </summary>
internal static class SamusCadenceDefinitions
{
    /// <summary>
    /// The updates on which Samus's vertical block moves and pose-change probes scan the blocks beside her from left to
    /// right, counted on the frame counter at the period's even place; on odd updates they scan right to left.
    /// Which of two simultaneous block reactions wins can depend on the order.
    /// </summary>
    internal static readonly UpdateCadence BlockScansLeftToRight = new(2, 0);

    /// <summary>
    /// The updates on which Crystal Flash takes its ammunition and gives its energy, counted on the frame counter at the
    /// period's first place. Crystal Flash keeps no cadence of its own.
    /// </summary>
    internal static readonly UpdateCadence CrystalFlashDrain = new(8, 0);

    /// <summary>
    /// The updates on which Samus taking lava or acid damage queues the energy sound, counted on the frame counter at the
    /// period's first place, while her energy is above the low-energy warning's.
    /// </summary>
    internal static readonly UpdateCadence LiquidDamageSound = new(8, 0);

    /// <summary>
    /// The updates on which Samus under water may breathe out a bubble, counted on the frame counter at the period's first
    /// place, when her mouth is under the surface and a bubble can form.
    /// </summary>
    internal static readonly UpdateCadence BreathBubble = new(128, 0);

    /// <summary>
    /// The updates on which Samus diving into lava queues the spray's sound with the spray, counted on the frame counter
    /// at the period's even place.
    /// </summary>
    internal static readonly UpdateCadence LavaSpraySound = new(2, 0);

    /// <summary>
    /// The updates on which the reserve tanks refilling Samus queue the refill sound, counted on the frame counter at the
    /// period's first place, while reserve energy remains.
    /// </summary>
    internal static readonly UpdateCadence ReserveRefillSound = new(8, 0);

    /// <summary>
    /// The updates on which the speed booster's echoes take Samus's position, counted on the game clock's frames within
    /// its second at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence SpeedEchoCapture = new(4, 0);

    /// <summary>
    /// The phase of the frame counter on which invincible Samus, and her arm cannon with her, is hidden: set on odd
    /// updates, so she flickers, unless knockback or a stored shine keeps her shown.
    /// </summary>
    internal static readonly UpdateCounterField InvincibilityFlickerHidden = new(0, 1);

    /// <summary>
    /// The updates on which the shinespark crash's echoes are drawn, orbiting or released, counted on the frame counter at
    /// the period's odd place.
    /// </summary>
    internal static readonly UpdateCadence CrashEchoesDrawn = new(2, 1);

    /// <summary>
    /// The phase of the hurt flash's count on which Samus's suit shows the hurt colours: set on its odd counts, so they
    /// alternate with her own colours each update until the count reaches seven.
    /// </summary>
    internal static readonly UpdateCounterField HurtColoursShown = new(0, 1);

    /// <summary>
    /// The updates on which Samus riding an elevator is drawn, counted on the frame counter at the period's even place, so
    /// she flickers on the elevator.
    /// </summary>
    internal static readonly UpdateCadence ElevatorRideDrawn = new(2, 0);
}
