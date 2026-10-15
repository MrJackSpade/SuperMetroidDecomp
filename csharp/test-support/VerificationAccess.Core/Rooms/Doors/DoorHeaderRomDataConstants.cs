namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values of the bank-$83 door headers that only verification reads.</summary>
internal static class DoorHeaderRomDataConstants
{
    /// <summary>Last Ceres door, <c>Door_CeresRidley</c>.</summary>
    public const ushort PostFxBlockEnd = 0xabb8;
    /// <summary>First Wrecked Ship door, <c>Door_BowlingAlley_0</c>.</summary>
    public const ushort PostFxBlockStart = 0xa18c;
    /// <summary>Last Lower Norfair door, <c>Door_LNSave_0</c>.</summary>
    public const ushort PreFxBlockEnd = 0x9ab6;
    /// <summary>First Crateria door, <c>Door_LandingSite_LandingCutscene</c>.</summary>
    public const ushort PreFxBlockStart = 0x88fe;
    /// <summary>Encoded byte length of one physical door record.</summary>
    public const int RecordByteCount = 12;

}
