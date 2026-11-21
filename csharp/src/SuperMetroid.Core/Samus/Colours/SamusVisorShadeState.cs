using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The room's animation of Samus's visor: the steady visor cycle played through the shared animation player (#2387).
/// </summary>
/// <remarks>
/// This is intentionally not a generic color tween. In rooms that animate the visor (<see cref="RoomVisorAnimation"/>),
/// the animation rotates only Samus's visor through the steady visor cycle's shades (#2285), each for its hold. Every
/// other room starts the cycle again on every update, so the first update in an animating room shows its first shade at
/// once.
/// </remarks>
public sealed class SamusVisorShadeState
{
    /// <summary>The steady visor cycle as the room plays it: its first shade shows on the update after it starts.</summary>
    private AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> Cycle { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.Of(SamusGlow.VisorCycle));

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
        // preserving where the cycle stands so ordinary room animation resumes where it stopped.
        if (specialColouringKind == SamusSpecialColouringKind.Xray)
        {
            return;
        }

        // Every other room starts the cycle again on every update. Besides disabling the animation,
        // this changes the visor's colour at once on the first update after entering a backdrop-
        // blending room: the cycle's first shade shows on its first update.
        if (roomVisorAnimation != RoomVisorAnimation.Animated)
        {
            SamusGlowPlay.Restart(Cycle, SamusGlowSegment.Of(SamusGlow.VisorCycle));
            return;
        }

        // The room shows only the steady cycle; the X-ray widening glow is X-ray's own suit colouring.
        if (Cycle.StepToNewFrame() is { } shade)
            colours.ShowVisor(shade);
    }
}
