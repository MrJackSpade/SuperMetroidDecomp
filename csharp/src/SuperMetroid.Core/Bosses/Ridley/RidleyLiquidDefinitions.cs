namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The acid motion Lower Norfair Ridley commands (#1413): it rises to the battle height when his reveal completes and
/// drains when his death roar ends.
/// </summary>
internal static class RidleyLiquidDefinitions
{
    /// <summary>The acid's battle height when Ridley's reveal completes, in world pixels.</summary>
    public const ushort BattleHeight = 440;
    /// <summary>The acid's rising speed: 96/256 of a pixel per frame upward.</summary>
    public static SignedEightEight RiseVelocity { get; } = SignedEightEight.FromSignedRaw(-96);
    /// <summary>The frames before the acid starts rising.</summary>
    public const ushort RiseDelay = 32;
    /// <summary>The acid's drained height when Ridley's death roar ends, in world pixels.</summary>
    public const ushort DrainedHeight = 528;
    /// <summary>The acid's draining speed: 64/256 of a pixel per frame downward.</summary>
    public static SignedEightEight DrainVelocity { get; } = SignedEightEight.FromSignedRaw(64);
    /// <summary>The frames before the acid starts draining.</summary>
    public const ushort DrainDelay = 1;
}
