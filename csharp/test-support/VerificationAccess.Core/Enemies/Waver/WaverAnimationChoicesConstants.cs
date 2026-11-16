namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Cartridge values and helpers of <see cref="WaverAnimationChoices"/> that only verification reads.</summary>
internal static class WaverAnimationChoicesConstants
{
    /// <summary>The way <paramref name="animation"/> faces.</summary>
    internal static HorizontalDirection Facing(WaverSegment animation) => animation switch
    {
        WaverSegment.SteadyFacingLeft or WaverSegment.SpinningFacingLeft => HorizontalDirection.Left,
        WaverSegment.SteadyFacingRight or WaverSegment.SpinningFacingRight => HorizontalDirection.Right,
        _ => throw new InvalidDataException($"Waver animation {animation} is not one of its four animations."),
    };
}
