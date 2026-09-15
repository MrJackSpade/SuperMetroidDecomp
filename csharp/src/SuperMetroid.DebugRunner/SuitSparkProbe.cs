using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Post-message suit admission; windup itself is earned with real inputs.</summary>
internal static class SuitSparkProbe
{
    public static ushort InputAt(int frame, bool left, int mode)
    {
        if (frame <= 151) return CrystalSparkProbe.InputAt(frame, left, 0);
        if ((mode & 3) == 0) return 0;
        return (ushort)(0x8000 | ((mode & 3) == 3 ? 0 : (mode & 3) == 1 ?
            (left ? 0x200 : 0x100) : (left ? 0x100 : 0x200)));
    }

    public static void Begin(ISnesAddressSpace bus, SuperMetroidRuntime runtime, SamusState samus, int mode)
    {
        if (samus.Shinespark.Phase != ShinesparkPhase.Windup)
            throw new InvalidDataException("Suit collection must interrupt controller-earned windup.");
        samus.EquippedItems |= (ushort)(mode >= 4 ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.VariaSuit);
        samus.CollectedItems = samus.EquippedItems;
        samus.SelectedHudItem = 5;
        runtime.SuitPickup.Begin(bus, samus, 0, 355,
            mode >= 4 ? SamusSuitPickupKind.Gravity : SamusSuitPickupKind.Varia);
    }

    public static void Verify(SuperMetroidRuntime runtime, SamusState samus, int frame, int mode)
    {
        if (frame == 152 && (!runtime.SuitPickup.IsActive || samus.Shinespark.StartStopTimer != 30))
            throw new InvalidDataException("Suit did not suspend windup.");
        if (frame == 322 && ((mode & 3) is 1 or 2) &&
            (!samus.Xray.IsActive || samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
             samus.HorizontalSpeed.SpeedBoostCounter != 0x0400))
            throw new InvalidDataException("Suit release did not replace windup with X-ray while retaining boost.");
    }

    public static string Snapshot(SuperMetroidRuntime runtime, SamusState samus)
    {
        var suit = runtime.SuitPickup;
        uint hash = 2166136261u;
        foreach (ushort word in suit.WindowTable)
        {
            hash = unchecked((hash ^ (byte)word) * 16777619u);
            hash = unchecked((hash ^ (byte)(word >> 8)) * 16777619u);
        }
        // X-ray subsequently reuses this shared table. The suit owns its numeric
        // window only until activation; its independent C# buffer is not that alias.
        return $"{(suit.IsActive ? 1 : 0):X4},{suit.Substate:X4},{samus.EquippedItems:X4}," +
            $"{samus.Shinespark.StartStopTimer:X4},{(samus.Xray.IsActive ? 1 : 0):X4}," +
            $"{hash:X8},{suit.LightBeamPosition:X4},{suit.LightBeamWideningSpeed:X4}," +
            $"{suit.FixedColorRed:X2},{suit.FixedColorGreen:X2},{suit.FixedColorBlue:X2}";
    }
}
