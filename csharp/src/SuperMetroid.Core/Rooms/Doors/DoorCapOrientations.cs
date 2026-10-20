namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Decodes and names the orientation of a door-cap draw list.</summary>
internal static class DoorCapOrientations
{
    /// <summary>The orientations in the order the cap draw-list tables at $84:A6xx..$84:AAxx store them.</summary>
    private static readonly ColoredDoorOrientation[] TableOrder =
        [ColoredDoorOrientation.Left, ColoredDoorOrientation.Right, ColoredDoorOrientation.Up, ColoredDoorOrientation.Down];

    /// <summary>The orientation at a position in a cap draw-list table.</summary>
    internal static ColoredDoorOrientation FromTableIndex(int index) => TableOrder[index];

    /// <summary>The orientation's word in a cap's editable visual identity.</summary>
    internal static string VisualName(ColoredDoorOrientation orientation) => orientation switch
    {
        ColoredDoorOrientation.Left => "left",
        ColoredDoorOrientation.Right => "right",
        ColoredDoorOrientation.Up => "up",
        ColoredDoorOrientation.Down => "down",
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorOrientation)} {orientation}."),
    };
}
