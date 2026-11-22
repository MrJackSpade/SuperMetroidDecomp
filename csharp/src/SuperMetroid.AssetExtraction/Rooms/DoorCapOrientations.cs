using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Decodes and names the orientation of a door-cap draw list.</summary>
internal static partial class DoorCapOrientations
{
    /// <summary>The orientations in the order the door caps' draw tables list them, which the caps' editable visuals follow.</summary>
    internal static IReadOnlyList<ColoredDoorOrientation> InTableOrder { get; } =
        [ColoredDoorOrientation.Left, ColoredDoorOrientation.Right, ColoredDoorOrientation.Up, ColoredDoorOrientation.Down];

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
