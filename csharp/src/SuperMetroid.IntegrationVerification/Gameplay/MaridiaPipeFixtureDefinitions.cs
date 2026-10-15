namespace SuperMetroid.IntegrationVerification.Gameplay;

/// <summary>Cartridge identities used by the bounded #391 diagnostic.</summary>
internal static class MaridiaPipeFixtureDefinitions
{
    /// <summary>$8F:D340, RoomHeader_PlasmaSpark: the authored source of door $83:A5AC.</summary>
    public const ushort PlasmaSparkRoom = 0xd340;

    /// <summary>$8F:D408, RoomHeader_Toilet: the tall Maridia elevatube.</summary>
    public const ushort TubeRoom = 0xd408;

    /// <summary>$8F:D48E, RoomHeader_Oasis: the southern tube destination.</summary>
    public const ushort OasisRoom = 0xd48e;

    /// <summary>$8F:E2B6 pins Samus to the center of the authored tube opening, X=$0080.</summary>
    public const ushort TubeCenterX = 0x0080;
}
