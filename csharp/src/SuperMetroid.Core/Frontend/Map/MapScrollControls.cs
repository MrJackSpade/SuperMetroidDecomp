using System.Collections.Immutable;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>The map scroll arrows' directions, the button each answers to and the step each takes.</summary>
internal static class MapScrollControls
{
    /// <summary>
    /// The four scroll directions in the order held arrows are tested, the first that may scroll taking the step, and the
    /// order the arrows are drawn in: left, right, up, down.
    /// </summary>
    public static ImmutableArray<MapScrollDirection> Directions { get; } =
        [MapScrollDirection.Left, MapScrollDirection.Right, MapScrollDirection.Up, MapScrollDirection.Down];

    /// <summary>The map scroll one accepted scroll pulse applies in <paramref name="direction"/>.</summary>
    public static Offset16<MapPixels> StepOffset(MapScrollDirection direction) => StepOffsets[direction];

    /// <summary>The step each of the four directions scrolls.</summary>
    private static IdentityTable<MapScrollDirection, Offset16<MapPixels>> StepOffsets { get; } = new(new Dictionary<MapScrollDirection, Offset16<MapPixels>>
    {
        [MapScrollDirection.Left] = new(-MapScrollDefinitions.StepPixels, 0),
        [MapScrollDirection.Right] = new(MapScrollDefinitions.StepPixels, 0),
        [MapScrollDirection.Up] = new(0, -MapScrollDefinitions.StepPixels),
        [MapScrollDirection.Down] = new(0, MapScrollDefinitions.StepPixels),
    });

    /// <summary>The controller direction that scrolls the map in <paramref name="direction"/>.</summary>
    public static ControllerButton ButtonFor(MapScrollDirection direction) => Buttons[direction];

    /// <summary>The button each of the four directions answers to.</summary>
    private static IdentityTable<MapScrollDirection, ControllerButton> Buttons { get; } = new(new Dictionary<MapScrollDirection, ControllerButton>
    {
        [MapScrollDirection.Left] = ControllerButton.Left,
        [MapScrollDirection.Right] = ControllerButton.Right,
        [MapScrollDirection.Up] = ControllerButton.Up,
        [MapScrollDirection.Down] = ControllerButton.Down,
    });
}
