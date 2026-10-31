namespace SuperMetroid.Core.Fx;

/// <summary>
/// Which of its area's eight effect slots a room-FX record turns on (#1415): each area names one palette effect and one
/// animated-tile object per slot, and a record may turn on any combination of them, so the slots are composable.
/// </summary>
[Flags]
public enum RoomFxEffectSlots
{
    /// <summary>No slot.</summary>
    None = 0,

    /// <summary>The area's first slot.</summary>
    Slot0 = 1 << 0,

    /// <summary>The area's second slot.</summary>
    Slot1 = 1 << 1,

    /// <summary>The area's third slot.</summary>
    Slot2 = 1 << 2,

    /// <summary>The area's fourth slot.</summary>
    Slot3 = 1 << 3,

    /// <summary>The area's fifth slot.</summary>
    Slot4 = 1 << 4,

    /// <summary>The area's sixth slot.</summary>
    Slot5 = 1 << 5,

    /// <summary>The area's seventh slot.</summary>
    Slot6 = 1 << 6,

    /// <summary>The area's eighth slot.</summary>
    Slot7 = 1 << 7,
}
