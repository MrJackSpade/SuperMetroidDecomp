namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Chooses a Waver's animation by its facing and spinning.</summary>
internal static class WaverAnimationChoices
{
    /// <summary>The steady of each facing.</summary>
    private static IdentityTable<HorizontalDirection, WaverSegment> SteadyByFacing => field ??= new(new Dictionary<HorizontalDirection, WaverSegment>
    {
        [HorizontalDirection.Left] = WaverSegment.SteadyFacingLeft,
        [HorizontalDirection.Right] = WaverSegment.SteadyFacingRight,
    });

    /// <summary>The steady animation facing <paramref name="facing"/>.</summary>
    internal static WaverSegment Steady(HorizontalDirection facing) => SteadyByFacing[facing];

    /// <summary>The is spin of each animation.</summary>
    private static IdentityTable<WaverSegment, bool> IsSpinByAnimation => field ??= new(new Dictionary<WaverSegment, bool>
    {
        [WaverSegment.SpinningFacingLeft] = true,
        [WaverSegment.SpinningFacingRight] = true,
        [WaverSegment.SteadyFacingLeft] = false,
        [WaverSegment.SteadyFacingRight] = false,
    });

    /// <summary>Whether <paramref name="animation"/> is one of the two spins rather than a steady pose.</summary>
    internal static bool IsSpin(WaverSegment animation) => IsSpinByAnimation[animation];

    /// <summary>The spinning of each facing.</summary>
    private static IdentityTable<HorizontalDirection, WaverSegment> SpinningByFacing => field ??= new(new Dictionary<HorizontalDirection, WaverSegment>
    {
        [HorizontalDirection.Left] = WaverSegment.SpinningFacingLeft,
        [HorizontalDirection.Right] = WaverSegment.SpinningFacingRight,
    });

    /// <summary>The spin facing <paramref name="facing"/>.</summary>
    internal static WaverSegment Spinning(HorizontalDirection facing) => SpinningByFacing[facing];
}
