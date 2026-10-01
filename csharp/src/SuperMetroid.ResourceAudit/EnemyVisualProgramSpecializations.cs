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
        [nameof(CommonEnemyProjectileInstructionProgramDefinitions)] = "4B9F47E80680DDFA78FC9716121B938E915AADCB4FBB4C09D5DEA9F5BB21B3B2",
        [nameof(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions)] = "D2A9DC8911732A71F49E80E9D95AD47D1976AD324D20D9712A364BEC8E090DF1",
        [nameof(GoldenTorizoJumpLandingInstructionProgramDefinitions)] = "315A995B59B5806888D50068184CBEABDE80B749B6F830B81650586C6F1F80DB",
        [nameof(GoldenTorizoStunnedInstructionProgramDefinitions)] = "F00EF6C4538BC256AC8C6FED813F52C2BC2334B36D640B3312333E5B54C76BB1",
        [nameof(TourianEntranceStatueInstructionProgramDefinitions)] = "C9325C79B422FC75760E847A28083D4B07DA9EBE8EE2E7518FBD406CD2C473B2",
    };

    private static readonly Dictionary<string, string> CustomLayouts = new()
    {
        [nameof(MotherBrainBodyInstructionProgramDefinitions)] = "6C16408D4EDA53AF1FCD170C725CCE24D3D99C28C7F3F0A4AD81E93D52377A49",
        [nameof(MotherBrainHeadInstructionProgramDefinitions)] = "D6A0B754F4B7E98F0A8A31D3006D8AE1723992E7C3E8FA724B6C9E137813A31E",
        [nameof(MotherBrainHandBeamBodyInstructionDefinitions)] = "5CE8C4528445A5CB7EEF07E09B761B943B6B148DEF942937704C9EB9F7E4953D",
        [nameof(MotherBrainFallingTubeInstructionDefinitions)] = "033CF1B6D8623C218FE238800A8CB556478372042B7B42D562CB4BE207419C96",
    };

    internal static void GuardCustomLayouts(string root)
    {
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
