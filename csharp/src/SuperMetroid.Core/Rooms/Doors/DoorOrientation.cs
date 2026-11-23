namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How a door is passed: the direction Samus travels through it and what closes behind her on arrival. The import
/// pipeline decodes the door data's orientation into these two fields.
/// </summary>
public readonly partial record struct DoorOrientation
{
    /// <summary>Creates an orientation from its fields.</summary>
    public DoorOrientation(DoorDirection direction, DoorClosingBehavior closing)
    {
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
