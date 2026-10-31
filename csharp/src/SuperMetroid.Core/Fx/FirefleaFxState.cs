using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx;

/// <summary>
/// FX $24's state: the flash clock that <c>$88:B07C</c> initializes and its pre-instruction <c>$88:B0BC</c> steps, and
/// the darkness level the dying Firefleas advance. The game keeps these for the session (<c>$7E:1778</c>, <c>$177A</c>,
/// <c>$177E</c>); the room FX state owns them for the session, so a room load does not reset them.
/// </summary>
public sealed class FirefleaFxState
{
    /// <summary>
    /// <c>FirefleaFlashing_Timer</c>: frames until the next flashing-index step, held while time is frozen. A step
    /// that reaches zero restarts it; one that starts from zero, before the FX initializes, wraps instead.
    /// </summary>
    public WrappingCountdown Timer { get; private set; }

    /// <summary><c>FirefleaFlashing_Index</c>: the current flashing-table element index, 0 through 11.</summary>
    public int Index { get; private set; }

    /// <summary><c>FirefleaFlashing_DarknessLevel</c>: the level published from the enemy-owned death counter, an even darkness-table byte offset 0 through 12.</summary>
    public int Darkness { get; private set; }

    /// <summary>Seeds the initial flash duration, first shade index, and undarkened room state.</summary>
    internal void Initialize()
    {
        Timer = WrappingCountdown.Start(FirefleaFxDefinitions.FlashDuration);
        Index = 0;
        Darkness = 0;
    }

    /// <summary>
    /// Samples enemy-owned darkness, advances the flash clock unless frozen, and writes the shared fixed colour using
    /// native 16-bit shade addition, including the seventh offset value.
    /// </summary>
    internal void Step(RoomFixedColor fixedColor, bool frozen, int darkness)
    {
        Ensure.NotNull(fixedColor);
        Darkness = darkness;
        if (frozen) return;
        CountdownStep step = Timer.Decrement();
        WrappingCountdown timer = step.Remaining;
        int index = Index;
        if (step.IsZero)
        {
            timer = WrappingCountdown.Start(FirefleaFxDefinitions.FlashDuration);
            if (WordWrap.Signed(darkness - FirefleaFxDefinitions.SteadyDarknessThreshold) < 0)
            {
                index++;
                if (index >= FirefleaFxDefinitions.FlashCount) index = 0;
            }
            else index = FirefleaFxDefinitions.SteadyFlashIndex;
            Index = index;
        }
        Timer = timer;

        int shade = WordWrap.LowByte(
            FirefleaShades.FlashingShade(index) +
            FirefleaShades.DarknessShade(darkness));
        // Each of the cartridge's three writes carries the shade as its five-bit intensity. Past the darkness ramp the
        // shade's upper bits only select channels the other writes set to the same value.
        int intensity = shade & RgbColor.MaxChannel;
        fixedColor.Color = new RgbColor(intensity, intensity, intensity);
    }
}
