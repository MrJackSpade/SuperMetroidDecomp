namespace SuperMetroid.Core.Rooms;

/// <summary>Named 16-bit room-header pointers within cartridge bank $8F.</summary>
public static class RoomHeaderPointers
{
    /// <summary>Landing Site at $8F:91F8.</summary>
    public const ushort LandingSite = 0x91f8;

    /// <summary>Parlor and Alcatraz at $8F:92FD.</summary>
    public const ushort ParlorAndAlcatraz = 0x92fd;

    /// <summary>Crateria save station at $8F:93D5.</summary>
    public const ushort CrateriaSaveStation = 0x93d5;

    /// <summary>Climb at $8F:96BA.</summary>
    public const ushort Climb = 0x96ba;

    /// <summary>Pit Room at $8F:975C.</summary>
    public const ushort PitRoom = 0x975c;

    /// <summary>Blue Brinstar elevator room at $8F:97B5.</summary>
    public const ushort BlueBrinstarElevatorRoom = 0x97b5;

    /// <summary>Bomb Torizo Room at $8F:9804.</summary>
    public const ushort BombTorizoRoom = 0x9804;

    /// <summary>Green Brinstar elevator room at $8F:9938 (area $00, room $19).</summary>
    public const ushort GreenBrinstarElevatorRoom = 0x9938;

    /// <summary>Green Brinstar main shaft at $8F:9AD9 (area $01, room $00).</summary>
    public const ushort GreenBrinstarMainShaft = 0x9ad9;

    /// <summary>Flyway at $8F:9879.</summary>
    public const ushort Flyway = 0x9879;

    /// <summary>Morph Ball Room at $8F:9E9F.</summary>
    public const ushort MorphBallRoom = 0x9e9f;

    /// <summary>Construction Zone at $8F:9F11.</summary>
    public const ushort ConstructionZone = 0x9f11;

    /// <summary>First Missile Room at $8F:A107.</summary>
    public const ushort FirstMissileRoom = 0xa107;

    /// <summary>Warehouse Kihunter room at $8F:A4DA (area $01, room $2C).</summary>
    public const ushort WarehouseKihunter = 0xa4da;

    /// <summary>Warehouse Save room at $8F:A70B (area $01, room $36).</summary>
    public const ushort WarehouseSave = 0xa70b;

    /// <summary>Blue Brinstar Energy Tank Room at $8F:9F64.</summary>
    public const ushort BlueBrinstarEnergyTankRoom = 0x9f64;

    /// <summary>Blue Brinstar boulder room at $8F:A1AD (area $01, room $1C).</summary>
    public const ushort BlueBrinstarBoulders = 0xa1ad;

    /// <summary>Blue Brinstar double-missile room at $8F:A1D8 (area $01, room $1D).</summary>
    public const ushort BlueBrinstarDoubleMissile = 0xa1d8;

    /// <summary>Ceres elevator shaft at $8F:DF45.</summary>
    public const ushort CeresElevatorShaft = 0xdf45;

    /// <summary>Ceres falling-tile room at $8F:DF8D.</summary>
    public const ushort CeresFallingTileRoom = 0xdf8d;

    /// <summary>Ceres magnet stairs at $8F:DFD7.</summary>
    public const ushort CeresMagnetStairs = 0xdfd7;

    /// <summary>Ceres dead-scientist room at $8F:E021.</summary>
    public const ushort CeresDeadScientistRoom = 0xe021;

    /// <summary>Ceres final hallway at $8F:E06B.</summary>
    public const ushort CeresFinalHallway = 0xe06b;

    /// <summary>Ceres Ridley room at $8F:E0B5.</summary>
    public const ushort CeresRidleyRoom = 0xe0b5;
}
