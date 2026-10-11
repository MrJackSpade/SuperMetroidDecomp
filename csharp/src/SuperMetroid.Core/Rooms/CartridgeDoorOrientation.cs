namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The direction Samus travels through a door: bits 0-1 of the door header's orientation
/// byte (<c>DoorDirection</c>, <c>$0791</c>). Consumers such as <c>$80:AD3E</c> and
/// <c>$82:E358</c> mask these two bits and dispatch on the four values.
/// </summary>
public enum DoorDirection : byte
{
    /// <summary>Direction 0: the destination lies to the right.</summary>
    Right = 0,
    /// <summary>Direction 1: the destination lies to the left.</summary>
    Left = 1,
    /// <summary>Direction 2: the destination lies below.</summary>
    Down = 2,
    /// <summary>Direction 3: the destination lies above.</summary>
    Up = 3,
}

/// <summary>Axis predicates over <see cref="DoorDirection"/>.</summary>
public static class DoorDirections
{
    /// <summary>
    /// True for down and up: the bit-1 test of <c>$82:DE55</c> and <c>$80:AF89</c>. Horizontal
    /// doors align camera Y and step X; vertical doors align camera X and step Y.
    /// </summary>
    public static bool IsVertical(this DoorDirection direction) => direction switch
    {
        DoorDirection.Right or DoorDirection.Left => false,
        DoorDirection.Down or DoorDirection.Up => true,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined door direction."),
    };
}

/// <summary>
/// What closes behind Samus on arrival: bits 2-3 of the orientation byte, which selects a
/// row of <c>Door_Closing_PLMs</c> (<c>$8F:E68A</c>, spawned by <c>$82:E8F9</c>).
/// </summary>
public enum DoorClosingBehavior : byte
{
    /// <summary>Entries 0-3 are <c>$0000</c>: nothing closes.</summary>
    None = 0,
    /// <summary>Entries 4-7 spawn the blue door-closing PLM facing the direction.</summary>
    BlueDoorCloses = 1,
    /// <summary>Entries 8-B spawn <c>PLMEntries_gateThatClosesInEscapeRoom1_PLM</c>.</summary>
    EscapeGateCloses = 2,
}

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
