namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's death (#1933): his dying fades, his death explosions and his body's pixelation as it dissolves. Each stage
/// sets what it reads when it starts.
/// </summary>
public sealed partial class PhantoonDeathState
{
    /// <summary>The dying fades in and out he has completed.</summary>
    public int FadeCycles { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The next death explosion in the explosion schedule (#2315).</summary>
    public ListStep<PhantoonDeathExplosionDefinition> NextExplosion { get; internal set; } = PhantoonDeathExplosions.First;

    /// <summary>The passes through the repeating part of the explosion schedule he has completed.</summary>
    public int ExplosionPasses { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>
    /// His body's pixelation block size in screen pixels, which the display latches: one (no quantization) until the dying
    /// fade grows it at its growth cadence up to its largest (<see cref="PhantoonDeathDefinitions"/>), and one again once his body is cleared.
    /// </summary>
    public int PixelationSize
    {
        get;
        internal set => field = Ensure.BetweenInclusive(PhantoonDeathDefinitions.InitialPixelationSize, PhantoonDeathDefinitions.FinalPixelationSize, value);
    } = PhantoonDeathDefinitions.InitialPixelationSize;

    /// <summary>The stage of his fade-out: his pixelation growing, or done and his fade running (#2560).</summary>
    public PhantoonDeathStage Stage { get; internal set; }
}
