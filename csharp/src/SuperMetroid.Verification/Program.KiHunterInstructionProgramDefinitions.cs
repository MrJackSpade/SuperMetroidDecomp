using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKiHunterInstructionProgramDefinitions()
    {
        VerifyKiHunterInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKiHunterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < KiHunterInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KiHunterInstructionMechanicsWord definition =
                KiHunterInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKiHunterInstructionWord(rom, 0xa80000 | definition.Address),
                $"KiHunter instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new KiHunterInstructionProgramReadGuard(rom);
        VerifyFlying(KiHunterInstructionProgramDefinitions.FlyingLeft, movingRight: false);
        VerifyFlying(KiHunterInstructionProgramDefinitions.FlyingRight, movingRight: true);
        VerifySwoop(KiHunterInstructionProgramDefinitions.SwoopLeft, movingRight: false);
        VerifySwoop(KiHunterInstructionProgramDefinitions.SwoopRight, movingRight: true);
        VerifyWingLoop(KiHunterInstructionProgramDefinitions.WingsLeft, "left");
        VerifyWingLoop(KiHunterInstructionProgramDefinitions.WingsRight, "right");
        VerifyDetachedWings();
        VerifyJump(KiHunterInstructionProgramDefinitions.JumpLeft, "left");
        VerifyJump(KiHunterInstructionProgramDefinitions.JumpRight, "right");
        VerifyLanding(KiHunterInstructionProgramDefinitions.LandLeft, "left");
        VerifyLanding(KiHunterInstructionProgramDefinitions.LandRight, "right");
        VerifySpit(KiHunterInstructionProgramDefinitions.SpitLeft, movingRight: false);
        VerifySpit(KiHunterInstructionProgramDefinitions.SpitRight, movingRight: true);

        AssertEqual(KiHunterInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live KiHunter spritemap words remain cartridge reads");
        for (int index = 0;
             index < KiHunterInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KiHunterInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads KiHunter presentation $A8:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled KiHunter mechanics bytes");
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
            $"{KiHunterInstructionProgramDefinitions.MechanicsWordCount} compiled words, " +
            "thirteen complete body/wing programs, two real acid spawns, and " +
            $"{KiHunterInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap reads pass with mechanics bytes forbidden.");

        void VerifyFlying(ushort program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            state.HorizontalVelocity = movingRight ? (ushort)1 : ushort.MaxValue;
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
            state.HorizontalVelocity = movingRight ? (ushort)1 : ushort.MaxValue;
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
            AssertEqual((ushort)24, state.WaitTimer,
                $"KiHunter {(movingRight ? "right" : "left")} spit wait timer");
            AssertEqual((ushort)0x004c, enemies.LastKiHunterSoundEffect!.Value,
                $"KiHunter {(movingRight ? "right" : "left")} spit sound");
            RoomEnemyProjectileKind kind = movingRight
                ? RoomEnemyProjectileKind.KiHunterAcidSpitRight
                : RoomEnemyProjectileKind.KiHunterAcidSpitLeft;
            AssertEqual(1, enemies.EnemyProjectiles.Count(projectile => projectile.Kind == kind),
                $"KiHunter {(movingRight ? "right" : "left")} acid spawn");
            AssertEqual(unchecked((ushort)(program + 0x1c)), body.CurrentInstruction,
                $"KiHunter {(movingRight ? "right" : "left")} spit reaches terminal sleep");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Body, RoomEnemySlot Wings,
            KiHunterEnemyState BodyState, KiHunterEnemyState WingState) CreatePair()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initializeBody = typeof(RoomEnemySystem).GetMethod("InitializeKiHunter", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var initializeWings = typeof(RoomEnemySystem).GetMethod(
                "InitializeKiHunterWings", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);

            RoomEnemySlot body = enemies.Slots[0];
            body.EnemyDefinitionPointer = RoomEnemySystem.KiHunterDefinition;
            body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            body.XPosition = 0x0080;
            body.YPosition = 0x0080;
            RoomEnemySlot wings = enemies.Slots[1];
            wings.EnemyDefinitionPointer = RoomEnemySystem.KiHunterWingsDefinition;
            wings.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            initializeBody(body);
            initializeWings(wings);
            return (enemies, body, wings,
                enemies.KiHunterStates[0]!, enemies.KiHunterStates[1]!);
        }

        static void Install(RoomEnemySlot slot, ushort program)
        {
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            slot.Timer = 0;
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments = [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

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

    private static ushort ReadKiHunterInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class KiHunterInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KiHunterInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled KiHunter mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KiHunterInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KiHunterInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
