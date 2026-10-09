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
    /// <summary>Total byte extent of one seven-frame stage, including its external-call command.</summary>
    internal const int StageBytes = FramesPerStage * FrameBytes + CallbackBytes;
    /// <summary>$86:C7F9, Delete following all three complete stages.</summary>
    internal const ushort TerminalDelete = Initial + StageCount * StageBytes;

    /// <summary>$86:C796/C79F/C7A3/C7A7/C7AB/C7AF/C7B3: selected frame holds,
    /// repeated in the next two stages (the repetition is calculated). Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] Durations = [3, 3, 2, 2, 1, 1, 1];
    /// <summary>Number of spritemap operands across the three stages.</summary>
    public static int PresentationWordCount => StageCount * FramesPerStage;

    /// <summary>Calculates the interleaved spritemap operand, skipping the first frame's external call.</summary>
    /// <param name="index">Zero-based frame index across all three stages.</param>
    /// <returns>Bank-local address of the frame's spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled frame sequence.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(StageStart(index / FramesPerStage) + FrameOffset(index % FramesPerStage) + sizeof(ushort));
    }

    /// <summary>Identifies the two projectile kinds whose instruction pointers belong to this program.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns><see langword="true"/> for the charging or fired Mother Brain hand beam.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging or RoomEnemyProjectileKind.MotherBrainHandBeamFired;

    /// <summary>Resolves instruction words within the translated stages and final delete command.</summary>
    /// <param name="address">Bank-local instruction address to decode.</param>
    /// <returns>The timer, external-call opcode, or delete opcode stored at the address.</returns>
    /// <exception cref="InvalidDataException">The address is not a recognized mechanics word in the translated program.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == TerminalDelete) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete;
        if (address >= Initial && address < TerminalDelete)
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

    /// <summary>Returns the native spawn-next-stage callback for a translated external-call instruction.</summary>
    /// <param name="instructionAddress">Address of the external-call opcode in the compiled sequence.</param>
    /// <returns>The bank-$86 callback address used by the native program.</returns>
    /// <exception cref="InvalidDataException">The address does not identify one of the stage call commands.</exception>
    internal static int ReadExternalFunction(ushort instructionAddress)
    {
        if (instructionAddress >= Initial && instructionAddress < TerminalDelete &&
            (instructionAddress - Initial) % StageBytes == FrameBytes) return SpawnNextCallback;
        throw new InvalidDataException(
            $"Mother Brain hand-beam external call ${instructionAddress:X4} is not translated.");
    }

    /// <summary>Computes the bank-local address of a stage's first frame.</summary>
    /// <param name="stage">Zero-based stage number in the three-stage beam sequence.</param>
    /// <returns>Address of that stage's initial frame-duration word.</returns>
    internal static int StageStart(int stage) => Initial + stage * StageBytes;

    /// <summary>Computes a frame's byte offset within its stage, accounting for the call after the first frame.</summary>
    /// <param name="frame">Zero-based frame number within one stage.</param>
    /// <returns>Offset from the stage start to the frame's duration word.</returns>
    internal static int FrameOffset(int frame) => frame * FrameBytes + (frame == 0 ? 0 : CallbackBytes);
}
