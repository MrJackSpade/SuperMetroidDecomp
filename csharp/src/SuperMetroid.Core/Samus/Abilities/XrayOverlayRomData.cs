namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Definition data used by the item/special-room X-ray overlay at $84:831A.</summary>
public static class XrayOverlayRomData
{
    /// <summary>Four rotating item graphics slots precede the four fixed ammo/tank slots.</summary>
    public const int DynamicGraphicsSlots = 4;
    /// <summary>Room special-case records use bank $8F and four bytes per record.</summary>
    public const int RoomBank = 0x8f0000;
    /// <summary>Item draw instruction pointers use bank $84.</summary>
    public const int ItemBank = 0x840000;
}
