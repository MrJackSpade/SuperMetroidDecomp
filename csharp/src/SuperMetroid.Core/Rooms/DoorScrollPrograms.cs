using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored scroll-byte writes performed by the pure bank-$8F door callbacks.
/// </summary>
/// <remarks>
/// The original routines are tiny 65816 programs, but they are definition data rather than
/// room-specific behavior: each one writes a fixed set of values to the 50-byte room-scroll
/// array and returns. Keeping that data here makes the runtime dispatcher exhaustive without
/// burying dozens of address cases inside functional room-loading code.
/// </remarks>
internal static class DoorScrollPrograms
{
    private static readonly IReadOnlyDictionary<ushort, DoorScrollWrite[]> Programs =
        new Dictionary<ushort, DoorScrollWrite[]>
        {
            [DoorCodes.DoorCode_Scroll6_Green] = [Green(0x06)],
            [DoorCodes.DoorASM_Scroll_0_Blue] = [Blue(0x00)],
            [DoorCodes.DoorASM_Scroll_13_Blue] = [Blue(0x13)],
            [DoorCodes.DoorASM_Scroll_4_Red_8_Green] = [Red(0x04), Green(0x08)],
            [DoorCodes.DoorASM_Scroll_8_9_A_B_Red] = [Red(0x08), Red(0x09), Red(0x0a), Red(0x0b)],
            [DoorCodes.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red] = [Red(0x02), Red(0x03), Red(0x04), Red(0x05), Red(0x0b), Red(0x0c), Red(0x0d), Red(0x11)],
            [DoorCodes.DoorASM_Scroll_1_4_Green] = [Green(0x01), Green(0x04)],
            [DoorCodes.DoorASM_Scroll_2_Blue] = [Blue(0x02)],
            [DoorCodes.DoorASM_Scroll_17_Blue] = [Blue(0x17)],
            [DoorCodes.DoorASM_Scroll_4_Blue] = [Blue(0x04)],
            [DoorCodes.DoorASM_Scroll_6_Green_duplicate] = [Green(0x06)],
            [DoorCodes.DoorASM_Scroll_3_Green] = [Green(0x03)],
            [DoorCodes.DoorASM_Scroll_18_1C_Green] = [Green(0x18), Green(0x1c)],
            [DoorCodes.DoorASM_Scroll_5_6_Blue] = [Blue(0x05), Blue(0x06)],
            [DoorCodes.DoorASM_Scroll_1D_Blue] = [Blue(0x1d)],
            [DoorCodes.DoorASM_Scroll_2_3_Green] = [Green(0x02), Green(0x03)],
            [DoorCodes.DoorASM_Scroll_0_Red_1_Green] = [Red(0x00), Green(0x01)],
            [DoorCodes.DoorASM_Scroll_B_Green] = [Green(0x0b)],
            [DoorCodes.DoorASM_Scroll_Scroll_1C_Red_1D_Blue] = [Red(0x1c), Blue(0x1d)],
            [DoorCodes.DoorASM_Scroll_4_Red] = [Red(0x04)],
            [DoorCodes.DoorASM_Scroll_20_24_25_Green] = [Green(0x20), Green(0x24), Green(0x25)],
            [DoorCodes.DoorASM_Scroll_2_Blue_duplicate] = [Blue(0x02)],
            [DoorCodes.DoorASM_Scroll_0_Green] = [Green(0x00)],
            [DoorCodes.DoorASM_Scroll_6_7_Green] = [Green(0x06), Green(0x07)],
            [DoorCodes.DoorASM_Scroll_1_Blue_2_Red] = [Blue(0x01), Red(0x02)],
            [DoorCodes.DoorASM_Scroll_1_Blue_3_Red] = [Blue(0x01), Red(0x03)],
            [DoorCodes.DoorASM_Scroll_0_Red_4_Blue] = [Red(0x00), Blue(0x04)],
            [DoorCodes.DoorASM_Scroll_2_3_Blue] = [Blue(0x02), Blue(0x03)],
            [DoorCodes.DoorASM_Scroll_0_1_Green] = [Green(0x00), Green(0x01)],
            [DoorCodes.DoorASM_Scroll_1_Green] = [Green(0x01)],
            [DoorCodes.DoorASM_Scroll_F_12_Green] = [Green(0x0f), Green(0x12)],
            [DoorCodes.DoorASM_Scroll_6_Green_duplicate_again] = [Green(0x06)],
            [DoorCodes.DoorASM_Scroll_0_Green_1_Blue] = [Green(0x00), Blue(0x01)],
            [DoorCodes.DoorASM_Scroll_2_Green] = [Green(0x02)],
            [DoorCodes.DoorASM_Scroll_3_4_Red_6_7_8_Blue] = [Red(0x03), Red(0x04), Blue(0x06), Blue(0x07), Blue(0x08)],
            [DoorCodes.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red] = [Blue(0x01), Blue(0x02), Blue(0x03), Green(0x04), Red(0x06)],
            [DoorCodes.DoorASM_Scroll_0_1_Blue] = [Blue(0x00), Blue(0x01)],
            [DoorCodes.DoorASM_Scroll_0_Blue_1_Red] = [Blue(0x00), Red(0x01)],
            [DoorCodes.DoorASM_Scroll_A_Green] = [Green(0x0a)],
            [DoorCodes.DoorASM_Scroll_0_2_Green] = [Green(0x00), Green(0x02)],
            [DoorCodes.DoorASM_Scroll_6_7_Blue_8_Red] = [Blue(0x06), Blue(0x07), Red(0x08)],
            [DoorCodes.DoorASM_Scroll_2_Red_3_Blue] = [Red(0x02), Blue(0x03)],
            [DoorCodes.DoorASM_Scroll_7_Green] = [Green(0x07)],
            [DoorCodes.DoorASM_Scroll_1_Red_2_Blue] = [Red(0x01), Blue(0x02)],
            [DoorCodes.DoorASM_Scroll_0_Blue_3_Red] = [Blue(0x00), Red(0x03)],
            [DoorCodes.DoorASM_Scroll_1_Blue_4_Red] = [Blue(0x01), Red(0x04)],
            [DoorCodes.DoorASM_Scroll_0_Blue_1_2_3_Red] = [Blue(0x00), Red(0x01), Red(0x02), Red(0x03)],
            [DoorCodes.DoorASM_Scroll_0_Green_duplicate] = [Green(0x00)],
            [DoorCodes.DoorASM_Scroll_0_1_Blue_4_Red] = [Blue(0x00), Blue(0x01), Red(0x04)],
            [DoorCodes.DoorASM_Scroll_0_Blue_3_Red_duplicate] = [Blue(0x00), Red(0x03)],
            [DoorCodes.DoorASM_Scroll_0_Blue_duplicate] = [Blue(0x00)],
            [DoorCodes.DoorASM_Scroll_0_Blue_1_Red_duplicate] = [Blue(0x00), Red(0x01)],
            [DoorCodes.DoorASM_Scroll_18_Blue] = [Blue(0x18)],
            [DoorCodes.DoorASM_Scroll_2_Blue_3_Red] = [Blue(0x02), Red(0x03)],
            [DoorCodes.DoorASM_Scroll_E_Red] = [Red(0x0e)],
            [DoorCodes.DoorASM_Scroll_1_Blue] = [Blue(0x01)],
            [DoorCodes.DoorASM_Scroll_0_Green_duplicate_again] = [Green(0x00)],
            [DoorCodes.DoorASM_Scroll_3_Red_4_Blue] = [Red(0x03), Blue(0x04)],
            [DoorCodes.DoorASM_Scroll_29_Blue] = [Blue(0x29)],
            [DoorCodes.DoorASM_Scroll_28_2E_Green] = [Green(0x28), Green(0x2e)],
            [DoorCodes.DoorASM_Scroll_6_7_8_9_A_B_Red] = [Red(0x06), Red(0x07), Red(0x08), Red(0x09), Red(0x0a), Red(0x0b)],
            [DoorCodes.DoorASM_Scroll_A_Red_B_Blue] = [Red(0x0a), Blue(0x0b)],
            [DoorCodes.DoorASM_Scroll_0_Red_4_Blue_duplicate] = [Red(0x00), Blue(0x04)],
            [DoorCodes.DoorASM_Scroll_0_Red_1_Blue] = [Red(0x00), Blue(0x01)],
            [DoorCodes.DoorASM_Scroll_9_Red_A_Blue] = [Red(0x09), Blue(0x0a)],
            [DoorCodes.DoorASM_Scroll_0_2_Red_1_Blue] = [Red(0x00), Red(0x02), Blue(0x01)],
            [DoorCodes.DoorASM_Scroll_1_Blue_duplicate] = [Blue(0x01)],
            [DoorCodes.DoorASM_Scroll_6_Blue] = [Blue(0x06)],
            [DoorCodes.DoorASM_Scroll_4_Red_duplicate] = [Red(0x04)],
            [DoorCodes.DoorASM_Scroll_4_7_Red] = [Red(0x04), Red(0x07)],
            [DoorCodes.DoorASM_Scroll_1_Blue_2_Red_duplicate] = [Blue(0x01), Red(0x02)],
            [DoorCodes.DoorASM_Scroll_0_2_Green_duplicate] = [Green(0x00), Green(0x02)],
            [DoorCodes.DoorASM_Scroll_0_1_Green_duplicate] = [Green(0x00), Green(0x01)],
            [DoorCodes.DoorASM_Scroll_18_Blue_19_Red] = [Blue(0x18), Red(0x19)],
        };

