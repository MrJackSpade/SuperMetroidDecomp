namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The acid motion Lower Norfair Ridley drives (#1413): it rises to the battle height when his reveal completes and
/// drains when his death roar ends.
/// </summary>
internal static partial class RidleyLiquidDefinitions
{
    /// <summary>The acid's battle height when Ridley's reveal completes, in world pixels.</summary>
    public const int BattleHeight = 440;
    /// <summary>The acid's rising speed: three eighths of a pixel per frame upward.</summary>
    public static SignedEightEight RiseVelocity { get; } = SignedEightEight.FromPixelFraction(-3, 8);
    /// <summary>The frames before the acid starts rising.</summary>
    public const int RiseDelay = 32;
    /// <summary>The acid's drained height when Ridley's death roar ends, in world pixels.</summary>
    public const int DrainedHeight = 528;
    /// <summary>The acid's draining speed: a quarter of a pixel per frame downward.</summary>
    public static SignedEightEight DrainVelocity { get; } = SignedEightEight.FromPixelFraction(1, 4);
    /// <summary>The frames before the acid starts draining.</summary>
    public const int DrainDelay = 1;
}
