namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Chooses a Brinstar Pipe Bug's program by its strength, facing and shooting.</summary>
internal static class PipeBugAnimationChoices
{
    /// <summary>The rising or shooting program of a normal or strong Pipe Bug with <paramref name="selector"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> is not one of the four animations.</exception>
    internal static PipeBugSegment BrinstarAnimation(bool strong, PipeBugAnimationSelector selector) =>
        PipeBugDefinitions.BrinstarAnimations.TryGetValue(new PipeBugAnimationKey(strong, selector), out PipeBugSegment program)
            ? program
            : throw new InvalidDataException(
                $"Pipe Bug animation selector {selector} is not one of its four animations.");

    /// <summary>The rising animation facing <paramref name="facing"/>.</summary>
    internal static PipeBugAnimationSelector Rising(HorizontalDirection facing) => facing switch
    {
        HorizontalDirection.Left => PipeBugAnimationSelector.RisingFacingLeft,
        HorizontalDirection.Right => PipeBugAnimationSelector.RisingFacingRight,
        _ => throw new InvalidDataException($"Pipe Bug facing {facing} is not a horizontal direction."),
    };

    /// <summary>The shooting animation with <paramref name="selector"/>'s facing.</summary>
    internal static PipeBugAnimationSelector ShootingWithSameFacing(PipeBugAnimationSelector selector) => selector switch
    {
        PipeBugAnimationSelector.RisingFacingLeft or PipeBugAnimationSelector.ShootingFacingLeft =>
            PipeBugAnimationSelector.ShootingFacingLeft,
        PipeBugAnimationSelector.RisingFacingRight or PipeBugAnimationSelector.ShootingFacingRight =>
            PipeBugAnimationSelector.ShootingFacingRight,
        _ => throw new InvalidDataException($"Pipe Bug animation selector {selector} is not one of its four animations."),
    };
}
