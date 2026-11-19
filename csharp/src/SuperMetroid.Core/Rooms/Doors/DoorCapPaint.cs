using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The paint a door cap's closed and opening images show (#2546): the blue cap's, the grey cap's, or the paint one
/// coloured cap's colour names (<see cref="ColoredDoorDefinitions.Colors"/>). Each paint draws the cap facing a way and
/// showing a look.
/// </summary>
internal abstract class DoorCapPaint
{
    /// <summary>Only the paints below draw a cap.</summary>
    private DoorCapPaint()
    {
    }

    /// <summary>The blue cap's paint, which any shot opens.</summary>
    internal static DoorCapPaint Blue { get; } = new BluePaint();

    /// <summary>The grey cap's paint, which a condition locks.</summary>
    internal static DoorCapPaint Grey { get; } = new GreyPaint();

    /// <summary>The paint of a cap coloured <paramref name="color"/>, which that colour's definition holds.</summary>
    internal static DoorCapPaint OfColor(ColoredDoorColor color) => new ColoredPaint(color);

    /// <summary>The cap in this paint facing <paramref name="orientation"/> showing <paramref name="look"/>.</summary>
    internal abstract RoomObjectDraw Of(ColoredDoorOrientation orientation, DoorCapLook look);

    /// <summary>The blue cap's paint, pictured by the blue-door frames.</summary>
    private sealed class BluePaint : DoorCapPaint
    {
        /// <inheritdoc/>
        internal override RoomObjectDraw Of(ColoredDoorOrientation orientation, DoorCapLook look) =>
            BlueDoorCaps.Family.Of(new BlueDoorCaps.Draw(orientation, look));
    }

    /// <summary>The grey cap's paint, pictured by the grey-door frames.</summary>
    private sealed class GreyPaint : DoorCapPaint
    {
        /// <inheritdoc/>
        internal override RoomObjectDraw Of(ColoredDoorOrientation orientation, DoorCapLook look) =>
            GreyDoorCaps.Family.Of(new GreyDoorCaps.Draw(orientation, look));
    }

    /// <summary>A coloured cap's paint, pictured by the coloured-door frames of <paramref name="color"/>.</summary>
    /// <param name="color">The cap's colour.</param>
    private sealed class ColoredPaint(ColoredDoorColor color) : DoorCapPaint
    {
        /// <inheritdoc/>
        internal override RoomObjectDraw Of(ColoredDoorOrientation orientation, DoorCapLook look) =>
            ColoredDoorCaps.Family.Of(new ColoredDoorCaps.Draw(color, orientation, look));
    }
}
