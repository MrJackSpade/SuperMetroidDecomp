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
        [nameof(CommonEnemyProjectileInstructionProgramDefinitions)] = "745F2209AFBF057E617830A089E9CDA8D518CF2E5C327750D1BE8745AACED221",
        [nameof(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions)] = "4DCD82EAE1D6A1D3BC8FAE9F8A4E7E8DB7544B27DAE930FEC37005AD90344ADE",
        [nameof(GoldenTorizoJumpLandingInstructionProgramDefinitions)] = "F8EF0540D791B3715DE310B0F68F6DBE7E4FE32FB6F2D0CAA899008613455AA5",
        [nameof(GoldenTorizoStunnedInstructionProgramDefinitions)] = "3DA0A675BA4413792DFA683B0D8D0F51D6B54C8E4BE00909E91443D2A74C39E9",
        [nameof(TourianEntranceStatueInstructionProgramDefinitions)] = "BB51936C0584E148468F52405BBB88D117E4830915FC2050C4DD7E3C060A7AA4",
    };

    private static readonly Dictionary<string, string> CustomLayouts = new()
    {
        [nameof(MotherBrainBodyInstructionProgramDefinitions)] = "A1820853D30EFE3E4622BBA57B70CF3C054274CABC40D863A06CB6F57F8C677A",
        [nameof(MotherBrainHeadInstructionProgramDefinitions)] = "E36A1D0F040DF490E7C1D1755F03E63F3ED4705B075466D2110EAD6E5093199A",
        [nameof(MotherBrainHandBeamBodyInstructionDefinitions)] = "AE97385D5C904FABA81D5E2219D9BAF156BE4133F34F981BB76941C8AF7C309B",
        [nameof(MotherBrainFallingTubeInstructionDefinitions)] = "1C2F7138D82495CFF28A271C948A6A34FC9F0258C7493E6BF5A70572C4F60AE0",
    };

    internal static void GuardCustomLayouts(string root)
    {
        // Falling-tube layout delegates its five visual identities to this calculated catalog.
        GuardSource(root, "csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs", "8A566B9D6A4B899CD9790E9EA79394935234CB6013C6DFC92855D81223D89419");
        foreach ((string name, string expected) in CustomLayouts)
            GuardOwner(root, name, expected);
    }

    /// <summary>
    /// The reviewed source of a Core program owner: its shipped definition file, or the development
    /// file it moved to whole, followed by the development adapter holding what only tools read.
    /// </summary>
    internal static string OwnerSource(string root, string name)
    {
        string shipped = Path.Combine(root, "csharp/src/SuperMetroid.Core/Game", name + ".cs");
        string tooling = Path.Combine(root, "csharp/src/SuperMetroid.Tooling/Core/Game", name + ".cs");
        string adapter = Path.Combine(root, "csharp/src/SuperMetroid.Tooling/Core/Game", name + "Tooling.cs");
        string definition = File.Exists(shipped) ? shipped : tooling;
        return File.ReadAllText(definition) + (File.Exists(adapter) ? File.ReadAllText(adapter) : "");
    }

    private static void GuardOwner(string root, string name, string expected)
    {
        if (!SourceMatches(OwnerSource(root, name), expected))
            throw new InvalidDataException(name + ": reviewed static layout changed; update its adapter after source review.");
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
        GuardOwner(root, type.Name, expected);
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

    private static readonly (ushort Start, ushort End)[] HeadRegions =
    [
        (MotherBrainHeadInstructionProgramDefinitionsTooling.EarlyStart, MotherBrainHeadInstructionProgramDefinitionsTooling.EarlyEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowAndNeutralPhaseTwoStart, MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowAndNeutralPhaseTwoEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralStart, MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralRegionEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.CorpseAndRingsStart, MotherBrainHeadInstructionProgramDefinitionsTooling.CorpseAndRingsEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.BombAndLaserStart, MotherBrainHeadInstructionProgramDefinitionsTooling.BombAndLaserEnd),
        (MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowChargeStart, MotherBrainHeadInstructionProgramDefinitionsTooling.RainbowChargeEnd),
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
