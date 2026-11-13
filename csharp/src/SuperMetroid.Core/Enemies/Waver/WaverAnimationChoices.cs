namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Chooses a Waver's animation by its facing and spinning.</summary>
internal static class WaverAnimationChoices
{
    /// <summary>The steady animation facing <paramref name="facing"/>.</summary>
    internal static WaverSegment Steady(HorizontalDirection facing) => facing switch
    {
        HorizontalDirection.Left => WaverSegment.SteadyFacingLeft,
        HorizontalDirection.Right => WaverSegment.SteadyFacingRight,
        _ => throw new InvalidDataException($"Waver facing {facing} is not a horizontal direction."),
    };

    /// <summary>The spin with <paramref name="animation"/>'s facing.</summary>
    internal static WaverSegment Spinning(WaverSegment animation) => Facing(animation) switch
    {
        HorizontalDirection.Left => WaverSegment.SpinningFacingLeft,
        HorizontalDirection.Right => WaverSegment.SpinningFacingRight,
        _ => throw new InvalidDataException($"Waver animation {animation} has no facing."),
    };

    /// <summary>The steady animation with <paramref name="animation"/>'s facing, as a finished spin returns to.</summary>
    internal static WaverSegment SteadyWithSameFacing(WaverSegment animation) => Steady(Facing(animation));

    /// <summary>The steady animation facing away from <paramref name="animation"/>'s facing, as a wall turns Waver.</summary>
    internal static WaverSegment TurnedAround(WaverSegment animation) => Facing(animation) switch
    {
        HorizontalDirection.Left => WaverSegment.SteadyFacingRight,
        HorizontalDirection.Right => WaverSegment.SteadyFacingLeft,
        _ => throw new InvalidDataException($"Waver animation {animation} has no facing."),
    };

    /// <summary>The way <paramref name="animation"/> faces.</summary>
    internal static HorizontalDirection Facing(WaverSegment animation) => animation switch
    {
        WaverSegment.SteadyFacingLeft or WaverSegment.SpinningFacingLeft => HorizontalDirection.Left,
        WaverSegment.SteadyFacingRight or WaverSegment.SpinningFacingRight => HorizontalDirection.Right,
        _ => throw new InvalidDataException($"Waver animation {animation} is not one of its four animations."),
    };
}
