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
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using static SuiteTimingVerification;

/// <summary>Represents cacatac instruction program definitions verification.</summary>
internal static class CacatacInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies cacatac instruction program definitions.</summary>
    internal static void VerifyCacatacInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCacatacInstructionProgramDefinitions), () => VerifyCacatacInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies cacatac instruction program definitions.</summary>
    internal static void VerifyCacatacInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < CacatacInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CacatacInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"Cacatac instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = NewCacatacInstructionProgramReadGuard(rom);
        Suite(nameof(VerifyIdle), () => VerifyIdle(upsideUp: true, CacatacInstructionProgramDefinitions.UpsideUpIdle));
        Suite(nameof(VerifyIdle), () => VerifyIdle(upsideUp: false, CacatacInstructionProgramDefinitions.UpsideDownIdle));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            upsideUp: true,
            CacatacInstructionProgramDefinitions.UpsideUpAttack,
            [
                CacatacSpikeDirection.LeftFacingUp,
                CacatacSpikeDirection.UpLeft,
                CacatacSpikeDirection.Up,
                CacatacSpikeDirection.UpRight,
                CacatacSpikeDirection.RightFacingUp,
            ]));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            upsideUp: false,
            CacatacInstructionProgramDefinitions.UpsideDownAttack,
            [
                CacatacSpikeDirection.LeftFacingDown,
                CacatacSpikeDirection.DownLeft,
                CacatacSpikeDirection.Down,
                CacatacSpikeDirection.DownRight,
                CacatacSpikeDirection.RightFacingDown,
            ]));

        for (int index = 0;
             index < CacatacInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = CacatacInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | address)),
                EnemySpritemapDefinitions.CacatacFrameAt(address),
                $"compiled Cacatac visual selector $A2:{address:X4} matches cartridge");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Cacatac mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => CacatacInstructionProgramDefinitions.ReadMechanicsWord(0x9e8e),
            "Cacatac spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CacatacInstructionProgramDefinitions.ReadMechanicsWord(0x9f2a),
            "adjacent Cacatac callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.CacatacFrameAt(0x9f30),
            "uncompiled Cacatac visual selector is rejected loudly");

        _ = ProbeCacatacInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCacatacInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Cacatac allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Cacatac mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Cacatac instruction mechanics: fifty-six compiled words, four complete " +
            "idle/attack programs, ten real spike spawns, and twenty-four compiled visual " +
            "selectors pass with source bytes forbidden.");

        void VerifyIdle(bool upsideUp, ushort program)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) =
                CreateSystem(upsideUp);
            RunFrames(enemies, slot, 65);
            AssertEqual(unchecked((ushort)(program + 6)), slot.CurrentInstruction,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} idle loops");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} idle restores patrol");
        }

        void VerifyAttack(
            bool upsideUp,
            ushort program,
            CacatacSpikeDirection[] expectedDirections)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) =
                CreateSystem(upsideUp);
            state.Function = CacatacEnemyFunction.Stopped;
            CacatacBehavior.SetCacatacInstructionList(slot, program);
            RunFrames(enemies, slot, 53);

            ushort idle = upsideUp
                ? CacatacInstructionProgramDefinitions.UpsideUpIdle
                : CacatacInstructionProgramDefinitions.UpsideDownIdle;
            AssertEqual(unchecked((ushort)(idle + 6)), slot.CurrentInstruction,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack returns to idle");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack resumes patrol");
            AssertEqual((ushort)0x0034, enemies.CacatacBehavior.LastCacatacSoundEffect!.Value,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack queues sound");

            ushort[] actualDirections = enemies.Projectiles.EnemyProjectiles
                .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike)
                .Select(projectile => projectile.Variable0)
                .Order()
                .ToArray();
            ushort[] expected = expectedDirections.Select(value => (ushort)value).Order().ToArray();
            AssertEqual(expected.Length, actualDirections.Length,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} spike count");
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual(expected[index], actualDirections[index],
                    $"Cacatac {(upsideUp ? "upright" : "inverted")} spike {index}");
            }
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, CacatacEnemyState State) CreateSystem(
            bool upsideUp)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.CacatacBehavior.InitializeCacatac;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Cacatac;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = upsideUp ? (ushort)0x0100 : (ushort)0;
            initialize(slot);
            return (enemies, slot, enemies.CacatacStates[0]!);
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Probes cacatac instruction mechanics allocation.</summary>
    private static int ProbeCacatacInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CacatacInstructionProgramDefinitions.ReadMechanicsWord(
                CacatacInstructionProgramDefinitions.UpsideUpIdle);
        }
        return checksum;
    }

    /// <summary>New cacatac instruction program read guard.</summary>
    private static CartridgeReadGuard NewCacatacInstructionProgramReadGuard(ISnesAddressSpace source)
    {
        static string Message(int address) => $"Production read compiled Cacatac instruction ${address:X6}.";
        return new CartridgeReadGuard(source)
            .Forbid(CacatacInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte, Message)
            .ForbidWords(0xa20000, CacatacInstructionProgramDefinitionsTooling.PresentationWordCount, CacatacInstructionProgramDefinitionsTooling.PresentationWordAddress, Message);
    }

    /// <summary>Cacatac instruction mechanics command.</summary>
    [VerificationCommand("--cacatac-instruction-mechanics")]
    internal static int CacatacInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCacatacInstructionProgramDefinitions), () => VerifyCacatacInstructionProgramDefinitions());
        return 0;
    }
}
