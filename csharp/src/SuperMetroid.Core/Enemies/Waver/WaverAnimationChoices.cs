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

    /// <summary>The spin facing <paramref name="facing"/>.</summary>
    internal static WaverSegment Spinning(HorizontalDirection facing) => facing switch
    {
        HorizontalDirection.Left => WaverSegment.SpinningFacingLeft,
        HorizontalDirection.Right => WaverSegment.SpinningFacingRight,
        _ => throw new InvalidDataException($"Waver facing {facing} is not a horizontal direction."),
    };
}
