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

    /// <summary>The shooting animation of the bug and facing <paramref name="animation"/> shows.</summary>
    /// <exception cref="InvalidDataException"><paramref name="animation"/> is not a Zeb or Zebbo animation.</exception>
    internal static PipeBugSegment ShootingWithSameFacing(PipeBugSegment animation) => animation switch
    {
        PipeBugSegment.ZebRisingLeft or PipeBugSegment.ZebShootingLeft => PipeBugSegment.ZebShootingLeft,
        PipeBugSegment.ZebRisingRight or PipeBugSegment.ZebShootingRight => PipeBugSegment.ZebShootingRight,
        PipeBugSegment.ZebboRisingLeft or PipeBugSegment.ZebboShootingLeft => PipeBugSegment.ZebboShootingLeft,
        PipeBugSegment.ZebboRisingRight or PipeBugSegment.ZebboShootingRight => PipeBugSegment.ZebboShootingRight,
        PipeBugSegment.GametRisingLeft or PipeBugSegment.GametFlyingLeft or PipeBugSegment.GametRisingRight or
            PipeBugSegment.GametFlyingRight or PipeBugSegment.GeegaFlyingLeft or PipeBugSegment.GeegaArcingLeft or
            PipeBugSegment.GeegaFlyingRight or PipeBugSegment.GeegaArcingRight or _ => throw new InvalidDataException($"Pipe Bug animation {animation} is not a Zeb or Zebbo animation."),
    };
}
