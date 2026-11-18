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

    /// <summary>Whether <paramref name="animation"/> is one of the two spins rather than a steady pose.</summary>
    internal static bool IsSpin(WaverSegment animation) => animation switch
    {
        WaverSegment.SpinningFacingLeft or WaverSegment.SpinningFacingRight => true,
        WaverSegment.SteadyFacingLeft or WaverSegment.SteadyFacingRight => false,
        _ => throw new InvalidDataException($"Waver animation {animation} is not one of its four animations."),
    };

    /// <summary>The spin facing <paramref name="facing"/>.</summary>
    internal static WaverSegment Spinning(HorizontalDirection facing) => facing switch
    {
        HorizontalDirection.Left => WaverSegment.SpinningFacingLeft,
        HorizontalDirection.Right => WaverSegment.SpinningFacingRight,
        _ => throw new InvalidDataException($"Waver facing {facing} is not a horizontal direction."),
    };
}
