namespace SuperMetroid.Core.Game;

/// <summary>Executable layout of the three recursive hand-beam stages at $86:C796-C7FA.</summary>
internal abstract class MotherBrainHandBeamInstructionProgramDefinitions
{
    /// <summary>$86:C796, shared charging/fired hand-beam instruction list.</summary>
    internal const ushort Initial = 0xc796;
    /// <summary>$86:C7FB, Spawn_MotherBrainRedBeam_Fired callback.</summary>
    internal const int SpawnNextCallback = 0x86c7fb;
    /// <summary>$86:C796/C7B7/C7D8 repeat the same seven-frame sequence before C7F9 Delete.</summary>
    internal const int StageCount = 3, FramesPerStage = 7;
    /// <summary>Each native frame stores one duration word and one spritemap word.</summary>
    internal const int FrameBytes = 2 * sizeof(ushort);
    /// <summary>$86:C79A/C7BB/C7DC store a call opcode word followed by its three-byte callback.</summary>
    internal const int CallbackBytes = sizeof(ushort) + 3;
    internal const int StageBytes = FramesPerStage * FrameBytes + CallbackBytes;
    /// <summary>$86:C7F9, Delete following all three complete stages.</summary>
    internal const ushort TerminalDelete = Initial + StageCount * StageBytes;

    /// <summary>$86:C796/C79F/C7A3/C7A7/C7AB/C7AF/C7B3: selected frame holds,
    /// repeated in the next two stages (the repetition is calculated). Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] Durations = [3, 3, 2, 2, 1, 1, 1];
    public static int PresentationWordCount => StageCount * FramesPerStage;

    /// <summary>Calculates the interleaved spritemap operand, skipping the first frame's external call.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(StageStart(index / FramesPerStage) + FrameOffset(index % FramesPerStage) + sizeof(ushort));
    }

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging or RoomEnemyProjectileKind.MotherBrainHandBeamFired;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == TerminalDelete) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete;
        if (address is >= Initial and < TerminalDelete)
        {
            int offset = (address - Initial) % StageBytes;
            if (offset == FrameBytes) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_CallExternalFunctionInY;
            if (offset == 0) return Durations[0];
            int frames = offset - CallbackBytes;
            if (frames >= FrameBytes && frames % FrameBytes == 0) return Durations[frames / FrameBytes];
        }
        throw new InvalidDataException(
            $"Mother Brain hand-beam mechanics pointer ${address:X4} is outside its translated program.");
    }

    internal static int ReadExternalFunction(ushort instructionAddress)
    {
        if (instructionAddress >= Initial && instructionAddress < TerminalDelete &&
            (instructionAddress - Initial) % StageBytes == FrameBytes) return SpawnNextCallback;
        throw new InvalidDataException(
            $"Mother Brain hand-beam external call ${instructionAddress:X4} is not translated.");
    }

    internal static int StageStart(int stage) => Initial + stage * StageBytes;
    internal static int FrameOffset(int frame) => frame * FrameBytes + (frame == 0 ? 0 : CallbackBytes);
}
