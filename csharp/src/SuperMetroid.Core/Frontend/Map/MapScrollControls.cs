using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Application-owned input order from the mixed visual/control arrow records.</summary>
internal static class MapScrollControls
{
    /// <summary>The four arrow records, processed left/right/up/down.</summary>
    public const int DirectionCount = 4;

    /// <summary>The direction of arrow record <paramref name="index"/>, in left/right/up/down order.</summary>
    public static MapScrollDirection ArrowAt(int index) => index switch
    {
        0 => MapScrollDirection.Left,
        1 => MapScrollDirection.Right,
        2 => MapScrollDirection.Up,
        3 => MapScrollDirection.Down,
        _ => throw new InvalidOperationException($"Map arrow record {index} has no {nameof(MapScrollDirection)}."),
    };

    /// <summary>The eight-pixel map scroll applied by one accepted scroll pulse in <paramref name="direction"/>.</summary>
    public static Offset16<MapPixels> StepOffset(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => new(-MapScrollDefinitions.StepPixels, 0),
        MapScrollDirection.Right => new(MapScrollDefinitions.StepPixels, 0),
        MapScrollDirection.Up => new(0, -MapScrollDefinitions.StepPixels),
        MapScrollDirection.Down => new(0, MapScrollDefinitions.StepPixels),
        MapScrollDirection.None => throw new ArgumentOutOfRangeException(nameof(direction), direction, "No map scroll step is active."),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "No map scroll step is active."),
    };

    /// <summary>
    /// Each arrow record associates the arrow's direction with its matching
    /// controller direction. Drawing position and animation fields are separate.
    /// </summary>
    public static ControllerButton ButtonFor(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => ControllerButton.Left,
        MapScrollDirection.Right => ControllerButton.Right,
        MapScrollDirection.Up => ControllerButton.Up,
        MapScrollDirection.Down => ControllerButton.Down,
        MapScrollDirection.None => throw new ArgumentOutOfRangeException(nameof(direction)),
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
}
