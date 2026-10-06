using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using static SuperMetroid.Core.Game.MotherBrainInstructionCodes;

namespace SuperMetroid.ResourceAudit;

/// <summary>Finite adapters for reviewed custom layouts; no gameplay interpreter is executed.</summary>
internal static class EnemyVisualProgramSpecializations
{
    /// <summary>$A9:9D21, Instruction_MotherBrainHead_GotoNeutralPhase3, no inline operands.</summary>
    private const ushort MotherBrainGotoNeutralPhaseThree = 0x9d21;

    // These files were inspected as control-only programs. Any source change
    // invalidates that disposition rather than granting a permanent exemption.
    private static readonly Dictionary<string, string> ControlOnly = new Dictionary<string, string>
    {
        [nameof(CommonEnemyProjectileInstructionProgramDefinitions)] = "E681BB9CF421324D916B1AD8F50D4780A17678158323A05EBA36ABB7A9E3509D",
        [nameof(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions)] = "699E83B6058B7FDC925C9E6BC0D9FC38DD8A7B682D17856FC07261B352A3201C",
        [nameof(GoldenTorizoJumpLandingInstructionProgramDefinitions)] = "3005930C469DDE29CCC5E38213020BE64F744E4BF92B41CB512D2ADF5536A0D0",
        [nameof(GoldenTorizoStunnedInstructionProgramDefinitions)] = "54E323801E720755A2B549DB47F66CBD18B0E4BCB2051DFF3A2D506EE3EEBFC6",
        [nameof(TourianEntranceStatueInstructionProgramDefinitions)] = "C59312C3A87CBDE15DB6397E86783EC62CD34942656F12CCA056EF94B838A322",
    };

    private static readonly Dictionary<string, string> CustomLayouts = new()
    {
        [nameof(MotherBrainBodyInstructionProgramDefinitions)] = "6EE26AC824E03B3697720FA6B9A18294EC171E834B5CACC01ACBEE8B1A87757E",
        [nameof(MotherBrainHeadInstructionProgramDefinitions)] = "E327A6AD9AC9E6C1C905D03A587DE4C403DD68049BC4EC1574BF0E4CEDDCF81B",
        [nameof(MotherBrainHandBeamBodyInstructionDefinitions)] = "1BDCF55B7F53E321BC293DAAA6DD82712484A305B44D0E533C18612E8B7F3D71",
        [nameof(MotherBrainFallingTubeInstructionDefinitions)] = "FCA4060649CFE4B7A5177FB4F311CBE357F56F44987AA88C33779E621A1362E8",
    };

    internal static void GuardCustomLayouts(string root)
    {
        // Falling-tube layout delegates its five visual identities to this calculated catalog.
        GuardSource(root, "csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs", "8A566B9D6A4B899CD9790E9EA79394935234CB6013C6DFC92855D81223D89419");
        foreach ((string name, string expected) in CustomLayouts)
            GuardSource(root, "csharp/src/SuperMetroid.Core/Game/" + name + ".cs", expected);
    }

    internal static void GuardSource(string root, string path, string expected)
    {
        string source = File.ReadAllText(Path.Combine(root, path));
        if (!SourceMatches(source, expected))
            throw new InvalidDataException(path + ": reviewed static layout changed; update its adapter after source review.");
    }

    internal static bool SourceMatches(string source, string expected) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            source.Replace("\r\n", "\n", StringComparison.Ordinal)))) == expected;

    internal static bool IsReviewedControlOnly(Type type, string root)
    {
        if (!ControlOnly.TryGetValue(type.Name, out string? expected)) return false;
        GuardSource(root, "csharp/src/SuperMetroid.Core/Game/" + type.Name + ".cs", expected);
        return true;
    }

    internal static bool IsMotherBrain(Type type) => type == typeof(MotherBrainBodyInstructionProgramDefinitions) ||
        type == typeof(MotherBrainHeadInstructionProgramDefinitions) ||
        type == typeof(MotherBrainHandBeamBodyInstructionDefinitions) ||
        type == typeof(MotherBrainFallingTubeInstructionDefinitions);

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
            for (int i = 0; i < MotherBrainFallingTubeInstructionDefinitions.ListCount; i++)
                operands.Add(checked((ushort)(MotherBrainFallingTubeInstructionDefinitions.FirstList +
                    i * MotherBrainFallingTubeInstructionDefinitions.ListStride + 2)));
            return true;
        }
        if (type == typeof(MotherBrainHandBeamBodyInstructionDefinitions))
        {
            foreach (ushort operand in MotherBrainHandBeamBodyInstructionDefinitions.PresentationOperands) operands.Add(operand);
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

    private static readonly (ushort Start, ushort End)[] HeadRegions =
    [
        (MotherBrainHeadInstructionProgramDefinitions.EarlyStart, MotherBrainHeadInstructionProgramDefinitions.EarlyEnd),
        (MotherBrainHeadInstructionProgramDefinitions.RainbowAndNeutralPhaseTwoStart, MotherBrainHeadInstructionProgramDefinitions.RainbowAndNeutralPhaseTwoEnd),
        (MotherBrainHeadInstructionProgramDefinitions.NeutralStart, MotherBrainHeadInstructionProgramDefinitions.NeutralRegionEnd),
        (MotherBrainHeadInstructionProgramDefinitions.CorpseAndRingsStart, MotherBrainHeadInstructionProgramDefinitions.CorpseAndRingsEnd),
        (MotherBrainHeadInstructionProgramDefinitions.BombAndLaserStart, MotherBrainHeadInstructionProgramDefinitions.BombAndLaserEnd),
        (MotherBrainHeadInstructionProgramDefinitions.RainbowChargeStart, MotherBrainHeadInstructionProgramDefinitions.RainbowChargeEnd),
    ];

    private static int HeadCommandBytes(ushort opcode) => opcode switch
    {
        Instruction_MotherBrain_GotoX or Instruction_MotherBrainHead_EnableNeckMovement_GotoX or
        Instruction_MotherBrainHead_QueueSoundX_Lib2_Max6 or Instruction_MotherBrainHead_QueueSoundX_Lib3_Max6 or
        Instruction_MotherBrainHead_SpawnBombProjectileWithParamX => 4,
        Instruction_CommonA9_Sleep or MotherBrainGotoNeutralPhaseThree or
        Instruction_MotherBrainHead_IncBabyMetroidAttackCounter or Instruction_MotherBrainHead_ResetBabyMetroidAttackCounter or
        Instruction_MotherBrainHead_DisableNeckMovement or Instruction_MotherBrainHead_AimOnionRingsAtBabyMetroid or
        Instruction_MotherBrainHead_AimOnionRingsAtSamus or Instruction_MotherBrainHead_QueueBabyMetroidAttackSFX or
        Instruction_MotherBrainHead_SpawnOnionRingsProjectile or Instruction_MotherBrainHead_SpawnPurpleBreathBigProjectile or
        Instruction_MotherBrainHead_MaybeGotoNeutralPhase3 or Instruction_MotherBrainHead_SpawnDroolProjectile or
        Instruction_MotherBrainHead_SetMainShakeTimerTo50 or Instruction_MotherBrainHead_MaybeGotoNeutralPhase2 or
        Instruction_MotherBrainHead_GotoDyingDroolInstList or InstList_MotherBrainHead_SpawnLaserProjectile or
        Instruction_MotherBrainHead_SpawnRainbowBeamChargingProj or Instruction_MotherBrainHead_SetupEffectsForRainbowBeamCharge => 2,
        _ => throw new InvalidDataException($"Unknown Mother Brain head command ${opcode:X4}; static layout needs review."),
    };

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
