namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam charge state: the flare counter $0CD0, the previous charge $0DC2, the charged-shot glow timer $0B18, the charge palette index $0B62 and the three flare components' frames and timers.</summary>
internal sealed class BeamChargeState
{

    /// <summary>WRAM <c>$0CD0</c>, 0 through 65535; 60 frames arms a charged shot, 120 is the SBA clamp.</summary>
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
    public NativeCountdown ChargedShotGlowTimer { get; internal set; }

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

    /// <summary>Current animation frame for each of the three flare components.</summary>
    internal readonly ushort[] _flareFrames = new ushort[3];
    /// <summary>Countdown to each flare component's next authored frame.</summary>
    internal readonly NativeCountdown[] _flareTimers = new NativeCountdown[3];
}
