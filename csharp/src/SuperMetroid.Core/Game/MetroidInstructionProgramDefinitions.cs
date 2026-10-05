namespace SuperMetroid.Core.Game;

/// <summary>One compiled ordinary-Metroid mechanics word at its bank-$A3 address.</summary>
internal readonly record struct MetroidInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled animation timing, sound callbacks, and loop control for ordinary Metroids.
/// Interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal static class MetroidInstructionProgramDefinitions
{
    /// <summary><c>InstList_Metroid_ChasingSamus</c> at $A3:E9CF.</summary>
    internal const ushort ChasingSamus = 0xe9cf;
    /// <summary><c>InstList_Metroid_DrainingSamus</c> at $A3:EA25.</summary>
    internal const ushort DrainingSamus = 0xea25;
    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX</c> entry at $A3:EA1F.</summary>
    internal const ushort ChasingSoundCallback = 0xea1f;
    /// <summary><c>Instruction_Metroid_PlayDrainingSamusSFX</c> entry at $A3:EA39.</summary>
    internal const ushort DrainingSoundCallback = 0xea39;
    /// <summary><c>BombedOffVelocities</c>, adjacent non-instruction data at $A3:EA3F.</summary>
    internal const ushort AdjacentBombedOffVelocities = 0xea3f;

    private static readonly ushort[] FrameDurations = [16, 16, 6, 10, 16];
    internal static int MechanicsWordCount => 31;
    internal static int PresentationWordCount => 25;
    internal static MetroidInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool chasing = index < 23;
        int local = chasing ? index : index - 23;
        int frames = chasing ? 20 : 5;
        ushort start = chasing ? ChasingSamus : DrainingSamus;
        ushort address = (ushort)(start + (local < frames ? local * 4 : frames * 4 + (local - frames) * 2));
        return new(address, ReadMechanicsWord(address));
    }
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 20 ? ChasingSamus + index * 4 + 2 : DrainingSamus + (index - 20) * 4 + 2);
    }
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - ChasingSamus;
        if ((uint)offset < 80) return offset % 4 == 2;
        offset = address - DrainingSamus;
        return (uint)offset < 20 && offset % 4 == 2;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        bool chasing = address < DrainingSamus;
        int start = chasing ? ChasingSamus : DrainingSamus;
        int offset = address - start;
        int frames = chasing ? 20 : 5;
        if ((uint)offset < frames * 4 && offset % 4 == 0) return FrameDurations[offset / 4 % 5];
        if (offset == frames * 4) return chasing ? EnemyInstructionCodePointers.Instruction_Metroid_PlayRandomMetroidSFX :
            EnemyInstructionCodePointers.Instruction_Metroid_PlayDrainingSamusSFX;
        if (offset == frames * 4 + 2) return CommonEnemyInstructionCodes.Goto;
        if (offset == frames * 4 + 4) return (ushort)start;
        throw new InvalidDataException($"Metroid instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int pointer = (ushort)address;
        bool chasing = pointer < DrainingSamus;
        int offset = pointer - (chasing ? ChasingSamus : DrainingSamus);
        int timedBytes = chasing ? 80 : 20;
        return (uint)offset < timedBytes + 6 && (offset >= timedBytes || offset % 4 < 2);
    }
}
