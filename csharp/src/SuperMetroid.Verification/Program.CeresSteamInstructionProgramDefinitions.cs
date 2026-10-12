using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

/// <summary>Represents ceres steam instruction program definitions verification.</summary>
internal static class CeresSteamInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies ceres steam instruction program definitions.</summary>
    internal static void VerifyCeresSteamInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCeresSteamInstructionProgramDefinitions), () => VerifyCeresSteamInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies ceres steam instruction program definitions.</summary>
    internal static void VerifyCeresSteamInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < CeresSteamInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CeresSteamInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | definition.Address)),
                $"Ceres steam instruction mechanics word $A6:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = NewCeresSteamInstructionProgramReadGuard(rom);
        (CeresSteamVariant Variant, ushort Program, ushort Active)[] programs =
        [
            (CeresSteamVariant.Up,
                CeresSteamInstructionProgramDefinitions.Up,
                CeresSteamInstructionProgramDefinitions.UpActive),
            (CeresSteamVariant.Left,
                CeresSteamInstructionProgramDefinitions.Left,
                CeresSteamInstructionProgramDefinitions.LeftActive),
            (CeresSteamVariant.Down,
                CeresSteamInstructionProgramDefinitions.Down,
                CeresSteamInstructionProgramDefinitions.DownActive),
            (CeresSteamVariant.Right,
                CeresSteamInstructionProgramDefinitions.Right,
                CeresSteamInstructionProgramDefinitions.RightActive),
        ];

        foreach ((CeresSteamVariant variant, ushort program, ushort active) in programs)
        {
            RoomEnemySystem enemies = CreateSystem(variant, out RoomEnemySlot slot);
            RunFrames(enemies, slot, 1);
            AssertTrue(slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} starts hidden and intangible");
            AssertEqual(unchecked((ushort)(program + 6)), slot.CurrentInstruction,
                $"Ceres steam {variant} reaches its activation branch");

            RunFrames(enemies, slot, 1);
            AssertTrue(!slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} activation reveals its plume");
            AssertEqual(unchecked((ushort)(active + 4)), slot.CurrentInstruction,
                $"Ceres steam {variant} begins its active frames");

            RunFrames(enemies, slot, 21);
            AssertTrue(slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} enters its hidden hold");

            RunFrames(enemies, slot, 64);
            AssertTrue(!slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} hold returns to active frames");
            AssertEqual(unchecked((ushort)(active + 4)), slot.CurrentInstruction,
                $"Ceres steam {variant} completes its full cycle");
        }

        AssertEqual(CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int index = 0;
             index < CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                CeresSteamInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"Ceres steam execution covers presentation $A6:{address:X4}");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guard.ObservedWords.Count,
            "Ceres Steam production uses compiled visual selectors without ROM reads");
        for (int index = 0; index < CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort address = CeresSteamInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa6, address, out ushort selector),
                "Ceres Steam visual operand has a compiled selector");
            AssertEqual(ReadVerificationWord(rom, (0xa6 << 16) | address), selector,
                "Ceres Steam compiled visual selector matches original operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Ceres steam mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CeresSteamInstructionProgramDefinitions.ReadMechanicsWord(0xf051),
            "Ceres steam extended-spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresSteamInstructionProgramDefinitions.ReadMechanicsWord(0xf11d),
            "adjacent Ceres steam callback code is rejected as mechanics");

        _ = ProbeCeresSteamInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCeresSteamInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ceres steam allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ceres steam mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Ceres steam instruction mechanics: sixty-eight compiled words, four shared " +
            "directional cycles, and thirty-six executed selectors match cartridge data with " +
            "runtime ROM reads forbidden.");

        RoomEnemySystem CreateSystem(CeresSteamVariant variant, out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
            Action<RoomEnemySlot> initialize = enemies.CeresSteamBehavior.InitializeCeresSteam;
            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Steam;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.Parameter1 = (ushort)variant;
            initialize(slot);
            return enemies;
        }

        void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
            {
                EnemyInstructionRunner.Process(enemies, arguments);
                VerifyExecutedEnemySelector(rom, slot, executedOperands);
            }
        }
    }

    /// <summary>Probes ceres steam instruction mechanics allocation.</summary>
    private static int ProbeCeresSteamInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresSteamInstructionProgramDefinitions.ReadMechanicsWord(
                CeresSteamInstructionProgramDefinitions.Up);
        }
        return checksum;
    }

    /// <summary>Creates ceres steam instruction program read guard.</summary>
    private static CartridgeReadGuard NewCeresSteamInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(CeresSteamInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Ceres steam mechanics ${address:X6}.")
            .ObserveWords(0xa60000, CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount, CeresSteamInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Ceres steam instruction mechanics command.</summary>
    [VerificationCommand("--ceres-steam-instruction-mechanics")]
    internal static int CeresSteamInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCeresSteamInstructionProgramDefinitions), () => VerifyCeresSteamInstructionProgramDefinitions());
        return 0;
    }
}
