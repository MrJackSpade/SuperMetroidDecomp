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
        [nameof(CommonEnemyProjectileInstructionProgramDefinitions)] = "ECEEAB15597158422F47BA4E1EB12EB6BF2D4F0DBB4BF2F87913A2E407C659D1",
        [nameof(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions)] = "6E5D066AC2B21A6DAA905FE0C06618FB560DF8982E2C2B8C2A5321041446104B",
        [nameof(GoldenTorizoJumpLandingInstructionProgramDefinitions)] = "7C40FB832920219672D6C26A111F163684B55251B169CC0F1D70E11799AC8456",
        [nameof(GoldenTorizoStunnedInstructionProgramDefinitions)] = "E81BAF67EE6DB25209B292A70F74D12F29CBD6C1D1B1A23821ABE5C51E5446A5",
        [nameof(TourianEntranceStatueInstructionProgramDefinitions)] = "75F94F33315595FA75DCDFEE8D1E35E4AF705F36C81C6A26BCFD5CFF4A39E079",
    };

    private static readonly Dictionary<string, string> CustomLayouts = new()
    {
        [nameof(MotherBrainBodyInstructionProgramDefinitions)] = "3DE6D14720ED5D9F3F581ED71952CCF8192F1B893DF922196100FD3D13314E3E",
        [nameof(MotherBrainHeadInstructionProgramDefinitions)] = "0C329E356A619E554E43622051779A9962B0EFB115B3C51748930960D3810196",
        [nameof(MotherBrainHandBeamBodyInstructionDefinitions)] = "EFF8284E8A50876F9711AD2BBC574BEEF32F083EDB492559C2ACECADAD552FE2",
        [nameof(MotherBrainFallingTubeInstructionDefinitions)] = "83D279758A5E1FEAF7FC600BCF7749A4E81AB60BAF5F71CA0EAEBF4D28296EA7",
    };

    internal static void GuardCustomLayouts(string root)
    {
        // Falling-tube layout delegates its five visual identities to this calculated catalog.
        GuardSource(root, "csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs", "E5B9FA6732E619B737CD88C679DBD0060082553328081CCE0FFA979592E25F85");
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
        SourceFingerprint.Of(source) == expected;

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
