using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Translated straight-line bank-$8F door callbacks that write the room scroll grid.</summary>
internal static class DoorScrollPrograms
{
    /// <summary>True when the native pointer selects a translated pure scroll callback.</summary>
    public static bool Contains(ushort pointer) => Dispatch(pointer, null);

    /// <summary>Executes ordered scroll writes; unknown pointers leave the grid unchanged.</summary>
    public static bool TryApply(ushort pointer, RoomScrollGrid scrolls)
    {
        ArgumentNullException.ThrowIfNull(scrolls);
        return Dispatch(pointer, scrolls);
    }

    // A null target probes callback ownership without performing writes.
    // The same named cases own both recognition and execution.
    private static bool Dispatch(ushort pointer, RoomScrollGrid? scrolls)
    {
        switch (pointer)
        {
            case DoorCodes.DoorCode_Scroll6_Green:
                Green(scrolls, 0x06);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Blue:
                Blue(scrolls, 0x00);
                return true;
            case DoorCodes.DoorASM_Scroll_13_Blue:
                Blue(scrolls, 0x13);
                return true;
            case DoorCodes.DoorASM_Scroll_4_Red_8_Green:
                Red(scrolls, 0x04);
                Green(scrolls, 0x08);
                return true;
            case DoorCodes.DoorASM_Scroll_8_9_A_B_Red:
                Red(scrolls, 0x08);
                Red(scrolls, 0x09);
                Red(scrolls, 0x0a);
                Red(scrolls, 0x0b);
                return true;
            case DoorCodes.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red:
                Red(scrolls, 0x02);
                Red(scrolls, 0x03);
                Red(scrolls, 0x04);
                Red(scrolls, 0x05);
                Red(scrolls, 0x0b);
                Red(scrolls, 0x0c);
                Red(scrolls, 0x0d);
                Red(scrolls, 0x11);
                return true;
            case DoorCodes.DoorASM_Scroll_1_4_Green:
                Green(scrolls, 0x01);
                Green(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_2_Blue:
                Blue(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_17_Blue:
                Blue(scrolls, 0x17);
                return true;
            case DoorCodes.DoorASM_Scroll_4_Blue:
                Blue(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_6_Green_duplicate:
                Green(scrolls, 0x06);
                return true;
            case DoorCodes.DoorASM_Scroll_3_Green:
                Green(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_18_1C_Green:
                Green(scrolls, 0x18);
                Green(scrolls, 0x1c);
                return true;
            case DoorCodes.DoorASM_Scroll_5_6_Blue:
                Blue(scrolls, 0x05);
                Blue(scrolls, 0x06);
                return true;
            case DoorCodes.DoorASM_Scroll_1D_Blue:
                Blue(scrolls, 0x1d);
                return true;
            case DoorCodes.DoorASM_Scroll_2_3_Green:
                Green(scrolls, 0x02);
                Green(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Red_1_Green:
                Red(scrolls, 0x00);
                Green(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_B_Green:
                Green(scrolls, 0x0b);
                return true;
            case DoorCodes.DoorASM_Scroll_Scroll_1C_Red_1D_Blue:
                Red(scrolls, 0x1c);
                Blue(scrolls, 0x1d);
                return true;
            case DoorCodes.DoorASM_Scroll_4_Red:
                Red(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_20_24_25_Green:
                Green(scrolls, 0x20);
                Green(scrolls, 0x24);
                Green(scrolls, 0x25);
                return true;
            case DoorCodes.DoorASM_Scroll_2_Blue_duplicate:
                Blue(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Green:
                Green(scrolls, 0x00);
                return true;
            case DoorCodes.DoorASM_Scroll_6_7_Green:
                Green(scrolls, 0x06);
                Green(scrolls, 0x07);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Blue_2_Red:
                Blue(scrolls, 0x01);
                Red(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Blue_3_Red:
                Blue(scrolls, 0x01);
                Red(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Red_4_Blue:
                Red(scrolls, 0x00);
                Blue(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_2_3_Blue:
                Blue(scrolls, 0x02);
                Blue(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_0_1_Green:
                Green(scrolls, 0x00);
                Green(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Green:
                Green(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_F_12_Green:
                Green(scrolls, 0x0f);
                Green(scrolls, 0x12);
                return true;
            case DoorCodes.DoorASM_Scroll_6_Green_duplicate_again:
                Green(scrolls, 0x06);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Green_1_Blue:
                Green(scrolls, 0x00);
                Blue(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_2_Green:
                Green(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_3_4_Red_6_7_8_Blue:
                Red(scrolls, 0x03);
                Red(scrolls, 0x04);
                Blue(scrolls, 0x06);
                Blue(scrolls, 0x07);
                Blue(scrolls, 0x08);
                return true;
            case DoorCodes.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red:
                Blue(scrolls, 0x01);
                Blue(scrolls, 0x02);
                Blue(scrolls, 0x03);
                Green(scrolls, 0x04);
                Red(scrolls, 0x06);
                return true;
            case DoorCodes.DoorASM_Scroll_0_1_Blue:
                Blue(scrolls, 0x00);
                Blue(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Blue_1_Red:
                Blue(scrolls, 0x00);
                Red(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_A_Green:
                Green(scrolls, 0x0a);
                return true;
            case DoorCodes.DoorASM_Scroll_0_2_Green:
                Green(scrolls, 0x00);
                Green(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_6_7_Blue_8_Red:
                Blue(scrolls, 0x06);
                Blue(scrolls, 0x07);
                Red(scrolls, 0x08);
                return true;
            case DoorCodes.DoorASM_Scroll_2_Red_3_Blue:
                Red(scrolls, 0x02);
                Blue(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_7_Green:
                Green(scrolls, 0x07);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Red_2_Blue:
                Red(scrolls, 0x01);
                Blue(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Blue_3_Red:
                Blue(scrolls, 0x00);
                Red(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Blue_4_Red:
                Blue(scrolls, 0x01);
                Red(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Blue_1_2_3_Red:
                Blue(scrolls, 0x00);
                Red(scrolls, 0x01);
                Red(scrolls, 0x02);
                Red(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Green_duplicate:
                Green(scrolls, 0x00);
                return true;
            case DoorCodes.DoorASM_Scroll_0_1_Blue_4_Red:
                Blue(scrolls, 0x00);
                Blue(scrolls, 0x01);
                Red(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Blue_3_Red_duplicate:
                Blue(scrolls, 0x00);
                Red(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Blue_duplicate:
                Blue(scrolls, 0x00);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Blue_1_Red_duplicate:
                Blue(scrolls, 0x00);
                Red(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_18_Blue:
                Blue(scrolls, 0x18);
                return true;
            case DoorCodes.DoorASM_Scroll_2_Blue_3_Red:
                Blue(scrolls, 0x02);
                Red(scrolls, 0x03);
                return true;
            case DoorCodes.DoorASM_Scroll_E_Red:
                Red(scrolls, 0x0e);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Blue:
                Blue(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Green_duplicate_again:
                Green(scrolls, 0x00);
                return true;
            case DoorCodes.DoorASM_Scroll_3_Red_4_Blue:
                Red(scrolls, 0x03);
                Blue(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_29_Blue:
                Blue(scrolls, 0x29);
                return true;
            case DoorCodes.DoorASM_Scroll_28_2E_Green:
                Green(scrolls, 0x28);
                Green(scrolls, 0x2e);
                return true;
            case DoorCodes.DoorASM_Scroll_6_7_8_9_A_B_Red:
                Red(scrolls, 0x06);
                Red(scrolls, 0x07);
                Red(scrolls, 0x08);
                Red(scrolls, 0x09);
                Red(scrolls, 0x0a);
                Red(scrolls, 0x0b);
                return true;
            case DoorCodes.DoorASM_Scroll_A_Red_B_Blue:
                Red(scrolls, 0x0a);
                Blue(scrolls, 0x0b);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Red_4_Blue_duplicate:
                Red(scrolls, 0x00);
                Blue(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_0_Red_1_Blue:
                Red(scrolls, 0x00);
                Blue(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_9_Red_A_Blue:
                Red(scrolls, 0x09);
                Blue(scrolls, 0x0a);
                return true;
            case DoorCodes.DoorASM_Scroll_0_2_Red_1_Blue:
                Red(scrolls, 0x00);
                Red(scrolls, 0x02);
                Blue(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Blue_duplicate:
                Blue(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_6_Blue:
                Blue(scrolls, 0x06);
                return true;
            case DoorCodes.DoorASM_Scroll_4_Red_duplicate:
                Red(scrolls, 0x04);
                return true;
            case DoorCodes.DoorASM_Scroll_4_7_Red:
                Red(scrolls, 0x04);
                Red(scrolls, 0x07);
                return true;
            case DoorCodes.DoorASM_Scroll_1_Blue_2_Red_duplicate:
                Blue(scrolls, 0x01);
                Red(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_0_2_Green_duplicate:
                Green(scrolls, 0x00);
                Green(scrolls, 0x02);
                return true;
            case DoorCodes.DoorASM_Scroll_0_1_Green_duplicate:
                Green(scrolls, 0x00);
                Green(scrolls, 0x01);
                return true;
            case DoorCodes.DoorASM_Scroll_18_Blue_19_Red:
                Blue(scrolls, 0x18);
                Red(scrolls, 0x19);
                return true;
            default: return false;
        }
    }

    /// <summary>Original callback catalog order, generated without a stored registration table.</summary>
    public static IEnumerable<ushort> Pointers
    {
        get
        {
            yield return DoorCodes.DoorCode_Scroll6_Green;
            yield return DoorCodes.DoorASM_Scroll_0_Blue;
            yield return DoorCodes.DoorASM_Scroll_13_Blue;
            yield return DoorCodes.DoorASM_Scroll_4_Red_8_Green;
            yield return DoorCodes.DoorASM_Scroll_8_9_A_B_Red;
            yield return DoorCodes.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red;
            yield return DoorCodes.DoorASM_Scroll_1_4_Green;
            yield return DoorCodes.DoorASM_Scroll_2_Blue;
            yield return DoorCodes.DoorASM_Scroll_17_Blue;
            yield return DoorCodes.DoorASM_Scroll_4_Blue;
            yield return DoorCodes.DoorASM_Scroll_6_Green_duplicate;
            yield return DoorCodes.DoorASM_Scroll_3_Green;
            yield return DoorCodes.DoorASM_Scroll_18_1C_Green;
            yield return DoorCodes.DoorASM_Scroll_5_6_Blue;
            yield return DoorCodes.DoorASM_Scroll_1D_Blue;
            yield return DoorCodes.DoorASM_Scroll_2_3_Green;
            yield return DoorCodes.DoorASM_Scroll_0_Red_1_Green;
            yield return DoorCodes.DoorASM_Scroll_B_Green;
            yield return DoorCodes.DoorASM_Scroll_Scroll_1C_Red_1D_Blue;
            yield return DoorCodes.DoorASM_Scroll_4_Red;
            yield return DoorCodes.DoorASM_Scroll_20_24_25_Green;
            yield return DoorCodes.DoorASM_Scroll_2_Blue_duplicate;
            yield return DoorCodes.DoorASM_Scroll_0_Green;
            yield return DoorCodes.DoorASM_Scroll_6_7_Green;
            yield return DoorCodes.DoorASM_Scroll_1_Blue_2_Red;
            yield return DoorCodes.DoorASM_Scroll_1_Blue_3_Red;
            yield return DoorCodes.DoorASM_Scroll_0_Red_4_Blue;
            yield return DoorCodes.DoorASM_Scroll_2_3_Blue;
            yield return DoorCodes.DoorASM_Scroll_0_1_Green;
            yield return DoorCodes.DoorASM_Scroll_1_Green;
            yield return DoorCodes.DoorASM_Scroll_F_12_Green;
            yield return DoorCodes.DoorASM_Scroll_6_Green_duplicate_again;
            yield return DoorCodes.DoorASM_Scroll_0_Green_1_Blue;
            yield return DoorCodes.DoorASM_Scroll_2_Green;
            yield return DoorCodes.DoorASM_Scroll_3_4_Red_6_7_8_Blue;
            yield return DoorCodes.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red;
            yield return DoorCodes.DoorASM_Scroll_0_1_Blue;
            yield return DoorCodes.DoorASM_Scroll_0_Blue_1_Red;
            yield return DoorCodes.DoorASM_Scroll_A_Green;
            yield return DoorCodes.DoorASM_Scroll_0_2_Green;
            yield return DoorCodes.DoorASM_Scroll_6_7_Blue_8_Red;
            yield return DoorCodes.DoorASM_Scroll_2_Red_3_Blue;
            yield return DoorCodes.DoorASM_Scroll_7_Green;
            yield return DoorCodes.DoorASM_Scroll_1_Red_2_Blue;
            yield return DoorCodes.DoorASM_Scroll_0_Blue_3_Red;
            yield return DoorCodes.DoorASM_Scroll_1_Blue_4_Red;
            yield return DoorCodes.DoorASM_Scroll_0_Blue_1_2_3_Red;
            yield return DoorCodes.DoorASM_Scroll_0_Green_duplicate;
            yield return DoorCodes.DoorASM_Scroll_0_1_Blue_4_Red;
            yield return DoorCodes.DoorASM_Scroll_0_Blue_3_Red_duplicate;
            yield return DoorCodes.DoorASM_Scroll_0_Blue_duplicate;
            yield return DoorCodes.DoorASM_Scroll_0_Blue_1_Red_duplicate;
            yield return DoorCodes.DoorASM_Scroll_18_Blue;
            yield return DoorCodes.DoorASM_Scroll_2_Blue_3_Red;
            yield return DoorCodes.DoorASM_Scroll_E_Red;
            yield return DoorCodes.DoorASM_Scroll_1_Blue;
            yield return DoorCodes.DoorASM_Scroll_0_Green_duplicate_again;
            yield return DoorCodes.DoorASM_Scroll_3_Red_4_Blue;
            yield return DoorCodes.DoorASM_Scroll_29_Blue;
            yield return DoorCodes.DoorASM_Scroll_28_2E_Green;
            yield return DoorCodes.DoorASM_Scroll_6_7_8_9_A_B_Red;
            yield return DoorCodes.DoorASM_Scroll_A_Red_B_Blue;
            yield return DoorCodes.DoorASM_Scroll_0_Red_4_Blue_duplicate;
            yield return DoorCodes.DoorASM_Scroll_0_Red_1_Blue;
            yield return DoorCodes.DoorASM_Scroll_9_Red_A_Blue;
            yield return DoorCodes.DoorASM_Scroll_0_2_Red_1_Blue;
            yield return DoorCodes.DoorASM_Scroll_1_Blue_duplicate;
            yield return DoorCodes.DoorASM_Scroll_6_Blue;
            yield return DoorCodes.DoorASM_Scroll_4_Red_duplicate;
            yield return DoorCodes.DoorASM_Scroll_4_7_Red;
            yield return DoorCodes.DoorASM_Scroll_1_Blue_2_Red_duplicate;
            yield return DoorCodes.DoorASM_Scroll_0_2_Green_duplicate;
            yield return DoorCodes.DoorASM_Scroll_0_1_Green_duplicate;
            yield return DoorCodes.DoorASM_Scroll_18_Blue_19_Red;
        }
    }

    private static void Red(RoomScrollGrid? scrolls, byte index) => scrolls?.SetStorage(index, RoomScrollState.RedBoundary);
    private static void Blue(RoomScrollGrid? scrolls, byte index) => scrolls?.SetStorage(index, RoomScrollState.Blue);
    private static void Green(RoomScrollGrid? scrolls, byte index) => scrolls?.SetStorage(index, RoomScrollState.Green);
}
