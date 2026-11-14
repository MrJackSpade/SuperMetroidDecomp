using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room effect's state: the flash clock its setup initializes and its per-frame behaviour steps, and
/// the darkness step the dying Firefleas advance, and the shade it publishes (<see cref="Tint"/>), which a Fireflea
/// room's load restarts from its first flash. The game keeps these for the session;
/// the room FX state owns them for the session, so a room load does not reset them.
/// </summary>
public sealed class FirefleaFxState
{
    /// <summary>
    /// Frames until the next flash phase, held while time is frozen. A step
    /// that reaches zero restarts it; one that starts from zero, before the FX initializes, wraps instead.
    /// </summary>
    public WrappingCountdown Timer { get; private set; }

    /// <summary>
    /// The room's place in its flash cycle, 0 through 11: the shade brightens over the first six phases and dims over
    /// the last six (<see cref="FirefleaShades.FlashingShade"/>).
    /// </summary>
    public int FlashPhase { get; private set; }

    /// <summary>
    /// The darkness step published from the enemies' death count: one step per Fireflea killed, from none through
    /// the sixth step (<see cref="FirefleaFxDefinitions.DarknessLevels"/>).
    /// </summary>
    public int DarknessStep { get; private set; }

    /// <summary>
    /// The shade Fireflea darkness subtracts from the room, grey on the game's fade scale: the flashing shade plus the
    /// darkness step's level, as the last unfrozen step chose it. A frozen step keeps it.
    /// </summary>
    public RgbColor Tint { get; private set; } = Shade(0, 0);

    /// <summary>Seeds the initial flash duration, the cycle's first phase, and the undarkened room, and shows its shade.</summary>
    internal void Initialize()
    {
        Timer = WrappingCountdown.Start(FirefleaFxDefinitions.FlashDuration);
        FlashPhase = 0;
        DarknessStep = 0;
        Tint = Shade(FlashPhase, DarknessStep);
    }

    /// <summary>
    /// Samples the enemies' darkness step, and unless frozen advances the flash clock and chooses the room's
    /// <see cref="Tint"/> from the flashing shade and the darkness step's level.
    /// </summary>
    internal void Step(bool frozen, int darknessStep)
    {
        DarknessStep = darknessStep;
        if (frozen) return;
        CountdownStep step = Timer.Decrement();
        WrappingCountdown timer = step.Remaining;
        int phase = FlashPhase;
        if (step.IsZero)
        {
            timer = WrappingCountdown.Start(FirefleaFxDefinitions.FlashDuration);
            if (darknessStep < FirefleaFxDefinitions.SteadyDarknessStep)
            {
                phase++;
                if (phase >= FirefleaFxDefinitions.FlashCount) phase = 0;
            }
            else phase = FirefleaFxDefinitions.SteadyFlashPhase;
            FlashPhase = phase;
        }
        Timer = timer;

        Tint = Shade(phase, darknessStep);
    }

    /// <summary>
    /// The grey of the flashing shade at flash phase <paramref name="phase"/> deepened by darkness step <paramref name="darknessStep"/>:
    /// the two fade levels add, and their sum never passes the scale's full level.
    /// </summary>
    private static RgbColor Shade(int phase, int darknessStep)
    {
        int level = FirefleaShades.FlashingShade(phase) + FirefleaShades.DarknessShade(darknessStep);
        return new FadeLevelColour(level, level, level).ToColour();
    }
}
