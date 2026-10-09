using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and verifies compiled Magdollite mechanics and representative instruction programs.</summary>
    private static void VerifyMagdolliteInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMagdolliteInstructionProgramDefinitions), () => VerifyMagdolliteInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares bank-A8 mechanics with the ROM and executes the head, pillar, hand, and lava-related programs.</summary>
    /// <param name="rom">Retail address space containing Magdollite instruction words and presentation selectors.</param>
    private static void VerifyMagdolliteInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < MagdolliteInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MagdolliteInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMagdolliteInstructionWord(rom, 0xa80000 | definition.Address),
                $"Magdollite instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new MagdolliteInstructionProgramReadGuard(rom);
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteInstructionProgramDefinitions.LeftIdle, "left"));
        Suite(nameof(VerifyIdle), () => VerifyIdle(MagdolliteInstructionProgramDefinitions.RightIdle, "right"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteInstructionProgramDefinitions.LeftThrow, "left"));
        Suite(nameof(VerifyThrow), () => VerifyThrow(MagdolliteInstructionProgramDefinitions.RightThrow, "right"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteInstructionProgramDefinitions.LeftSubmerge, "left"));
        Suite(nameof(VerifySubmerge), () => VerifySubmerge(MagdolliteInstructionProgramDefinitions.RightSubmerge, "right"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteInstructionProgramDefinitions.LeftEmerge, "left"));
        Suite(nameof(VerifyEmerge), () => VerifyEmerge(MagdolliteInstructionProgramDefinitions.RightEmerge, "right"));

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
        Suite(nameof(VerifyStaticProgram), () => VerifyStaticProgram(
            MagdolliteInstructionProgramDefinitions.PillarCap,
            MagdollitePart.TrackingOverlay,
            "pillar cap"));

        for (int index = 0;
             index < MagdolliteInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                MagdolliteInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadMagdolliteInstructionWord(rom, 0xa80000 | address),
                EnemySpritemapDefinitions.MagdolliteFrameAt(address),
                $"compiled Magdollite presentation $A8:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Magdollite mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(0xac9e),
            "Magdollite spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(0xae12),
            "adjacent Magdollite callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.MagdolliteFrameAt(0xae12),
            "adjacent Magdollite callback code is rejected as presentation");

        _ = ProbeMagdolliteInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMagdolliteInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Magdollite allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Magdollite mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Magdollite instruction mechanics: " +
            $"{MagdolliteInstructionProgramDefinitionsTooling.MechanicsWordCount} compiled words, " +
            "seventeen complete head/pillar/hand programs, six real lava spawns, and " +
            $"{MagdolliteInstructionProgramDefinitionsTooling.PresentationWordCount} compiled " +
            "visual selections pass with both source classes forbidden.");

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

    /// <summary>Repeatedly reads the left-idle entry word for the warmed-allocation check.</summary>
    /// <returns>A checksum that keeps the mechanics lookups observable.</returns>
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

    /// <summary>Reads a little-endian instruction word from the supplied bus address.</summary>
    /// <param name="bus">Retail address space containing the instruction bytes.</param>
    /// <param name="address">Bus address of the low byte.</param>
    /// <returns>The word formed by the addressed byte and its successor.</returns>
    private static ushort ReadMagdolliteInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects production reads of compiled Magdollite mechanics and presentation-selector bytes.</summary>
    /// <param name="source">Wrapped address space used for reads and writes outside the compiled instruction data.</param>
    private sealed class MagdolliteInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets attempts to read a byte belonging to a compiled mechanics or presentation word.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-byte request through the forbidden-range guard.</summary>
        /// <param name="address">Cartridge bus address to read.</param>
        /// <returns>The byte returned by the wrapped source when allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled instruction-data reads before forwarding other byte requests.</summary>
        /// <param name="address">Bus address of the requested byte.</param>
        /// <returns>The byte returned by the wrapped source when the address is allowed.</returns>
        public byte ReadByte(int address)
        {
            if (MagdolliteInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                IsPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Magdollite instruction byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Identifies either byte of a compiled Magdollite presentation-word operand in bank A8.</summary>
        /// <param name="address">Bus address of the byte to classify.</param>
        /// <returns><see langword="true"/> when the byte belongs to a presentation word.</returns>
        private static bool IsPresentationByte(int address) =>
            (address & 0xff0000) == 0xa80000 &&
            (MagdolliteInstructionProgramDefinitions.IsPresentationWord(
                unchecked((ushort)address)) ||
             MagdolliteInstructionProgramDefinitions.IsPresentationWord(
                unchecked((ushort)(address - 1))));

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte written at <paramref name="address"/>.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
