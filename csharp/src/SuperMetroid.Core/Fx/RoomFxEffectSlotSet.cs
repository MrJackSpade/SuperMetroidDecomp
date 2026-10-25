namespace SuperMetroid.Core.Fx;

/// <summary>Reads a record's effect slots by their place in the area's list (#1415).</summary>
public static class RoomFxEffectSlotSet
{
    /// <summary>The number of effect slots an area has.</summary>
    public const int Count = 8;

    /// <summary>Whether <paramref name="slots"/> turns on slot <paramref name="index"/>, 0 through 7.</summary>
    public static bool Includes(this RoomFxEffectSlots slots, int index) => slots.HasFlag(Slot(index));

    /// <summary>Slot <paramref name="index"/>, 0 through 7.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a slot.</exception>
    public static RoomFxEffectSlots Slot(int index) => index switch
    {
        0 => RoomFxEffectSlots.Slot0,
        1 => RoomFxEffectSlots.Slot1,
        2 => RoomFxEffectSlots.Slot2,
        3 => RoomFxEffectSlots.Slot3,
        4 => RoomFxEffectSlots.Slot4,
        5 => RoomFxEffectSlots.Slot5,
        6 => RoomFxEffectSlots.Slot6,
        7 => RoomFxEffectSlots.Slot7,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
