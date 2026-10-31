namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Chooses a Waver's animation by its facing and spinning.</summary>
internal static class WaverPrograms
{
    /// <summary>The steady animation facing <paramref name="facing"/>.</summary>
    internal static WaverProgram Steady(HorizontalDirection facing) => facing switch
    {
        HorizontalDirection.Left => WaverProgram.SteadyFacingLeft,
        HorizontalDirection.Right => WaverProgram.SteadyFacingRight,
        _ => throw new InvalidDataException($"Waver facing {facing} is not a horizontal direction."),
    };

    /// <summary>The spin with <paramref name="animation"/>'s facing.</summary>
    internal static WaverProgram Spinning(WaverProgram animation) => Facing(animation) switch
    {
        HorizontalDirection.Left => WaverProgram.SpinningFacingLeft,
        HorizontalDirection.Right => WaverProgram.SpinningFacingRight,
        _ => throw new InvalidDataException($"Waver animation {animation} has no facing."),
    };

    /// <summary>The steady animation with <paramref name="animation"/>'s facing, as a finished spin returns to.</summary>
    internal static WaverProgram SteadyWithSameFacing(WaverProgram animation) => Steady(Facing(animation));

    /// <summary>The steady animation facing away from <paramref name="animation"/>'s facing, as a wall turns Waver.</summary>
    internal static WaverProgram TurnedAround(WaverProgram animation) => Facing(animation) switch
    {
        HorizontalDirection.Left => WaverProgram.SteadyFacingRight,
        HorizontalDirection.Right => WaverProgram.SteadyFacingLeft,
        _ => throw new InvalidDataException($"Waver animation {animation} has no facing."),
    };

    /// <summary>The way <paramref name="animation"/> faces.</summary>
    internal static HorizontalDirection Facing(WaverProgram animation) => animation switch
    {
        WaverProgram.SteadyFacingLeft or WaverProgram.SpinningFacingLeft => HorizontalDirection.Left,
        WaverProgram.SteadyFacingRight or WaverProgram.SpinningFacingRight => HorizontalDirection.Right,
        _ => throw new InvalidDataException($"Waver animation {animation} is not one of its four animations."),
    };
}
