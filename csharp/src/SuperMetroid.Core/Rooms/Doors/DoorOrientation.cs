namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How a door is passed: the direction Samus travels through it and what closes behind her on arrival. The import
/// pipeline decodes the cartridge's orientation byte into these two fields.
/// </summary>
public readonly record struct DoorOrientation
{
    /// <summary>Creates an orientation from its fields.</summary>
    public DoorOrientation(DoorDirection direction, DoorClosingBehavior closing)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined door direction.");
        if (!Enum.IsDefined(closing))
            throw new ArgumentOutOfRangeException(nameof(closing), closing, "Undefined door closing behavior.");
        Direction = direction;
        Closing = closing;
    }

    /// <summary>Direction of travel through the door.</summary>
    public DoorDirection Direction { get; }

    /// <summary>What the arrival spawns behind Samus.</summary>
    public DoorClosingBehavior Closing { get; }

    /// <summary>True for <see cref="DoorDirection.Down"/> and <see cref="DoorDirection.Up"/>.</summary>
    public bool IsVertical => Direction.IsVertical();
}
