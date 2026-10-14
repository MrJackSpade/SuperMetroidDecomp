namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The door header's orientation byte (<c>$83</c> record offset 3) decoded into its proven
/// fields. <c>Door_Closing_PLMs</c> defines exactly twelve values, $00-$0B; any other byte
/// would index past that table and is rejected where the header is loaded.
/// </summary>
public readonly record struct CartridgeDoorOrientation
{
    /// <summary>Creates an orientation from its decoded fields.</summary>
    public CartridgeDoorOrientation(DoorDirection direction, DoorClosingBehavior closing)
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

    /// <summary>True for <see cref="DoorDirection.Down"/> and <see cref="DoorDirection.Up"/> (<c>$82:DE55</c> bit 1).</summary>
    public bool IsVertical => Direction.IsVertical();

    /// <summary>The closing behavior of row <paramref name="row"/> of <c>Door_Closing_PLMs</c>.</summary>
    private static DoorClosingBehavior ClosingRow(int row) => row switch
    {
        0 => DoorClosingBehavior.None,
        1 => DoorClosingBehavior.BlueDoorCloses,
        2 => DoorClosingBehavior.EscapeGateCloses,
        _ => throw new InvalidDataException($"Door_Closing_PLMs has no row {row}."),
    };

    /// <summary>Decodes the cartridge byte, rejecting values outside <c>Door_Closing_PLMs</c>.</summary>
    public static CartridgeDoorOrientation Decode(byte raw)
    {
        if (raw > 0x0b)
            throw new InvalidDataException($"Door orientation byte ${raw:X2} is outside Door_Closing_PLMs ($00-$0B).");
        return new CartridgeDoorOrientation((DoorDirection)(raw & 3), ClosingRow(raw >> 2));
    }
}
