namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Chooses a Brinstar Pipe Bug's animation by its strength, facing and shooting (#1810).</summary>
internal static class PipeBugAnimationChoices
{
    /// <summary>The rising animation of a <paramref name="kind"/> Pipe Bug, the ordinary Zeb or strong Zebbo, facing <paramref name="facing"/>.</summary>
    internal static PipeBugSegment Rising(BrinstarPipeBugKind kind, HorizontalDirection facing) => kind == BrinstarPipeBugKind.Weak
        ? Facing(facing, PipeBugSegment.ZebRisingLeft, PipeBugSegment.ZebRisingRight)
        // The remaining member, BrinstarPipeBugKind.Strong.
        : Facing(facing, PipeBugSegment.ZebboRisingLeft, PipeBugSegment.ZebboRisingRight);

    /// <summary>The shooting animation of a <paramref name="kind"/> Pipe Bug, the ordinary Zeb or strong Zebbo, facing <paramref name="facing"/>.</summary>
    internal static PipeBugSegment Shooting(BrinstarPipeBugKind kind, HorizontalDirection facing) => kind == BrinstarPipeBugKind.Weak
        ? Facing(facing, PipeBugSegment.ZebShootingLeft, PipeBugSegment.ZebShootingRight)
        // The remaining member, BrinstarPipeBugKind.Strong.
        : Facing(facing, PipeBugSegment.ZebboShootingLeft, PipeBugSegment.ZebboShootingRight);

    /// <summary>The animation of the two that faces <paramref name="facing"/>.</summary>
    private static PipeBugSegment Facing(HorizontalDirection facing, PipeBugSegment left, PipeBugSegment right) =>
        // The remaining member, HorizontalDirection.Right, takes the right-facing animation.
        facing == HorizontalDirection.Left ? left : right;
}
