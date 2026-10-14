using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Verification.Harness;

/// <summary>Checks KiHunter body and wing instruction programs, acid spawning, and compiled visual selectors.</summary>
internal static class KiHunterInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle for KiHunter instruction verification.</summary>
    internal static void VerifyKiHunterInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises thirteen flight, swoop, wing, ground, and spit lists while forbidding migrated bank-$A8 reads.</summary>
    internal static void VerifyKiHunterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < KiHunterInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KiHunterInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa80000 | definition.Address)),
                $"KiHunter instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = NewKiHunterInstructionProgramGuard(rom);
        Suite(nameof(VerifyFlying), () => VerifyFlying(KiHunterInstructionProgramDefinitions.FlyingLeft, movingRight: false));
        Suite(nameof(VerifyFlying), () => VerifyFlying(KiHunterInstructionProgramDefinitions.FlyingRight, movingRight: true));
        Suite(nameof(VerifySwoop), () => VerifySwoop(KiHunterInstructionProgramDefinitions.SwoopLeft, movingRight: false));
        Suite(nameof(VerifySwoop), () => VerifySwoop(KiHunterInstructionProgramDefinitions.SwoopRight, movingRight: true));
        Suite(nameof(VerifyWingLoop), () => VerifyWingLoop(KiHunterInstructionProgramDefinitions.WingsLeft, "left"));
        Suite(nameof(VerifyWingLoop), () => VerifyWingLoop(KiHunterInstructionProgramDefinitions.WingsRight, "right"));
        Suite(nameof(VerifyDetachedWings), () => VerifyDetachedWings());
        Suite(nameof(VerifyJump), () => VerifyJump(KiHunterInstructionProgramDefinitions.JumpLeft, "left"));
        Suite(nameof(VerifyJump), () => VerifyJump(KiHunterInstructionProgramDefinitions.JumpRight, "right"));
        Suite(nameof(VerifyLanding), () => VerifyLanding(KiHunterInstructionProgramDefinitions.LandLeft, "left"));
        Suite(nameof(VerifyLanding), () => VerifyLanding(KiHunterInstructionProgramDefinitions.LandRight, "right"));
        Suite(nameof(VerifySpit), () => VerifySpit(KiHunterInstructionProgramDefinitions.SpitLeft, movingRight: false));
        Suite(nameof(VerifySpit), () => VerifySpit(KiHunterInstructionProgramDefinitions.SpitRight, movingRight: true));

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled KiHunter mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => KiHunterInstructionProgramDefinitions.ReadMechanicsWord(0xe9fc),
            "KiHunter spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KiHunterInstructionProgramDefinitions.ReadMechanicsWord(0xeb2e),
            "adjacent KiHunter spritemap data is rejected as mechanics");

        _ = ProbeKiHunterInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKiHunterInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "KiHunter allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed KiHunter mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"KiHunter instruction mechanics: " +
            $"{KiHunterInstructionProgramDefinitionsTooling.MechanicsWordCount} compiled words, " +
            "thirteen complete body/wing programs, two real acid spawns, and " +
            $"{KiHunterInstructionProgramDefinitions.PresentationWordCount} compiled " +
            "visual selectors pass with cartridge reads forbidden.");

        void VerifyFlying(ushort program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            state.Velocity = state.Velocity.WithWholeX(movingRight ? (short)1 : (short)-1);
            Install(body, program);
            RunFrames(enemies, body, 7);
            AssertTrue(body.CurrentInstruction >= program &&
                body.CurrentInstruction <= unchecked((ushort)(program + 0x0c)),
                $"KiHunter {(movingRight ? "right" : "left")} flight remains in steady list");
        }

        void VerifySwoop(ushort program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            state.Velocity = state.Velocity.WithWholeX(movingRight ? (short)1 : (short)-1);
            Install(body, program);
            RunFrames(enemies, body, 55);
            ushort idle = movingRight
                ? KiHunterInstructionProgramDefinitions.FlyingRight
                : KiHunterInstructionProgramDefinitions.FlyingLeft;
            AssertTrue(body.CurrentInstruction >= idle &&
                body.CurrentInstruction <= unchecked((ushort)(idle + 0x0c)),
                $"KiHunter {(movingRight ? "right" : "left")} swoop returns to flight");
        }

        void VerifyWingLoop(ushort program, string direction)
        {
            (RoomEnemySystem enemies, _, RoomEnemySlot wings, _, _) = CreatePair();
            Install(wings, program);
            RunFrames(enemies, wings, 7);
            AssertTrue(wings.CurrentInstruction >= program &&
                wings.CurrentInstruction <= unchecked((ushort)(program + 0x0c)),
                $"KiHunter {direction} wing loop remains bounded");
        }

        void VerifyDetachedWings()
        {
            (RoomEnemySystem enemies, _, RoomEnemySlot wings, _, _) = CreatePair();
            Install(wings, KiHunterInstructionProgramDefinitions.DetachedWings);
            RunFrames(enemies, wings, 3);
            AssertEqual(unchecked((ushort)(KiHunterInstructionProgramDefinitions.DetachedWings + 4)),
                wings.CurrentInstruction,
                "KiHunter detached wings reach terminal sleep");
        }

        void VerifyJump(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            Install(body, program);
            RunFrames(enemies, body, 40);
            AssertEqual(KiHunterEnemyFunction.GroundJump, state.Function,
                $"KiHunter {direction} jump installs ground-jump owner");
            AssertEqual(unchecked((ushort)(program + 0x1a)), body.CurrentInstruction,
                $"KiHunter {direction} jump reaches terminal sleep");
        }

        void VerifyLanding(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            Install(body, program);
            RunFrames(enemies, body, 45);
            AssertEqual(KiHunterEnemyFunction.GroundWait, state.Function,
                $"KiHunter {direction} landing installs ground-wait owner");
            AssertEqual(unchecked((ushort)(program + 0x16)), body.CurrentInstruction,
                $"KiHunter {direction} landing reaches terminal sleep");
        }

        void VerifySpit(ushort program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            Install(body, program);
            RunFrames(enemies, body, 90);
            AssertEqual(KiHunterEnemyFunction.GroundWait, state.Function,
                $"KiHunter {(movingRight ? "right" : "left")} spit restores ground wait");
            AssertEqual((ushort)24, state.WaitTimer.RawValue,
                $"KiHunter {(movingRight ? "right" : "left")} spit wait timer");
            AssertEqual((ushort)0x004c, (enemies.KiHunterBehavior.LastKiHunterSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.KiHunterBehavior.LastKiHunterSoundEffect.")),
                $"KiHunter {(movingRight ? "right" : "left")} spit sound");
            RoomEnemyProjectileKind kind = movingRight
                ? RoomEnemyProjectileKind.KiHunterAcidSpitRight
                : RoomEnemyProjectileKind.KiHunterAcidSpitLeft;
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == kind),
                $"KiHunter {(movingRight ? "right" : "left")} acid spawn");
            AssertEqual(unchecked((ushort)(program + 0x1c)), body.CurrentInstruction,
                $"KiHunter {(movingRight ? "right" : "left")} spit reaches terminal sleep");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Body, RoomEnemySlot Wings,
            KiHunterEnemyState BodyState, KiHunterEnemyState WingState) CreatePair()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initializeBody = enemies.KiHunterBehavior.InitializeKiHunter;
            Action<RoomEnemySlot> initializeWings = enemies.KiHunterBehavior.InitializeKiHunterWings;

            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            body.EnemyDefinitionPointer = EnemyDefinitionId.KihunterGreen;
            body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            body.Position = body.Position.WithWhole(new(0x0080, 0x0080));
            RoomEnemySlot wings = enemies.SlotTable.Slots[1];
            wings.EnemyDefinitionPointer = EnemyDefinitionId.KihunterGreenWings;
            wings.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            initializeBody(body);
            initializeWings(wings);
            return (enemies, body, wings,
                (enemies.KiHunterStates[0] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter state.")), (enemies.KiHunterStates[1] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter state.")));
        }

        static void Install(RoomEnemySlot slot, ushort program)
        {
            slot.CurrentInstruction = program;
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.Timer = NativeCountdown.Start(0);
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Consumes warmed KiHunter mechanics words for a per-frame allocation check.</summary>
    private static int ProbeKiHunterInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KiHunterInstructionProgramDefinitions.ReadMechanicsWord(
                KiHunterInstructionProgramDefinitions.FlyingLeft);
        }
        return checksum;
    }

    /// <summary>Rejects runtime cartridge reads of compiled mechanics and visual selector words.</summary>
    private static CartridgeReadGuard NewKiHunterInstructionProgramGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(KiHunterInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled KiHunter mechanics ${address:X6}.")
            .ForbidWords(0xa80000, KiHunterInstructionProgramDefinitions.PresentationWordCount,
                KiHunterInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Production read compiled KiHunter visual byte ${address:X6}.");

    /// <summary>Runs the KiHunter instruction check against the local retail cartridge image.</summary>
    [VerificationCommand("--kihunter-instruction-mechanics")]
    internal static int KihunterInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions());
        return 0;
    }
}
