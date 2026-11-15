namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's death (#1933): his dying fades, his death explosions and his body's pixelation as it dissolves. Each stage
/// sets what it reads when it starts.
/// </summary>
public sealed class PhantoonDeathState
{
    /// <summary>The dying fades in and out he has completed.</summary>
    public int FadeCycles { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The next death explosion in the explosion schedule.</summary>
    public int NextExplosion { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The passes through the repeating part of the explosion schedule he has completed.</summary>
    public int ExplosionPasses { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>His body's pixelation block size in screen pixels while the pixelation grows.</summary>
    public int PixelationSize
    {
        get;
        internal set => field = Ensure.BetweenInclusive(PhantoonDeathDefinitions.InitialPixelationSize, PhantoonDeathDefinitions.FinalPixelationSize, value);
    } = PhantoonDeathDefinitions.InitialPixelationSize;

    /// <summary>Whether his body's pixelation has finished growing and the fade-out runs.</summary>
    public bool PixelationFinished { get; internal set; }
}
