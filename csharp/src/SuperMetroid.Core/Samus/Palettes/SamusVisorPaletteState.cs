using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// The room's animation of Samus's visor: a countdown of suit-colouring
/// updates and the visor shade shown next.
/// </summary>
/// <remarks>
/// This is intentionally not a generic color tween. In rooms that animate the visor (<see cref="RoomVisorAnimation"/>),
/// the animation rotates only Samus's visor through the last three of the six visor shades. The
/// countdown and the shade are only ever set by this type: the reset installs countdown one at the cycle's first shade,
/// and every expiry reloads the countdown to the frame delay and moves through the cycle's three shades. The countdown
/// therefore holds one through the frame delay between updates and never counts down from zero.
/// </remarks>
public sealed class SamusVisorPaletteState
{
    /// <summary>Updates left after the reset before the first visor shade is shown.</summary>
    private const int ResetTimer = 1;

    /// <summary>
    /// Updates left before the next visor shade, one through
    /// <see cref="SamusPaletteDefinitions.Visor.FrameDelay"/>; it reads the frame delay on the update that showed a shade.
    /// </summary>
    public int Timer { get; private set; } = ResetTimer;

    /// <summary>
    /// The visor shade shown next: one of the steady cycle's three, from
    /// <see cref="SamusPaletteDefinitions.Visor.CycleFirstShade"/> up to the shade count.
    /// </summary>
    public int NextShade { get; private set; } = SamusPaletteDefinitions.Visor.CycleFirstShade;

    /// <summary>
    /// Advances one update and optionally shows the next shade on Samus's visor.
    /// </summary>
    /// <param name="colours">Samus's shown colours.</param>
    /// <param name="specialSamusPaletteType">The special palette that owns Samus's colours, if any.</param>
    /// <param name="roomVisorAnimation">Whether the room animates the visor (<see cref="RoomFxOverlayState.VisorAnimation"/>).</param>
    public void Update(
        SamusColours colours,
        SamusSpecialPaletteType specialSamusPaletteType,
        RoomVisorAnimation roomVisorAnimation)
    {
        Ensure.NotNull(colours);

        // X-ray's suit colouring owns the same visor. The room's visor animation returns immediately,
        // preserving the countdown and shade so ordinary room animation resumes where it stopped.
        if (specialSamusPaletteType == SamusSpecialPaletteType.Xray)
        {
            return;
        }

        // Every other room resets the animation on every call. Besides disabling the animation,
        // this primes an immediate color write on the first call after entering a backdrop-
        // blending room: countdown one expires before the shade is consulted.
        if (roomVisorAnimation != RoomVisorAnimation.Animated)
        {
            Timer = ResetTimer;
            NextShade = SamusPaletteDefinitions.Visor.CycleFirstShade;
            return;
        }

        Timer = DecrementTimer(Timer);
        if (Timer != 0)
        {
            return;
        }

        // The visor handler reloads the expired countdown to the frame delay and keeps the shade.
        Timer = SamusPaletteDefinitions.Visor.FrameDelay;
        int shade = NextShade;
        colours.ShowVisor(SamusShades.Visor(shade));

        // Only the cycle's three shades belong to the room-backdrop cycle. The shades before
        // them are X-ray widening colors and are selected by X-ray's separate suit colouring.
        int nextShade = shade + 1;
        NextShade = nextShade < SamusPaletteDefinitions.Visor.CycleEndShade
            ? nextShade
            : SamusPaletteDefinitions.Visor.CycleFirstShade;
    }

    /// <summary>
    /// One call's countdown step. The countdown is one or more on every call (see the remarks), so the step never
    /// reaches the game's borrow into the shade; a zero countdown is a broken invariant and fails.
    /// </summary>
    private static int DecrementTimer(int timer)
    {
        if (timer <= 0)
            throw new InvalidOperationException($"Visor countdown {timer} was decremented below its expiry.");
        return timer - 1;
    }
}
