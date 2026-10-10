namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed control words for the n00b-tube PLM's two reachable instruction
/// branches. Draw operands select separately compiled physical layouts; the
/// break sound remains an authored audio operand at its one byte position.
/// </summary>
internal static class NoobTubePlmProgramDefinitions
{
    /// <summary>Main n00b-tube instruction list at $84:D4D4.</summary>
    private const ushort MainStart = 0xd4d4;
    /// <summary>One-byte library-two break sound at $84:D506.</summary>
    private const ushort BreakSoundAddress = 0xd506;
    /// <summary>Continuation after the break-sound byte at $84:D507.</summary>
    private const ushort MainAfterSound = 0xd507;
    /// <summary>Already-broken room-state branch at $84:D521.</summary>
    private const ushort AlreadyBrokenStart = 0xd521;

    /// <summary>$84:D4E8: install the accepted-input wake callback after a power-bomb hit.</summary>
    private const ushort WaitForInput = 0xd4e8;
    /// <summary>$84:D4F2: lock Samus and begin the crack/break sequence.</summary>
    private const ushort BreakTube = 0xd4f2;

    /// <summary>
    /// Semantic NTSC control cases preserve exactly thirty-six word starts and
    /// the single packed sound byte. Preserve parity changes and reject the
    /// unused D519..D520 draw branch; no overlapping word views are introduced.
    /// No program blob or generated cache remains.
    /// </summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (!IsEvenStep(address, MainStart, 0xd504) &&
            !IsEvenStep(address, MainAfterSound, 0xd517) &&
            !IsEvenStep(address, AlreadyBrokenStart, 0xd523)) return false;
        value = address switch
        {
            0xd4d4 => (ushort)RoomPlmInstruction.GotoIfEventSet,
            0xd4d6 or 0xd511 => (ushort)NoobTubePlmRomData.BrokenEvent,
            0xd4d8 => AlreadyBrokenStart,
            0xd4da or 0xd4e8 => (ushort)RoomPlmInstruction.LinkInstruction,
            0xd4dc => WaitForInput,
            0xd4de or 0xd4ec => (ushort)RoomPlmInstruction.InstallPreInstruction,
            0xd4e0 => (ushort)NoobTubePlmPreInstruction.WakeOnPowerBomb,
            0xd4e2 or 0xd4fc or 0xd500 => 1,
            0xd4e4 => (ushort)NoobTubeDraw.Intact,
            0xd4e6 or 0xd4f0 => (ushort)RoomPlmInstruction.Sleep,
            0xd4ea => BreakTube,
            0xd4ee => (ushort)NoobTubePlmPreInstruction.WakeOnAcceptedInput,
            0xd4f2 => (ushort)RoomPlmInstruction.ClearPreInstruction,
            0xd4f4 => (ushort)RoomPlmInstruction.LockSamus,
            0xd4f6 => (ushort)RoomPlmInstruction.SpawnNoobTubeCrack,
            0xd4f8 => 48,
            0xd4fa => (ushort)NoobTubeDraw.Damaged,
            0xd4fe => (ushort)NoobTubeDraw.OpenedRows,
            0xd502 => (ushort)NoobTubeDraw.BrokenFull,
            0xd504 => (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            0xd507 => (ushort)RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles,
            0xd509 => (ushort)RoomPlmInstruction.TriggerNoobTubeEarthquake,
            0xd50b => 96,
            0xd50d => (ushort)NoobTubeDraw.Opened,
            0xd50f => (ushort)RoomPlmInstruction.SetEvent,
            0xd513 or AlreadyBrokenStart => (ushort)RoomPlmInstruction.EnableNoobTubeWaterPhysics,
            0xd515 => (ushort)RoomPlmInstruction.UnlockSamus,
            _ => (ushort)RoomPlmInstruction.Delete, // D517 or D523.
        };
        return true;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address == BreakSoundAddress)
        {
            value = NoobTubePlmRomData.BreakSound;
            return true;
        }
        value = 0;
        return false;
    }

    private static bool IsEvenStep(ushort address, ushort first, ushort last) =>
        address >= first && address <= last && ((address - first) & 1) == 0;
}
