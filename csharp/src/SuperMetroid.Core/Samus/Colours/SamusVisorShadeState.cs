using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The room's animation of Samus's visor: a countdown of suit-colouring
/// updates and the visor shade shown next.
/// </summary>
/// <remarks>
/// This is intentionally not a generic color tween. In rooms that animate the visor (<see cref="RoomVisorAnimation"/>),
/// the animation rotates only Samus's visor through the steady visor cycle's shades (#2285). The
/// countdown and the shade are only ever set by this type: the reset starts a countdown of one at the cycle's first shade,
/// and every expiry reloads the countdown to the frame delay and moves through the cycle's three shades. The countdown
/// therefore holds one through the frame delay between updates and never counts down from zero.
/// </remarks>
public sealed class SamusVisorShadeState
{
    /// <summary>Updates left after the reset before the first visor shade is shown.</summary>
    private const int ResetTimer = 1;

    /// <summary>
    /// Updates left before the next visor shade, one through
    /// <see cref="SamusShadeDefinitions.Visor.FrameDelay"/>; it reads the frame delay on the update that showed a shade.
    /// </summary>
    public int Timer { get; private set; } = ResetTimer;

    /// <summary>
    /// The steady visor cycle's shade shown next, 0 through <see cref="SamusShadeDefinitions.Visor.CycleShades"/> less one.
    /// </summary>
    public int NextShade { get; private set; }

    /// <summary>
    /// Advances one update and optionally shows the next shade on Samus's visor.
    /// </summary>
    /// <param name="colours">Samus's shown colours.</param>
    /// <param name="specialColouringKind">The special colouring that owns Samus's colours, if any.</param>
    /// <param name="roomVisorAnimation">Whether the room animates the visor (<see cref="RoomFxOverlayState.VisorAnimation"/>).</param>
    public void Update(
        SamusColours colours,
        SamusSpecialColouringKind specialColouringKind,
        RoomVisorAnimation roomVisorAnimation)
    {
        Ensure.NotNull(colours);

        // X-ray's suit colouring owns the same visor. The room's visor animation returns immediately,
        // preserving the countdown and shade so ordinary room animation resumes where it stopped.
        if (specialColouringKind == SamusSpecialColouringKind.Xray)
        {
            return;
        }

        // Every other room resets the animation on every update. Besides disabling the animation,
        // this changes the visor's colour at once on the first update after entering a backdrop-
        // blending room: countdown one expires before the shade is consulted.
        if (roomVisorAnimation != RoomVisorAnimation.Animated)
        {
            Timer = ResetTimer;
            NextShade = 0;
            return;
        }

        Timer = DecrementTimer(Timer);
        if (Timer != 0)
        {
            return;
        }

        // The visor's colour step reloads the expired countdown to the frame delay and keeps the shade.
        Timer = SamusShadeDefinitions.Visor.FrameDelay;
        int shade = NextShade;
        colours.ShowVisor(SamusShades.VisorCycle(shade));

        // The room shows only the steady cycle; the X-ray widening glow is X-ray's own suit colouring.
        int nextShade = shade + 1;
        NextShade = nextShade < SamusShadeDefinitions.Visor.CycleShades ? nextShade : 0;
    }

    /// <summary>
    /// One update's countdown step. The countdown is one or more on every update (see the remarks), so the step never
    /// takes a shade off a zero countdown; a zero countdown is a broken invariant and fails.
    /// </summary>
    private static int DecrementTimer(int timer)
    {
        if (timer <= 0)
            throw new InvalidOperationException($"Visor countdown {timer} was decremented below its expiry.");
        return timer - 1;
    }
}
