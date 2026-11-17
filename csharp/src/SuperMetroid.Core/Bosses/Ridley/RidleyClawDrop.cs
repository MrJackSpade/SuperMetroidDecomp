namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How far below Ridley's body his claw hangs for each reach of his legs (#2335).</summary>
/// <param name="Standing">The drop while he stands.</param>
/// <param name="LegsHalfExtended">The drop with his legs half extended.</param>
/// <param name="LegsExtended">The drop with his legs extended.</param>
internal readonly record struct RidleyClawDrop(int Standing, int LegsHalfExtended, int LegsExtended)
{
    /// <summary>The drop for <paramref name="height"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="height"/> is not one of the three claw heights.</exception>
    internal int For(RidleyClawHeight height) => height switch
    {
        RidleyClawHeight.Standing => Standing,
        RidleyClawHeight.LegsHalfExtended => LegsHalfExtended,
        RidleyClawHeight.LegsExtended => LegsExtended,
        _ => throw new ArgumentOutOfRangeException(nameof(height), height, "Ridley has three claw heights."),
    };
}
