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
    public static RoomFxEffectSlots Slot(int index) => Slots[Ensure.BetweenInclusive(0, Count - 1, index)];

    /// <summary>Each slot, by its place in the area's list.</summary>
    private static readonly RoomFxEffectSlots[] Slots =
    [
        RoomFxEffectSlots.Slot0, RoomFxEffectSlots.Slot1, RoomFxEffectSlots.Slot2, RoomFxEffectSlots.Slot3,
        RoomFxEffectSlots.Slot4, RoomFxEffectSlots.Slot5, RoomFxEffectSlots.Slot6, RoomFxEffectSlots.Slot7,
    ];
}
