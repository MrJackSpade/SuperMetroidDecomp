namespace SuperMetroid.Core.Fx;

/// <summary>
/// Which of its area's eight effect slots a room-FX record turns on (#1415): each area names one palette effect and one
/// animated-tile object per slot, and a record may turn on any combination of them, so the slots are composable.
/// </summary>
[Flags]
public enum RoomFxEffectSlots : byte
{
    /// <summary>No slot.</summary>
    None = 0,

    /// <summary>The area's first slot.</summary>
    Slot0 = 0x01,

    /// <summary>The area's second slot.</summary>
    Slot1 = 0x02,

    /// <summary>The area's third slot.</summary>
    Slot2 = 0x04,

    /// <summary>The area's fourth slot.</summary>
    Slot3 = 0x08,

    /// <summary>The area's fifth slot.</summary>
    Slot4 = 0x10,

    /// <summary>The area's sixth slot.</summary>
    Slot5 = 0x20,

    /// <summary>The area's seventh slot.</summary>
    Slot6 = 0x40,

    /// <summary>The area's eighth slot.</summary>
    Slot7 = 0x80,
}
