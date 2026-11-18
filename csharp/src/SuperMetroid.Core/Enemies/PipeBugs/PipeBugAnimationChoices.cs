namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Chooses a Brinstar Pipe Bug's animation by its strength, facing and shooting (#1810).</summary>
internal static class PipeBugAnimationChoices
{
    /// <summary>The rising animation of a <paramref name="kind"/> Pipe Bug, the ordinary Zeb or strong Zebbo, facing <paramref name="facing"/>.</summary>
    internal static PipeBugSegment Rising(BrinstarPipeBugKind kind, HorizontalDirection facing) => kind switch
    {
        BrinstarPipeBugKind.Weak => Facing(facing, PipeBugSegment.ZebRisingLeft, PipeBugSegment.ZebRisingRight),
        BrinstarPipeBugKind.Strong => Facing(facing, PipeBugSegment.ZebboRisingLeft, PipeBugSegment.ZebboRisingRight),
        _ => throw new InvalidDataException($"Pipe Bug kind {kind} has no rising animation."),
    };

    /// <summary>The shooting animation of a <paramref name="kind"/> Pipe Bug, the ordinary Zeb or strong Zebbo, facing <paramref name="facing"/>.</summary>
    internal static PipeBugSegment Shooting(BrinstarPipeBugKind kind, HorizontalDirection facing) => kind switch
    {
        BrinstarPipeBugKind.Weak => Facing(facing, PipeBugSegment.ZebShootingLeft, PipeBugSegment.ZebShootingRight),
        BrinstarPipeBugKind.Strong => Facing(facing, PipeBugSegment.ZebboShootingLeft, PipeBugSegment.ZebboShootingRight),
        _ => throw new InvalidDataException($"Pipe Bug kind {kind} has no shooting animation."),
    };

    /// <summary>The animation of the two that faces <paramref name="facing"/>.</summary>
    private static PipeBugSegment Facing(HorizontalDirection facing, PipeBugSegment left, PipeBugSegment right) => facing switch
    {
        HorizontalDirection.Left => left,
        HorizontalDirection.Right => right,
        _ => throw new InvalidDataException($"Pipe Bug facing {facing} is not a horizontal direction."),
    };
}
