using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMagdolliteInstructionProgramDefinitions()
    {
        VerifyMagdolliteInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyMagdolliteInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < MagdolliteInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MagdolliteInstructionMechanicsWord definition =
                MagdolliteInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMagdolliteInstructionWord(rom, 0xa80000 | definition.Address),
                $"Magdollite instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new MagdolliteInstructionProgramReadGuard(rom);
        VerifyIdle(MagdolliteInstructionProgramDefinitions.LeftIdle, "left");
        VerifyIdle(MagdolliteInstructionProgramDefinitions.RightIdle, "right");
        VerifyThrow(MagdolliteInstructionProgramDefinitions.LeftThrow, "left");
        VerifyThrow(MagdolliteInstructionProgramDefinitions.RightThrow, "right");
        VerifySubmerge(MagdolliteInstructionProgramDefinitions.LeftSubmerge, "left");
        VerifySubmerge(MagdolliteInstructionProgramDefinitions.RightSubmerge, "right");
        VerifyEmerge(MagdolliteInstructionProgramDefinitions.LeftEmerge, "left");
        VerifyEmerge(MagdolliteInstructionProgramDefinitions.RightEmerge, "right");

        ushort[] pillarPrograms =
        [
            MagdolliteInstructionProgramDefinitions.PillarPhase0,
            MagdolliteInstructionProgramDefinitions.PillarPhase1,
            MagdolliteInstructionProgramDefinitions.PillarPhase2,
            MagdolliteInstructionProgramDefinitions.PillarPhase3,
            MagdolliteInstructionProgramDefinitions.PillarPhase4,
            MagdolliteInstructionProgramDefinitions.PillarPhase5,
            MagdolliteInstructionProgramDefinitions.PillarPhase6,
            MagdolliteInstructionProgramDefinitions.PillarPhase7,
        ];
        for (int phase = 0; phase < pillarPrograms.Length; phase++)
            VerifyStaticProgram(pillarPrograms[phase], MagdollitePart.RisingBody, $"pillar {phase}");
        VerifyStaticProgram(
            MagdolliteInstructionProgramDefinitions.PillarCap,
            MagdollitePart.TrackingOverlay,
            "pillar cap");

        AssertEqual(MagdolliteInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Magdollite spritemap words remain cartridge reads");
        for (int index = 0;
             index < MagdolliteInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                MagdolliteInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Magdollite presentation $A8:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Magdollite mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(0xac9e),
            "Magdollite spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(0xae12),
            "adjacent Magdollite callback code is rejected as mechanics");

        _ = ProbeMagdolliteInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMagdolliteInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Magdollite allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Magdollite mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Magdollite instruction mechanics: " +
            $"{MagdolliteInstructionProgramDefinitions.MechanicsWordCount} compiled words, " +
            "seventeen complete head/pillar/hand programs, six real lava spawns, and " +
            $"{MagdolliteInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap reads pass with mechanics bytes forbidden.");

        void VerifyIdle(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                CreateSystem(MagdollitePart.Head);
            Install(slot, program);
            RunFrames(enemies, slot, 55);
            AssertTrue(slot.CurrentInstruction >= program &&
                slot.CurrentInstruction <= unchecked((ushort)(program + 0x10)),
                $"Magdollite {direction} idle remains inside its loop");
        }

        void VerifyThrow(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.TrackingOverlay);
            state.ThrowOriginX = slot.XPosition;
            state.ThrowOriginY = slot.YPosition;
            Install(slot, program);
            RunFrames(enemies, slot, 80);
            AssertEqual((ushort)0x0061, enemies.LastMagdolliteSoundEffect!.Value,
                $"Magdollite {direction} throw sound");
            AssertEqual(3,
                enemies.EnemyProjectiles.Count(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite),
                $"Magdollite {direction} throw projectile count");
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} throw releases animation wait");
        }

        void VerifySubmerge(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            Install(head, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} submerge releases animation wait");
            AssertTrue(!enemies.Slots[1].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} submerge reveals pillar");
            AssertTrue(!enemies.Slots[2].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} submerge reveals hand");
        }

        void VerifyEmerge(ushort program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, MagdolliteEnemyState state) =
                CreateSystem(MagdollitePart.Head);
            enemies.Slots[1].Properties =
                enemies.Slots[1].Properties.Without(EnemyProperties.Invisible);
            enemies.Slots[2].Properties =
                enemies.Slots[2].Properties.Without(EnemyProperties.Invisible);
            Install(head, program);
            RunFrames(enemies, head, 80);
            AssertTrue(!state.AnimationBusy,
                $"Magdollite {direction} emerge releases animation wait");
            AssertTrue(enemies.Slots[1].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} emerge hides pillar");
            AssertTrue(enemies.Slots[2].Properties.HasAny(EnemyProperties.Invisible),
                $"Magdollite {direction} emerge hides hand");
        }

        void VerifyStaticProgram(ushort program, MagdollitePart part, string name)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) = CreateSystem(part);
            Install(slot, program);
            RunFrames(enemies, slot, 3);
            AssertEqual(unchecked((ushort)(program + 4)), slot.CurrentInstruction,
                $"Magdollite {name} reaches terminal sleep");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, MagdolliteEnemyState State) CreateSystem(
            MagdollitePart selectedPart)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeMagdollite", flags)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState?>>(enemies);
            for (int index = 0; index < 3; index++)
            {
                RoomEnemySlot member = enemies.Slots[index];
                member.EnemyDefinitionPointer = RoomEnemySystem.MagdolliteDefinition;
                member.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
                member.Parameter1 = (ushort)index;
                member.Parameter2 = 0;
                member.XPosition = 0x0080;
                member.YPosition = 0x0080;
                initialize(member, null);
            }

            RoomEnemySlot selected = enemies.Slots[(int)selectedPart];
            return (enemies, selected, enemies.MagdolliteStates[(int)selectedPart]!);
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

    private static int ProbeMagdolliteInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(
                MagdolliteInstructionProgramDefinitions.LeftIdle);
        }
        return checksum;
    }

    private static ushort ReadMagdolliteInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class MagdolliteInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MagdolliteInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Magdollite mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MagdolliteInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MagdolliteInstructionProgramDefinitions.PresentationWordAddress(index);
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
