namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Baby Metroid used by Mother Brain's final
/// cutscene. Interleaved selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class MotherBrainBabyInstructionProgramDefinitions
{

    /// <summary><c>InstList_BabyMetroid_Initial</c> at $A9:CFA2.</summary>
    internal const ushort Initial = 0xcfa2;

    /// <summary><c>InstList_BabyMetroid_DrainingMotherBrain</c> at $A9:CFB8.</summary>
    internal const ushort DrainingMotherBrain = 0xcfb8;

    /// <summary><c>InstList_BabyMetroid_TakingFatalBlow</c> at $A9:CFCE.</summary>
    internal const ushort TakingFatalBlow = 0xcfce;

    /// <summary>$A9:CFA2/CFA6/CFAA/CFAE: selected normal pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort InitialPoseHold = 16;
    /// <summary>$A9:CFB8: selected first draining pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort DrainFirstPoseHold = 8;
    /// <summary>$A9:CFBC: selected second draining pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort DrainSecondPoseHold = 8;
    /// <summary>$A9:CFC0: selected third draining pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort DrainThirdPoseHold = 5;
    /// <summary>$A9:CFC4: selected return draining pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort DrainReturnPoseHold = 2;
    /// <summary>$A9:CFCE: selected fatal-blow hold before Sleep. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort FatalBlowHold = 128;
    /// <summary>Each loop has four duration/spritemap records followed by its native goto callback opcode.</summary>
    internal const int LoopFrames = 4, FrameBytes = 2 * sizeof(ushort);

    /// <summary>Dispatches the two frame loops and terminal fatal-blow hold/Sleep from the reviewed holds above.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int initialOffset = address - Initial;
        if (initialOffset >= 0 && initialOffset <= LoopFrames * FrameBytes && initialOffset % FrameBytes == 0)
            return initialOffset == LoopFrames * FrameBytes
                ? (ushort)BabyMetroidInstruction.GotoInitial : InitialPoseHold;
        int drainOffset = address - DrainingMotherBrain;
        if (drainOffset >= 0 && drainOffset <= LoopFrames * FrameBytes && drainOffset % FrameBytes == 0)
            return (drainOffset / FrameBytes) switch
            {
                0 => DrainFirstPoseHold,
                1 => DrainSecondPoseHold,
                2 => DrainThirdPoseHold,
                3 => DrainReturnPoseHold,
                _ => (ushort)BabyMetroidInstruction.GotoDrainingMotherBrain,
            };
        if (address == TakingFatalBlow) return FatalBlowHold;
        if (address == TakingFatalBlow + FrameBytes) return CommonEnemyInstructionCodes.Sleep;
        throw new InvalidDataException(
            $"Mother Brain Baby instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }
}
