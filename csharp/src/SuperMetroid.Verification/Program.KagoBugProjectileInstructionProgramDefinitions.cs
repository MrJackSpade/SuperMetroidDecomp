using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the cartridge-backed checks for Kago-bug projectile programs and mechanics.</summary>
    private static void VerifyKagoBugProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKagoBugProjectileInstructionProgramDefinitions), () => VerifyKagoBugProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics and exercises the production Kago producer, animation loops, and shot/drop path.</summary>
    /// <param name="rom">Retail address space used to compare mechanics and presentation-selector words.</param>
    private static void VerifyKagoBugProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKagoBugProjectileInstructionWord(rom, definition.Address),
                $"Kago-bug projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new KagoBugProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0x0002));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0x0080));
        typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
            enemies,
            new SamusState { Health = 99, MaxHealth = 99 });

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod("SpawnKagoBug", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 128;
        source.YPosition = 112;
        source.VramTilesIndex = 0x0200;
        source.PaletteIndex = 0x0c00;
        AssertTrue((bool)spawn.Invoke(enemies, [source])!,
            "real Kago producer allocates its bug");

        RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
            candidate => candidate.Kind == RoomEnemyProjectileKind.KagoBug);
        AssertEqual(KraidRockProjectileInstructionProgramDefinitions.SharedRockAndKagoBug,
            projectile.InstructionPointer,
            "real Kago producer retains the cartridge's shared Kraid-rock initial pose");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Landed;
        RunForcedTicks(projectile, 3);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.Landed + 4)),
            projectile.InstructionPointer,
            "Kago landed program runs its callback and loops");
        AssertEqual(
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Idle,
            projectile.PreInstruction,
            "Kago landed callback restores idle movement");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Falling;
        RunForcedTicks(projectile, 2);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.Falling + 4)),
            projectile.InstructionPointer,
            "Kago falling pose completes its loop");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.JumpStart;
        RunForcedTicks(projectile, 3);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.JumpLoop + 4)),
            projectile.InstructionPointer,
            "Kago jump introduction reaches the looping airborne pose");
        AssertEqual(
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Jumping,
            projectile.PreInstruction,
            "Kago jump callback installs airborne movement");
        RunForcedTicks(projectile, 1);
        AssertEqual(unchecked((ushort)(
                KagoBugProjectileInstructionProgramDefinitions.JumpLoop + 4)),
            projectile.InstructionPointer,
            "Kago airborne pose loops");

        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Shot;
        projectile.GraphicsIndex = 0xffff;
        RunForcedTicks(projectile, 1);
        AssertEqual((ushort)0, projectile.GraphicsIndex,
            "Kago shot program applies its palette-zero callback");
        RunForcedTicks(projectile, 4);
        AssertEqual((ushort)0xd07a, projectile.InstructionPointer,
            "Kago shot program displays all five explosion frames");
        RunForcedTicks(projectile, 1);
        AssertTrue(!projectile.IsActive,
            "Kago shot program requests its drop and reaches shared deletion");
        AssertTrue(enemies.LastKagoBugDropRequest is not null,
            "Kago shot program emits its production drop request before deletion");

        AssertEqual(0, guard.ObservedPresentationWords.Count, "compiled Kago bug visuals require no cartridge reads");
        for (int index = 0;
             index < KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KagoBugProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "compiled Kago bug selector exists");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "compiled Kago bug selector matches original operand");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Kago, shared initial-pose, and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => KagoBugProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd03e),
            "Kago-bug spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KagoBugProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd080),
            "unreferenced duplicate delete list is rejected as Kago-bug mechanics");

        _ = ProbeKagoBugProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKagoBugProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kago-bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kago-bug projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kago-bug projectile instruction mechanics: twenty-three private words, " +
            "the real producer, landed/falling/jump loops, complete shot/drop deletion, " +
            "and eleven native compiled spritemap operands pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot target, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                target.InstructionTimer = 1;
                process.Invoke(enemies, [target, new SamusState(), (ushort)0, (ushort)0]);
            }
        }
    }

    /// <summary>Repeatedly reads compiled landed and shot mechanics words for the warmed allocation probe.</summary>
    /// <returns>A checksum consumed by the caller to keep the lookup loop observable.</returns>
    private static int ProbeKagoBugProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KagoBugProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KagoBugProjectileInstructionProgramDefinitions.Landed
                    : KagoBugProjectileInstructionProgramDefinitions.Shot);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian projectile-program word from the cartridge bank.</summary>
    /// <param name="source">Address space containing the native word.</param>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <returns>The combined sixteen-bit instruction or operand word.</returns>
    private static ushort ReadKagoBugProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Address-space proxy that observes Kago presentation operands and rejects reads of compiled projectile mechanics.</summary>
    /// <param name="source">Underlying bus for permitted reads and forwarded writes.</param>
    private sealed class KagoBugProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation word addresses observed through projectile-bank reads.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempted reads rejected because they target Kago, shared rock, or common projectile mechanics.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge imports through the mechanics guard used by generic reads.</summary>
        /// <param name="address">Cartridge address being imported.</param>
        /// <returns>The permitted byte from the wrapped source.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled projectile mechanics reads, records presentation operands, and forwards other reads.</summary>
        /// <param name="address">Address requested by production code.</param>
        /// <returns>The byte from the underlying source when the address is permitted.</returns>
        public byte ReadByte(int address)
        {
            if (KagoBugProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                KraidRockProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kago-bug projectile mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = KagoBugProjectileInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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

        /// <summary>Forwards writes to the underlying cartridge address space.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
