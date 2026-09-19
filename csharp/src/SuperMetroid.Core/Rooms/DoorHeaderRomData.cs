namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$83 door-header ranges retained for cartridge parity diagnostics.</summary>
public static class DoorHeaderRomData
{
    /// <summary>SNES bank containing all physical retail door records.</summary>
    public const int BankAddress = 0x830000;

    /// <summary>Encoded byte length of one physical door record.</summary>
    public const int RecordByteCount = 12;

    /// <summary>
    /// Shared elevator pseudo-door at <c>$83:88FC</c>. Its zero destination word is the
    /// native discriminator; the remaining bytes intentionally overlap the first physical
    /// door record because collision never consumes them for this special entry.
    /// </summary>
    public const ushort ElevatorPseudoDoorPointer = 0x88fc;

    /// <summary>First Crateria door, <c>Door_LandingSite_LandingCutscene</c>.</summary>
    public const ushort PreFxBlockStart = 0x88fe;

    /// <summary>Last Lower Norfair door, <c>Door_LNSave_0</c>.</summary>
    public const ushort PreFxBlockEnd = 0x9ab6;

    /// <summary>First Wrecked Ship door, <c>Door_BowlingAlley_0</c>.</summary>
    public const ushort PostFxBlockStart = 0xa18c;

    /// <summary>Last Ceres door, <c>Door_CeresRidley</c>.</summary>
    public const ushort PostFxBlockEnd = 0xabb8;
}