    /// <summary>True when the pointer names one of the fixed scroll-write callbacks.</summary>
    public static bool Contains(ushort pointer) => Programs.ContainsKey(pointer);

    /// <summary>Applies a translated program and reports whether the pointer was recognized.</summary>
    public static bool TryApply(ushort pointer, RoomScrollGrid scrolls)
    {
        ArgumentNullException.ThrowIfNull(scrolls);
        if (!Programs.TryGetValue(pointer, out DoorScrollWrite[]? writes))
            return false;

        foreach (DoorScrollWrite write in writes)
            scrolls.SetStorage(write.StorageIndex, (byte)write.State);
        return true;
    }

    /// <summary>All translated pure callback pointers, exposed for exhaustive ROM audits.</summary>
    public static IEnumerable<ushort> Pointers => Programs.Keys;

    private static DoorScrollWrite Red(byte index) =>
        new(index, RoomScrollState.RedBoundary);

    private static DoorScrollWrite Blue(byte index) =>
        new(index, RoomScrollState.Blue);

    private static DoorScrollWrite Green(byte index) =>
        new(index, RoomScrollState.Green);
}

/// <summary>One byte store made by a cartridge door callback.</summary>
internal readonly record struct DoorScrollWrite(byte StorageIndex, RoomScrollState State);
