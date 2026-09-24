using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNinjaSpacePirateInstructionProgramDefinitions()
    {
        VerifyNinjaSpacePirateInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyNinjaSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            NinjaSpacePirateInstructionMechanicsWord definition =
                NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNinjaPirateWord(rom, 0xb20000 | definition.Address),
                $"ninja Pirate mechanics word $B2:{definition.Address:X4}");
        }

        var guard = new NinjaSpacePirateInstructionReadGuard(rom);
        ushort[] programs =
        [
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackLeft,
            NinjaSpacePirateInstructionProgramDefinitions.SpinJumpLeft,
            NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.FlinchFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftJump,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftDive,
            NinjaSpacePirateInstructionProgramDefinitions.WalkToLeftPost,
            NinjaSpacePirateInstructionProgramDefinitions.InitialFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.LandFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.KickFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackRight,
            NinjaSpacePirateInstructionProgramDefinitions.SpinJumpRight,
            NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.FlinchFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickRightJump,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickRightDive,
            NinjaSpacePirateInstructionProgramDefinitions.WalkToRightPost,
            NinjaSpacePirateInstructionProgramDefinitions.InitialFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.LandFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.KickFacingRight,
        ];

        foreach (ushort program in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, 140);
        }

        VerifyClawAttack(
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackLeft,
            expectedFirstXOffset: -32);
        VerifyClawAttack(
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackRight,
            expectedFirstXOffset: 32);
        VerifyDive(
            NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftDive,
            NinjaSpacePirateFunction.DivekickLeftDive);
        VerifyDive(
            NinjaSpacePirateInstructionProgramDefinitions.DivekickRightDive,
            NinjaSpacePirateFunction.DivekickRightDive);

        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        for (int index = 0;
             index < NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
            var (enemies, slot, samus) = CreateSystem();
            slot.CurrentInstruction = unchecked((ushort)(address - 2));
            slot.InstructionTimer = 1;
            process.Invoke(enemies,
                [slot, samus, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            AssertEqual(ReadNinjaPirateWord(rom, 0xb20000 | address),
                slot.SpritemapPointer,
                $"production execution selects ninja Pirate frame $B2:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled ninja Pirate mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xf162),
            "ninja Pirate spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xf536),
            "adjacent ninja Pirate callback code is rejected as mechanics");

        _ = ProbeNinjaSpacePirateInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNinjaSpacePirateInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "ninja Pirate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed ninja Pirate mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Ninja Space Pirate instruction mechanics: 308 compiled words, all twenty " +
            "production programs, claw/palette/sound/function callbacks, and 140 " +
            "compiled frame selectors pass with source bytes forbidden.");

        void VerifyClawAttack(ushort program, int expectedFirstXOffset)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, 40);
            NinjaSpacePirateEnemyState state = enemies.NinjaSpacePirateStates[0]!;
            AssertEqual(1, state.SpawnedClawCount,
                $"ninja Pirate claw program $B2:{program:X4} first spawn count");
            RoomEnemyProjectileSlot claw = enemies.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(
                unchecked((ushort)(slot.XPosition + expectedFirstXOffset)),
                claw.XPosition,
                $"ninja Pirate claw program $B2:{program:X4} first spawn X");
        }

        void VerifyDive(ushort program, NinjaSpacePirateFunction function)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, 3);
            AssertEqual(function, enemies.NinjaSpacePirateStates[0]!.Function,
                $"ninja Pirate dive program $B2:{program:X4} function");
            AssertEqual((ushort)0x0e00, slot.PaletteIndex,
                $"ninja Pirate dive program $B2:{program:X4} palette");
            AssertEqual((ushort)0x0066, enemies.LastSpacePirateSoundEffect!.Value,
                $"ninja Pirate dive program $B2:{program:X4} sound");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)(() => 1));
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.GreyNinjaSpacePirateDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0100;
            slot.XRadius = 8;
            slot.YRadius = 16;
            slot.Parameter2 = 0x0080;
            typeof(RoomEnemySystem).GetMethod("InitializeNinjaSpacePirate", flags)!
                .Invoke(enemies, [slot]);
            return (enemies, slot, new SamusState { XPosition = 0x0080, YPosition = 0x0100 });
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, samus, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++) process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeNinjaSpacePirateInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingLeft);
        }
        return checksum;
    }

    private static ushort ReadNinjaPirateWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class NinjaSpacePirateInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (NinjaSpacePirateInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled ninja Pirate mechanics ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled ninja Pirate frame selector " +
                            $"${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
