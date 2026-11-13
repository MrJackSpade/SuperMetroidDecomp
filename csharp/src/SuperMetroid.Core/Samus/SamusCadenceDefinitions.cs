namespace SuperMetroid.Core.Samus;

/// <summary>
/// The counter cadences and phases of Samus's movement, abilities, sounds and drawing (#1606): her block scans' order,
/// Crystal Flash's drain, her liquid damage sounds and bubbles, her reserve refill sound, and her flickers and echoes.
/// </summary>
internal static class SamusCadenceDefinitions
{
    /// <summary>
    /// The updates on which Samus's vertical block moves and pose-change probes scan the blocks beside her from left to
    /// right: every second update of the frame counter, on its even place; on odd updates they scan right to left.
    /// Which of two simultaneous block reactions wins can depend on the order.
    /// </summary>
    internal static readonly UpdateCadence BlockScansLeftToRight = new(2, 0);

    /// <summary>
    /// The updates on which Crystal Flash takes its ammunition and gives its energy: every eighth update of the frame
    /// counter, on its first place. Crystal Flash keeps no cadence of its own.
    /// </summary>
    internal static readonly UpdateCadence CrystalFlashDrainEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The updates on which Samus taking lava or acid damage queues the energy sound: every eighth update of the frame
    /// counter, on its first place, while she has 71 energy or more.
    /// </summary>
    internal static readonly UpdateCadence LiquidDamageSoundEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The updates on which Samus under water may breathe out a bubble: every 128th update of the frame counter, on its
    /// first place, when her mouth is under the surface and a bubble can form.
    /// </summary>
    internal static readonly UpdateCadence BreathBubbleEvery128thUpdate = new(128, 0);

    /// <summary>
    /// The updates on which Samus diving into lava queues the spray's sound with the spray: every second update of the
    /// frame counter, on its even place.
    /// </summary>
    internal static readonly UpdateCadence LavaSpraySoundEverySecondUpdate = new(2, 0);

    /// <summary>
    /// The updates on which the reserve tanks refilling Samus queue the refill sound: every eighth update of the frame
    /// counter, on its first place, while reserve energy remains.
    /// </summary>
    internal static readonly UpdateCadence ReserveRefillSoundEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The updates on which the speed booster's echoes take Samus's position: every fourth update of the game clock's
    /// frames within its second, on its first place.
    /// </summary>
    internal static readonly UpdateCadence SpeedEchoCaptureEveryFourthUpdate = new(4, 0);

    /// <summary>
    /// The phase of the frame counter on which invincible Samus, and her arm cannon with her, is hidden: set on odd
    /// updates, so she flickers, unless knockback or a stored shine keeps her shown.
    /// </summary>
    internal static readonly UpdateCounterField InvincibilityFlickerHidden = new(0, 1);

    /// <summary>
    /// The updates on which the shinespark crash's echoes are drawn, orbiting or released: every second update of the
    /// frame counter, on its odd place.
    /// </summary>
    internal static readonly UpdateCadence CrashEchoesDrawnEverySecondUpdate = new(2, 1);

    /// <summary>
    /// The phase of the hurt flash's count on which Samus's suit shows the hurt colours: set on its odd counts, so they
    /// alternate with her own colours each update until the count reaches seven.
    /// </summary>
    internal static readonly UpdateCounterField HurtColoursShown = new(0, 1);

    /// <summary>
    /// The phase of the frame counter on which an uncharged power, ice or wave shot in an even slot is drawn: set on odd
    /// updates; a shot in an odd slot is drawn while it is clear, so neighbouring shots flicker on opposite updates.
    /// </summary>
    internal static readonly UpdateCounterField BeamDrawnInEvenSlot = new(0, 1);

    /// <summary>
    /// The phase of the frame counter on which an uncharged spazer or plasma shot in an odd slot is drawn: set for two
    /// updates of every four; a shot in an even slot is drawn while it is clear.
    /// </summary>
    internal static readonly UpdateCounterField WideBeamDrawnInOddSlot = new(1, 1);

    /// <summary>
    /// The updates on which Samus riding an elevator is drawn: every second update of the frame counter, on its even
    /// place, so she flickers on the elevator.
    /// </summary>
    internal static readonly UpdateCadence ElevatorRideDrawnEverySecondUpdate = new(2, 0);
}
