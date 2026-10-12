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
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class StokeInstructionProgramDefinitionsVerification
{
    internal static void VerifyStokeInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyStokeInstructionProgramDefinitions), () => VerifyStokeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyStokeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < StokeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                StokeInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"Stoke instruction mechanics word $A2:{definition.Address:X4}");
        }

        for (int index = 0;
             index < StokeInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = StokeInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | address)),
                OwtchStokeVisualDefinitions.FrameAt(EnemyDefinitionId.Stoke, address),
                $"compiled Stoke frame selector $A2:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => OwtchStokeVisualDefinitions.FrameAt(EnemyDefinitionId.Stoke, 0x8934),
            "Stoke timing word is not a visual selector");

        var guard = NewStokeInstructionProgramReadGuard(rom);
        HashSet<ushort> observedFrames = [];
        Suite(nameof(VerifyWalking), () => VerifyWalking(
            StokeInstructionProgramDefinitions.MovingLeft,
            StokeDirection.Left,
            StokeAiFunction.MovingLeft,
            0x8938,
            "left"));
        Suite(nameof(VerifyWalking), () => VerifyWalking(
            StokeInstructionProgramDefinitions.MovingRight,
            StokeDirection.Right,
            StokeAiFunction.MovingRight,
            0x895e,
            "right"));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            StokeInstructionProgramDefinitions.AttackingLeft,
            StokeDirection.Left,
            StokeAiFunction.MovingLeft,
            0x8938,
            expectedProjectileDirection: 0,
            direction: "left"));
        Suite(nameof(VerifyAttack), () => VerifyAttack(
            StokeInstructionProgramDefinitions.AttackingRight,
            StokeDirection.Right,
            StokeAiFunction.MovingRight,
            0x895e,
            expectedProjectileDirection: 1,
            direction: "right"));

        AssertEqual(10, observedFrames.Count, "all Stoke walking/attack frames execute");
        foreach (ushort frame in new ushort[]
                 { 0x8aca, 0x8ad6, 0x8ae7, 0x8af3, 0x8aff,
                   0x8b15, 0x8b21, 0x8b32, 0x8b3e, 0x8b4a })
            AssertTrue(observedFrames.Contains(frame), $"Stoke live frame ${frame:X4}");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Stoke mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => StokeInstructionProgramDefinitions.ReadMechanicsWord(0x8936),
            "interleaved Stoke spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => StokeInstructionProgramDefinitions.ReadMechanicsWord(0x897e),
            "adjacent Stoke callback code is rejected as mechanics");

        _ = ProbeStokeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeStokeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Stoke allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Stoke mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Stoke instruction mechanics: twenty-six compiled words, both walking " +
            "loops, both attacks and real directional projectile spawns, and twelve " +
            "compiled visual selectors pass with source bytes forbidden.");

        void VerifyWalking(
            ushort program,
            StokeDirection expectedDirection,
            StokeAiFunction expectedFunction,
            ushort expectedLoopCursor,
            string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, StokeEnemyState state) =
                CreateStoke(expectedDirection);
            Install(slot, program);
            RunStokeProgram(enemies, slot, 41);
            AssertEqual(expectedDirection, state.Direction,
                $"Stoke {direction} walking callback direction");
            AssertEqual(expectedFunction, state.Function,
                $"Stoke {direction} walking callback function");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Stoke {direction} walking program returns through native goto");
        }

        void VerifyAttack(
            ushort program,
            StokeDirection expectedDirection,
            StokeAiFunction expectedFunction,
            ushort expectedLoopCursor,
            ushort expectedProjectileDirection,
            string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, StokeEnemyState state) =
                CreateStoke(expectedDirection);
            state.Function = StokeAiFunction.IdleDuringAttack;
            Install(slot, program);
            RunStokeProgram(enemies, slot, 33);

            AssertEqual(expectedDirection, state.Direction,
                $"Stoke {direction} attack restores direction");
            AssertEqual(expectedFunction, state.Function,
                $"Stoke {direction} attack restores walking function");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Stoke {direction} attack enters walking loop");
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(
                candidate => candidate.Kind == RoomEnemyProjectileKind.StokeProjectile);
            AssertEqual(expectedProjectileDirection, projectile.DirectionParameter,
                $"Stoke {direction} attack projectile direction");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, StokeEnemyState State) CreateStoke(
            StokeDirection direction)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.StokeBehavior.InitializeStoke;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Stoke;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = (ushort)direction;
            slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
            initialize(slot);
            return (enemies, slot, enemies.StokeStates[0]!);
        }

        static void Install(RoomEnemySlot slot, ushort program)
        {
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            slot.Timer = 0;
        }

        void RunStokeProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
            {
                EnemyInstructionRunner.Process(enemies, arguments);
                if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
                    observedFrames.Add(slot.SpritemapPointer);
            }
        }
    }

    private static int ProbeStokeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += StokeInstructionProgramDefinitions.ReadMechanicsWord(
                StokeInstructionProgramDefinitions.MovingLeft);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewStokeInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(StokeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Stoke instruction byte ${address:X6}.")
            .ForbidWords(0xa20000, StokeInstructionProgramDefinitionsTooling.PresentationWordCount, StokeInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read compiled Stoke instruction byte ${address:X6}.");

    [VerificationCommand("--stoke-instruction-mechanics")]
    internal static int StokeInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyStokeInstructionProgramDefinitions), () => VerifyStokeInstructionProgramDefinitions());
        return 0;
    }
}
