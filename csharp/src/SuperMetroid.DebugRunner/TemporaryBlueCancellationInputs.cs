using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

/// <summary>Isolates Dash and equipment changes after controller-earned charge expiry.</summary>
internal static class TemporaryBlueCancellationInputs
{
    public static ushort At(int frame, bool left, int mode)
    {
        if (frame < 400) return TemporaryBlueCarryInputs.At(frame, left, 4);
        SnesButton input = SnesButton.R;
        if (mode is 1 or 2 or 4 or 5) input |= left ? SnesButton.Left : SnesButton.Right;
        if (mode is 2 or 5 or 7) input |= SnesButton.B;
        return (ushort)input;
    }

    public static void BeforeFrame(SamusState samus, int frame, int mode)
    {
        // Isolate the equipment word consumed by gameplay, not the pause-menu UI.
        // Collected inventory remains unchanged, as it does when disabling an item.
        if (frame == 400 && mode is >= 3 and <= 6)
            samus.EquippedItems &= unchecked((ushort)~(ushort)SamusEquipmentFlags.SpeedBooster);
        if (frame == 410 && mode == 6)
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.SpeedBooster;
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        if (frame < 399) return;
        bool rebuilding = mode == 2 && frame > 401;
        ushort expected = mode is 0 or 3 or 6 or 7 || frame <= 400 ? (ushort)0x0401 : (ushort)0;
        // The full row comparator checks every rebuilding counter value. This
        // separate assertion checks retention/cancellation and its terminal result.
        if (!rebuilding && samus.HorizontalSpeed.SpeedBoostCounter != expected)
            throw new InvalidDataException("Temporary boost cancellation did not occur at its native movement boundary.");
        if (frame == 459 && mode == 2 && samus.HorizontalSpeed.SpeedBoostCounter != 0x0201)
            throw new InvalidDataException("Running Dash must restart accumulation, not retain full boost.");
        if (samus.HorizontalSpeed.ContactDamageIndex != 0)
            throw new InvalidDataException("Retained charge is not itself active terrain contact damage.");
    }
}
