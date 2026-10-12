using SuperMetroid.Core.Game;
using static SuperMetroid.Core.Game.MotherBrainInstructionCodes;

namespace SuperMetroid.ResourceAudit;

/// <summary>Finite adapters for reviewed custom layouts; no gameplay interpreter is executed.</summary>
internal static class EnemyVisualProgramSpecializations
{
    /// <summary>$A9:9D21, Instruction_MotherBrainHead_GotoNeutralPhase3, no inline operands.</summary>
    private const ushort MotherBrainGotoNeutralPhaseThree = 0x9d21;

    // Owners whose programs contain only control words: no visual selector operands to resolve.
    /// <summary>Stores control only.</summary>
    private static readonly HashSet<string> ControlOnly =
    [
        nameof(CommonEnemyProjectileInstructionProgramDefinitions),
        nameof(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions),
        nameof(GoldenTorizoJumpLandingInstructionProgramDefinitions),
        nameof(GoldenTorizoStunnedInstructionProgramDefinitions),
        nameof(TourianEntranceStatueInstructionProgramDefinitions),
    ];

    /// <summary>Checks whether the instruction controls flow without drawing artwork.</summary>
    internal static bool IsControlOnly(Type type) => ControlOnly.Contains(type.Name);

    /// <summary>Checks whether the program belongs to Mother Brain.</summary>
    internal static bool IsMotherBrain(Type type) => type == typeof(MotherBrainBodyInstructionProgramDefinitions) ||
        type == typeof(MotherBrainHeadInstructionProgramDefinitions) ||
        type == typeof(MotherBrainHandBeamBodyInstructionDefinitions) ||
        type == typeof(MotherBrainFallingTubeInstructionDefinitions);

    /// <summary>Attempts to resolve operands.</summary>
    internal static bool TryOperands(Type type, HashSet<ushort> operands)
    {
        if (type == typeof(MotherBrainBodyInstructionProgramDefinitions))
        {
            // Timers are independently obtained from the mechanics-word gaps.
            operands.Add(MotherBrainBodyInstructionProgramDefinitions.InitialDummyVisualOperand);
            return true;
        }
        if (type == typeof(MotherBrainFallingTubeInstructionDefinitions))
        {
            for (int i = 0; i < MotherBrainFallingTubeInstructionDefinitionsTooling.ListCount; i++)
                operands.Add(checked((ushort)(MotherBrainFallingTubeInstructionDefinitions.FirstList +
                    i * MotherBrainFallingTubeInstructionDefinitions.ListStride + 2)));
            return true;
        }
        if (type == typeof(MotherBrainHandBeamBodyInstructionDefinitions))
        {
            foreach (ushort operand in MotherBrainHandBeamBodyInstructionDefinitionsTooling.PresentationOperands) operands.Add(operand);
            return true;
        }
        if (type != typeof(MotherBrainHeadInstructionProgramDefinitions)) return false;
        foreach ((ushort start, ushort end) in HeadRegions)
        {
            // Linear static decoding of every record, including dormant lists.
            // Gotos consume their operands but never execute or choose a path.
            int cursor = start;
            while (cursor <= end)
            {
                ushort word = MotherBrainHeadInstructionProgramDefinitions.ReadWord((ushort)cursor);
                int bytes;
                if (word < 0x8000)
                {
                    operands.Add(checked((ushort)(cursor + 2)));
                    bytes = 4;
                }
                else bytes = HeadCommandBytes(word);
                if (cursor + bytes > end + 2)
                    throw new InvalidDataException($"Mother Brain head record ${cursor:X4} crosses its declared region.");
                cursor += bytes;
            }
        }
        return true;
    }

    /// <summary>Stores head regions.</summary>
    private static readonly (ushort Start, ushort End)[] HeadRegions =
    [
        (MotherBrainHeadInstructionProgramDefinitionsTooling.EarlyStart, MotherBrainHeadInstructionProgramDefinitionsTooling.EarlyEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowAndNeutralPhaseTwoStart, MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowAndNeutralPhaseTwoEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralStart, MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralRegionEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.CorpseAndRingsStart, MotherBrainHeadInstructionProgramDefinitionsTooling.CorpseAndRingsEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.BombAndLaserStart, MotherBrainHeadInstructionProgramDefinitionsTooling.BombAndLaserEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowChargeStart, MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowChargeEnd),
    ];

    /// <summary>Head command bytes.</summary>
    private static int HeadCommandBytes(ushort opcode) =>
        opcode is Instruction_CommonA9_Sleep or MotherBrainGotoNeutralPhaseThree ? 2
        : Enum.IsDefined((MotherBrainInstruction)opcode) ? HeadCommandBytes((MotherBrainInstruction)opcode)
        : throw new InvalidDataException($"Unknown Mother Brain head command ${opcode:X4}; static layout needs review.");

    /// <summary>Head command bytes.</summary>
    private static int HeadCommandBytes(MotherBrainInstruction opcode) => opcode switch
    {
        MotherBrainInstruction.MotherBrain_GotoX or MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX or
        MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6 or MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib3_Max6 or
        MotherBrainInstruction.MotherBrainHead_SpawnBombProjectileWithParamX => 4,
        MotherBrainInstruction.MotherBrainHead_IncBabyMetroidAttackCounter or MotherBrainInstruction.MotherBrainHead_ResetBabyMetroidAttackCounter or
        MotherBrainInstruction.MotherBrainHead_DisableNeckMovement or MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtBabyMetroid or
        MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus or MotherBrainInstruction.MotherBrainHead_QueueBabyMetroidAttackSFX or
        MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile or MotherBrainInstruction.MotherBrainHead_SpawnPurpleBreathBigProjectile or
        MotherBrainInstruction.MotherBrainHead_MaybeGotoNeutralPhase3 or MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile or
        MotherBrainInstruction.MotherBrainHead_SetMainShakeTimerTo50 or MotherBrainInstruction.MotherBrainHead_MaybeGotoNeutralPhase2 or
        MotherBrainInstruction.MotherBrainHead_GotoDyingDroolInstList or MotherBrainInstruction.InstList_MotherBrainHead_SpawnLaserProjectile or
        MotherBrainInstruction.MotherBrainHead_SpawnRainbowBeamChargingProj or MotherBrainInstruction.MotherBrainHead_SetupEffectsForRainbowBeamCharge => 2,
        _ => throw new InvalidDataException($"{opcode} is not a Mother Brain head command; static layout needs review."),
    };

    /// <summary>Attempts to resolve.</summary>
    internal static bool TryResolve(Type type, ushort operand, out ushort pointer)
    {
        if (type == typeof(MotherBrainBodyInstructionProgramDefinitions))
            pointer = MotherBrainBodyInstructionProgramDefinitions.ReadVisualSelector(operand);
        else if (type == typeof(MotherBrainHeadInstructionProgramDefinitions))
            pointer = MotherBrainHeadInstructionProgramDefinitions.ReadWord(operand);
        else if (type == typeof(MotherBrainHandBeamBodyInstructionDefinitions))
            pointer = MotherBrainHandBeamBodyInstructionDefinitions.ReadVisualSelector(operand);
        else if (type == typeof(MotherBrainFallingTubeInstructionDefinitions))
            pointer = MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(operand);
        else { pointer = 0; return false; }
        return true;
    }
}
