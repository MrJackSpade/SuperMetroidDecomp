using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam charge state: the flare counter $0CD0, the previous charge $0DC2, the charged-shot glow timer $0B18, the charge palette index $0B62 and the three flare components' frames and timers.</summary>
internal sealed class BeamChargeState
{

    /// <summary>WRAM <c>$0CD0</c>, 0 through 65535; 60 frames arms a charged shot, 120 is the SBA clamp.</summary>
    [IntegerQuantity("updates the charge flare has shown")]
    public int FlareCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>WRAM <c>$0DC2</c>, 0 through 65535; sampled before the current charge-input update.</summary>
    public int PreviousBeamChargeCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// WRAM <c>$0B18</c>. Ordinary charged shots use values four through zero for three
    /// white frames and a fourth-call suit restore. Hyper Beam uses `$8014` through `$8000`
    /// for ten ROM palettes held two calls each before the same restore path.
    /// </summary>
    public WrappingCountdown ChargedShotGlowTimer { get; internal set; }

    /// <summary>
    /// WRAM <c>$0B62</c>, a byte offset into the active six-word charge-palette list.
    /// Native values are exactly 0, 2, 4, 6, 8, and 10; the sixth call wraps to zero. An imported value is 0 through
    /// 65535.
    /// </summary>
    public int SamusChargePaletteIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The most recent `$91:D743` charged-shot palette sub-handler result, which survives room loads.</summary>
    internal SamusBeamChargePaletteStepResult LastBeamChargePaletteStep { get; set; }

    /// <summary>The nested `$91:D83F` visor palette result, which survives room loads.</summary>
    internal SamusVisorPaletteStepResult LastVisorPaletteStep { get; set; }

    /// <summary>Current animation frame for each of the three flare components.</summary>
    internal readonly ushort[] FlareFrames = new ushort[3];
    /// <summary>Countdown to each flare component's next authored frame.</summary>
    internal readonly WrappingCountdown[] FlareTimers = new WrappingCountdown[3];

    /// <summary>The current animation frame of flare <paramref name="component"/>.</summary>
    internal ushort FlareFrame(int component) => FlareFrames[component];

    /// <summary>Sets flare <paramref name="component"/>'s animation frame.</summary>
    internal void SetFlareFrame(int component, ushort frame) => FlareFrames[component] = frame;

    /// <summary>Restarts flare <paramref name="component"/>'s countdown to its next frame.</summary>
    internal void StartFlareTimer(int component, int frames) => FlareTimers[component] = WrappingCountdown.Start(frames);

    /// <summary>Counts one call of flare <paramref name="component"/>'s countdown, keeping what remains.</summary>
    internal CountdownStep DecrementFlareTimer(int component)
    {
        CountdownStep step = FlareTimers[component].Decrement();
        FlareTimers[component] = step.Remaining;
        return step;
    }

    /// <summary>The ordinary charge flare's first call: every frame at zero, the three countdowns at 3, 5 and 4.</summary>
    internal void StartChargeFlare()
    {
        Array.Clear(FlareFrames);
        StartFlareTimer(0, 3);
        StartFlareTimer(1, 5);
        StartFlareTimer(2, 4);
    }

    /// <summary>Hyper Beam's literal stores at <c>$90:BD29-$BD52</c>: frames 29, 5 and 5, each countdown at 3.</summary>
    internal void StartHyperBeamFlare()
    {
        FlareFrames[0] = 29;
        FlareFrames[1] = 5;
        FlareFrames[2] = 5;
        StartFlareTimer(0, 3);
        StartFlareTimer(1, 3);
        StartFlareTimer(2, 3);
    }

    /// <summary>Clears every flare frame and countdown.</summary>
    internal void ClearFlare()
    {
        Array.Clear(FlareFrames);
        Array.Clear(FlareTimers);
    }

    /// <summary>Projectile_Func7_Shinespark's charge teardown when spark windup begins: the flare counter and every flare component clear.</summary>
    internal void CancelForShinespark()
    {
        FlareCounter = 0;
        ClearFlare();
    }
}
