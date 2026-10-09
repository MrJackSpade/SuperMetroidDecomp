using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the gunship dust instruction checks against the installed retail ROM.</summary>
    private static void VerifyGunshipDustInstructionProgramDefinitions() =>
        Suite(nameof(VerifyGunshipDustInstructionProgramDefinitions), () => VerifyGunshipDustInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>
    /// Compares compiled mechanics and visual selectors with the cartridge, then runs each
    /// liftoff dust program through production to verify its frames, duration, and deletion.
    /// </summary>
    /// <param name="rom">Retail address space supplying the reference instruction and selector words.</param>
    private static void VerifyGunshipDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < GunshipDustInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                GunshipDustInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Gunship dust mechanics word $86:{definition.Address:X4}");
        }

        var guard = new GunshipDustInstructionReadGuard(rom);
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnGunshipLiftoffDustCloud", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        for (int programIndex = 0;
             programIndex < GunshipDustInstructionProgramDefinitions.ProgramCount;
             programIndex++)
        {
            ushort parameter = unchecked((ushort)(programIndex * 2));
            GunshipDustInstructionProgramDefinition program =
                GunshipDustInstructionProgramDefinitions.Program(programIndex);
            RoomEnemySystem enemies = NewSystem();
            var samus = new SamusState { XPosition = 0x1234, YPosition = 0x5678 };
            spawn.Invoke(enemies, [parameter, samus]);
            RoomEnemyProjectileSlot dust = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind ==
                    RoomEnemyProjectileKind.GunshipLiftoffDustCloud);
            AssertEqual(
                GunshipDustInstructionProgramDefinitions.InitialForParameter(parameter),
                dust.InstructionPointer,
                "real gunship dust producer selects its named program");
            AssertEqual(program.Initial, dust.InstructionPointer,
                "gunship dust parameter and program catalogs agree");

            for (int frame = 0; frame < program.Durations.Length; frame++)
            {
                RunForcedTick(enemies, dust);
                AssertEqual(program.Durations[frame], dust.InstructionTimer,
                    $"gunship dust program {programIndex} frame {frame} duration");
                AssertEqual(
                    unchecked((ushort)(program.FirstFrame + (frame + 1) * 4)),
                    dust.InstructionPointer,
                    $"gunship dust program {programIndex} frame {frame} continuation");
            }

            AssertEqual((ushort)1, dust.GeneralTimer,
                $"gunship dust program {programIndex} initializes one animation pass");
            RunForcedTick(enemies, dust);
            AssertTrue(!dust.IsActive,
                $"gunship dust program {programIndex} exits its decrement branch and deletes");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count, "compiled gunship visuals require no cartridge reads");
        for (int index = 0;
             index < GunshipDustInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = GunshipDustInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "compiled gunship selector exists");
            AssertEqual(ReadVerificationWord(rom, (0x86 << 16) | address), selector,
                "compiled gunship selector matches original operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids gunship dust mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(0xa19d),
            "gunship dust spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(0xa2e2),
            "gunship dust selector-table data is rejected as mechanics");

        _ = ProbeGunshipDustInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeGunshipDustInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "gunship dust allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed gunship dust mechanics lookups allocate no storage");

        Console.WriteLine(
            "Gunship dust instruction mechanics: seventy-six compiled words, all six " +
            "real producers, frame loops, deletions, and forty-six native compiled spritemap operands pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            return enemies;
        }

        void RunForcedTick(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    /// <summary>
    /// Repeatedly resolves the two endpoint program words for the warmed mechanics lookup
    /// allocation check performed by the caller.
    /// </summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbeGunshipDustInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += GunshipDustInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? GunshipDustInstructionProgramDefinitions.Index0
                    : GunshipDustInstructionProgramDefinitions.IndexA);
        }
        return checksum;
    }

    /// <summary>
    /// Rejects reads from compiled gunship-dust mechanics and records presentation operands
    /// requested from the cartridge during production execution.
    /// </summary>
    /// <param name="source">Underlying address space for permitted reads and forwarded writes.</param>
    private sealed class GunshipDustInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the distinct native presentation operand addresses read from the cartridge.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets the number of rejected reads from compiled mechanics words.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-time read through the same mechanics read guard.</summary>
        /// <param name="address">Absolute cartridge address requested by the importer.</param>
        /// <returns>The source byte when the address is not compiled mechanics data.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects mechanics reads, records presentation-operand reads, and forwards other bytes.</summary>
        /// <param name="address">Absolute address requested by production code.</param>
        /// <returns>The source byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a compiled mechanics word.</exception>
        public byte ReadByte(int address)
        {
            if (GunshipDustInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled gunship dust mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < GunshipDustInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = GunshipDustInstructionProgramDefinitions
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

        /// <summary>Forwards a byte write unchanged to the wrapped address space.</summary>
        /// <param name="address">Absolute destination address.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
