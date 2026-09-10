/// <summary>Retail identities and controlled initial conditions for issue 517.</summary>
internal static class SpinDoorFixtureDefinitions
{
    /// <summary>RoomHeader_Flyway, $8F:9879; source of Door_Flyway_1 ($83:8BC2).</summary>
    public const ushort SourceRoom = 0x9879;
    /// <summary>RoomHeader_BombTorizo, $8F:9804; destination of the reported door.</summary>
    public const ushort DestinationRoom = 0x9804;
    /// <summary>Controlled right-edge source position in the three-screen Flyway.</summary>
    public const ushort SourceX = 0x02f8;
    /// <summary>Controlled descending spin's center, inside the open doorway.</summary>
    public const ushort SourceY = 0x0080;
    /// <summary>Nonzero whole and fractional speed to detect either being lost during loading.</summary>
    public const uint DescendingSpeed = 0x00023456;
    /// <summary>SHA-256 of the LF-normalized original-CPU loading and movement trace.</summary>
    public const string NativeTraceSha256 = "2DF88FD63FC4D173EC87CED263E9A79FD2C048E80924385A3C04ADC7D73B110D";
}
