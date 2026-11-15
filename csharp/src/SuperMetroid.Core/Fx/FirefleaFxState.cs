using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room effect's state: the flash clock its setup starts and its per-frame behaviour steps, the darkness step
/// the dying Firefleas advance, and the shade it publishes (<see cref="Tint"/>). A Fireflea room's effect creates it as the
/// room loads, from its first flash and an undarkened room, and it lives as long as that room (#2053).
/// </summary>
[RoomLifetime]
public sealed class FirefleaFxState
{
    /// <summary>
    /// Frames until the next flash phase, held while time is frozen: a step that reaches zero starts the next phase and
    /// restarts the count from <see cref="FirefleaFxDefinitions.FlashDuration"/>.
    /// </summary>
    public int FramesUntilNextFlash { get; private set; } = FirefleaFxDefinitions.FlashDuration;

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

    /// <summary>
    /// Samples the enemies' darkness step, and unless frozen advances the flash clock and chooses the room's
    /// <see cref="Tint"/> from the flashing shade and the darkness step's level.
    /// </summary>
    internal void Step(bool frozen, int darknessStep)
    {
        DarknessStep = darknessStep;
        if (frozen) return;
        int phase = FlashPhase;
        if (--FramesUntilNextFlash == 0)
        {
            FramesUntilNextFlash = FirefleaFxDefinitions.FlashDuration;
            if (darknessStep < FirefleaFxDefinitions.SteadyDarknessStep)
            {
                phase++;
                if (phase >= FirefleaFxDefinitions.FlashCount) phase = 0;
            }
            else phase = FirefleaFxDefinitions.SteadyFlashPhase;
            FlashPhase = phase;
        }

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
