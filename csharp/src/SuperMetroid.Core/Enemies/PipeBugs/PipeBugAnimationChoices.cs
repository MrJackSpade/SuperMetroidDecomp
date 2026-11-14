namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Chooses a Brinstar Pipe Bug's animation by its strength, facing and shooting (#1810).</summary>
internal static class PipeBugAnimationChoices
{
    /// <summary>The rising animation of a normal Zeb or strong Zebbo facing <paramref name="facing"/>.</summary>
    internal static PipeBugSegment Rising(bool strong, HorizontalDirection facing) => (strong, facing) switch
    {
        (false, HorizontalDirection.Left) => PipeBugSegment.ZebRisingLeft,
        (false, HorizontalDirection.Right) => PipeBugSegment.ZebRisingRight,
        (true, HorizontalDirection.Left) => PipeBugSegment.ZebboRisingLeft,
        (true, HorizontalDirection.Right) => PipeBugSegment.ZebboRisingRight,
        _ => throw new InvalidDataException($"Pipe Bug facing {facing} is not a horizontal direction."),
    };

    /// <summary>The shooting animation of the bug and facing <paramref name="animation"/> shows.</summary>
    /// <exception cref="InvalidDataException"><paramref name="animation"/> is not a Zeb or Zebbo animation.</exception>
    internal static PipeBugSegment ShootingWithSameFacing(PipeBugSegment animation) => animation switch
    {
        PipeBugSegment.ZebRisingLeft or PipeBugSegment.ZebShootingLeft => PipeBugSegment.ZebShootingLeft,
        PipeBugSegment.ZebRisingRight or PipeBugSegment.ZebShootingRight => PipeBugSegment.ZebShootingRight,
        PipeBugSegment.ZebboRisingLeft or PipeBugSegment.ZebboShootingLeft => PipeBugSegment.ZebboShootingLeft,
        PipeBugSegment.ZebboRisingRight or PipeBugSegment.ZebboShootingRight => PipeBugSegment.ZebboShootingRight,
        _ => throw new InvalidDataException($"Pipe Bug animation {animation} is not a Zeb or Zebbo animation."),
    };
}
