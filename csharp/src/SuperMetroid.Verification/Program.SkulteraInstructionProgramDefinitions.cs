using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents skultera instruction program definitions verification.</summary>
internal static class SkulteraInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies skultera instruction program definitions.</summary>
    internal static void VerifySkulteraInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySkulteraInstructionProgramDefinitions), () => VerifySkulteraInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies skultera instruction program definitions.</summary>
    internal static void VerifySkulteraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < SkulteraInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SkulteraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa30000 | definition.Address)),
                $"Skultera instruction mechanics word $A3:{definition.Address:X4}");
        }

        var guard = NewSkulteraInstructionProgramReadGuard(rom);
        RoomEnemySystem leftSystem =
            CreateSkulteraProgramSystem(guard, startsLeft: true, out RoomEnemySlot left);
        RoomEnemySystem rightSystem =
            CreateSkulteraProgramSystem(guard, startsLeft: false, out RoomEnemySlot right);

        AssertEqual(SkulteraInstructionProgramDefinitions.SwimmingLeft,
            left.CurrentInstruction, "Skultera initializer selects left-swimming program");
        AssertEqual(SkulteraInstructionProgramDefinitions.SwimmingRight,
            right.CurrentInstruction, "Skultera initializer selects right-swimming program");

        RunSkulteraProgram(leftSystem, left, frames: 45);
        RunSkulteraProgram(rightSystem, right, frames: 45);
        AssertEqual((ushort)2, left.Layer,
            "left-swimming Skultera program selects native layer two");
        AssertEqual((ushort)6, right.Layer,
            "right-swimming Skultera program selects native layer six");

        Action<RoomEnemySlot, SkulteraEnemyState, bool> beginTurn = SkulteraBehavior.BeginSkulteraTurn;
        SkulteraEnemyState leftState = leftSystem.SkulteraStates[0]!;
        SkulteraEnemyState rightState = rightSystem.SkulteraStates[0]!;
        beginTurn(left, leftState, true);
        beginTurn(right, rightState, false);
        AssertEqual(SkulteraInstructionProgramDefinitions.TurningRight,
            left.CurrentInstruction, "left swimmer enters turning-right program");
        AssertEqual(SkulteraInstructionProgramDefinitions.TurningLeft,
            right.CurrentInstruction, "right swimmer enters turning-left program");

        RunSkulteraProgram(leftSystem, left, frames: 80);
        RunSkulteraProgram(rightSystem, right, frames: 80);
        AssertTrue(leftState.TurnFinished,
            "turning-right program publishes Skultera completion flag");
        AssertTrue(rightState.TurnFinished,
            "turning-left program publishes Skultera completion flag");
        AssertEqual((ushort)0x905e, left.CurrentInstruction,
            "turning-right program sleeps at its native terminal command");
        AssertEqual((ushort)0x9094, right.CurrentInstruction,
            "turning-left program sleeps at its native terminal command");

        for (int index = 0;
             index < SkulteraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SkulteraInstructionProgramDefinitions.PresentationWordAddress(index);
            RoomEnemySystem enemies = CreateSkulteraProgramSystem(guard,
                startsLeft: true, out RoomEnemySlot slot);
            slot.CurrentInstruction = unchecked((ushort)(address - 2));
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0]);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa30000 | address)),
                slot.SpritemapPointer,
                $"production execution selects Skultera frame $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Skultera mechanics and visual bytes");

        AssertThrows<InvalidDataException>(
            () => SkulteraInstructionProgramDefinitions.ReadMechanicsWord(0x902e),
            "interleaved Skultera spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkulteraInstructionProgramDefinitions.ReadMechanicsWord(0x9096),
            "adjacent Skultera callback implementation is rejected as mechanics");

        _ = ProbeSkulteraInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSkulteraInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Skultera allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Skultera mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Skultera instruction mechanics: thirty-two compiled words, both swimming " +
            "loops, both turning programs, layer/completion callbacks, sleeps, and " +
            "twenty-two compiled visual selectors pass with source bytes forbidden.");

        static RoomEnemySystem CreateSkulteraProgramSystem(
            CartridgeReadGuard guard,
            bool startsLeft,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.SkulteraBehavior.InitializeSkultera;
            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Skultera;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.Parameter1 = startsLeft ? (ushort)0x0100 : (ushort)0x0000;
            slot.Parameter2 = 0x0120;
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0200));
            initialize(slot);
            slot.InstructionTimer = 1;
            return enemies;
        }

        static void RunSkulteraProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Probes skultera instruction mechanics allocation.</summary>
    private static int ProbeSkulteraInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SkulteraInstructionProgramDefinitions.ReadMechanicsWord(
                SkulteraInstructionProgramDefinitions.SwimmingLeft);
        }
        return checksum;
    }

    /// <summary>New skultera instruction program read guard.</summary>
    private static CartridgeReadGuard NewSkulteraInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(SkulteraInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Skultera mechanics byte ${address:X6}.")
            .ForbidWords(0xa30000, SkulteraInstructionProgramDefinitions.PresentationWordCount, SkulteraInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Production read compiled Skultera visual selector ${address:X6}.");

    /// <summary>Skultera instruction mechanics command.</summary>
    [VerificationCommand("--skultera-instruction-mechanics")]
    internal static int SkulteraInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySkulteraInstructionProgramDefinitions), () => VerifySkulteraInstructionProgramDefinitions());
        return 0;
    }
}
