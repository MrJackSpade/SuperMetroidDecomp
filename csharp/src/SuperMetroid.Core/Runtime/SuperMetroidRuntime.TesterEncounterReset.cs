using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    private void ResetSavedEncounters()
    {
        // Run after the saved mirror is restored and before any room-state or
        // enemy/PLM selection. Ordinary room transitions must never reapply this.
        foreach (AreaId area in TesterEncounterResetDefinitions.Areas)
            System.ClearBossBits(area, BossBitMasks.AllKnown);
        System.ClearEvent(EventNumber.ShaktoolClearedPath);

        Span<byte> doors = stackalloc byte[Bank80SystemState.DoorBitByteCount];
        for (int index = 0; index < doors.Length; index++)
            doors[index] = System.GetOpenedDoorByteRaw(index);
        foreach (ushort bit in TesterEncounterResetDefinitions.EyeDoorBits)
            doors[bit >> 3] &= unchecked((byte)~(1 << (bit & 7)));
        System.LoadOpenedDoorBytes(doors);
    }
}