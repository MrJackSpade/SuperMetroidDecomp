namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Chooses a Brinstar Pipe Bug's animation by its strength, facing and shooting (#1810).</summary>
internal static class PipeBugAnimationChoices
{
    /// <summary>The rising animation of a normal Zeb or strong Zebbo facing <paramref name="facing"/>.</summary>
    internal static PipeBugSegment Rising(bool strong, HorizontalDirection facing) => facing switch
    {
        HorizontalDirection.Left => strong ? PipeBugSegment.ZebboRisingLeft : PipeBugSegment.ZebRisingLeft,
        HorizontalDirection.Right => strong ? PipeBugSegment.ZebboRisingRight : PipeBugSegment.ZebRisingRight,
        _ => throw new InvalidDataException($"Pipe Bug facing {facing} is not a horizontal direction."),
    };

    /// <summary>The shooting animation of a normal Zeb or strong Zebbo facing <paramref name="facing"/>.</summary>
    internal static PipeBugSegment Shooting(bool strong, HorizontalDirection facing) => facing switch
    {
        HorizontalDirection.Left => strong ? PipeBugSegment.ZebboShootingLeft : PipeBugSegment.ZebShootingLeft,
        HorizontalDirection.Right => strong ? PipeBugSegment.ZebboShootingRight : PipeBugSegment.ZebShootingRight,
        _ => throw new InvalidDataException($"Pipe Bug facing {facing} is not a horizontal direction."),
    };
}
