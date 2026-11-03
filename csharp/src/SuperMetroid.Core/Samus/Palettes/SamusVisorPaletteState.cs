using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Room visor-colour animation run by <c>HandleVisorPalette</c>: a countdown of
/// palette-handler calls and the visor shade shown next.
/// </summary>
/// <remarks>
/// This is intentionally not a generic color tween. In rooms whose default layer-blending configuration animates
/// the visor, the routine rotates only Samus's visor colour through the last three of the six visor shades. The
/// countdown and the shade are only ever set by this type: the reset installs countdown one at the cycle's first shade,
/// and every expiry reloads the countdown to the frame delay and moves through the cycle's three shades. The countdown
/// therefore holds one through the frame delay between calls and never decrements from zero, which is why the
/// game's word-wide decrement of the countdown and the shade beside it never borrows into the shade.
/// </remarks>
public sealed class SamusVisorPaletteState
{
    /// <summary>Calls left after the reset before the first visor colour is written.</summary>
    private const int ResetTimer = 1;

    /// <summary>
    /// Palette-handler calls left before the next visor colour, one through
    /// <see cref="SamusPaletteDefinitions.Visor.FrameDelay"/>; it reads the frame delay on the call that wrote a colour.
    /// </summary>
    public int Timer { get; private set; } = ResetTimer;

    /// <summary>
    /// The visor shade shown next: one of the steady cycle's three, from
    /// <see cref="SamusPaletteDefinitions.Visor.CycleFirstShade"/> up to the shade count.
    /// </summary>
    public int NextShade { get; private set; } = SamusPaletteDefinitions.Visor.CycleFirstShade;

    /// <summary>
    /// Advances one palette-handler call and optionally shows the next shade as Samus's visor colour.
    /// </summary>
    public SamusVisorPaletteStepResult Update(
        SamusColours colours,
        SamusSpecialPaletteType specialSamusPaletteType,
        LayerBlendingConfiguration layerBlendingDefaultConfig)
    {
        Ensure.NotNull(colours);

        // X-ray handler eight owns this same visor color. The visor handler returns immediately,
        // preserving the countdown and shade so ordinary room animation resumes where it stopped.
        if (specialSamusPaletteType == SamusSpecialPaletteType.Xray)
        {
            return new SamusVisorPaletteStepResult();
        }

        // Every other room resets the animation on every call. Besides disabling the animation,
        // this primes an immediate color write on the first call after entering a backdrop-
        // color-math room: countdown one expires before the shade is consulted.
        if (!layerBlendingDefaultConfig.AnimatesVisor())
        {
            Timer = ResetTimer;
            NextShade = SamusPaletteDefinitions.Visor.CycleFirstShade;
            return new SamusVisorPaletteStepResult();
        }

        Timer = DecrementTimer(Timer);
        if (Timer != 0)
        {
            return new SamusVisorPaletteStepResult();
        }

        // The visor handler reloads the expired countdown to the frame delay and keeps the shade.
        Timer = SamusPaletteDefinitions.Visor.FrameDelay;
        int shade = NextShade;
        colours.ShowVisor(SamusShades.Visor(shade));

        // Only the cycle's three shades belong to the room-backdrop cycle. The shades before
        // them are X-ray widening colors and are selected by the separate X-ray palette handler.
        int nextShade = shade + 1;
        NextShade = nextShade < SamusPaletteDefinitions.Visor.CycleEndShade
            ? nextShade
            : SamusPaletteDefinitions.Visor.CycleFirstShade;

        return new SamusVisorPaletteStepResult();
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
