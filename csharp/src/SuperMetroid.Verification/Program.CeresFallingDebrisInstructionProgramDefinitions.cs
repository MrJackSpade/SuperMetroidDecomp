using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Ceres falling-debris mechanics and production instruction checks against the retail ROM.</summary>
    private static void VerifyCeresFallingDebrisInstructionProgramDefinitions() =>
        Suite(nameof(VerifyCeresFallingDebrisInstructionProgramDefinitions), () => VerifyCeresFallingDebrisInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>
    /// Compares compiled Ceres debris mechanics with cartridge words, then exercises both
    /// projectile variants while observing presentation reads and validating their frames.
    /// </summary>
    /// <param name="rom">Retail address space used as the source of native instruction words and selectors.</param>
    private static void VerifyCeresFallingDebrisInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Ceres falling-debris mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new CeresFallingDebrisInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        foreach ((bool dark, RoomEnemyProjectileKind kind, ushort program, ushort sleep) in
                 new[]
                 {
                     (false, RoomEnemyProjectileKind.CeresFallingDebrisLight,
                         CeresFallingDebrisInstructionProgramDefinitions.Light, (ushort)0x9754),
                     (true, RoomEnemyProjectileKind.CeresFallingDebrisDark,
                         CeresFallingDebrisInstructionProgramDefinitions.Dark, (ushort)0x975a),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            enemies.SpawnCeresFallingDebris(96, dark);
            RoomEnemyProjectileSlot debris = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == kind);
            AssertEqual(program, debris.InstructionPointer,
                "real Ceres debris producer selects the named pose");
            RunForcedTick(enemies, debris);
            AssertEqual(sleep, debris.InstructionPointer,
                "Ceres debris displays its pose before terminal sleep");
            RunForcedTick(enemies, debris);
            AssertEqual(sleep, debris.InstructionPointer,
                "Ceres debris terminal sleep retains its own opcode");
            AssertEqual((ushort)0, debris.InstructionTimer,
                "Ceres debris terminal sleep stores the native zero timer");

            debris.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
            debris.InstructionTimer = 1;
            RunForcedTick(enemies, debris);
            AssertTrue(!debris.IsActive,
                "Ceres debris shot reaction reaches the compiled shared delete program");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "CeresFallingDebris execution performs no live spritemap operand reads");
        AssertEqual(CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "CeresFallingDebris executes every native visual operand");
        for (int index = 0; index < CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = CeresFallingDebrisInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"CeresFallingDebris executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "CeresFallingDebris has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "CeresFallingDebris compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared-delete debris mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(0x9752),
            "Ceres debris spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(0x974e),
            "word before Ceres debris programs is rejected as mechanics");

        _ = ProbeCeresFallingDebrisInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCeresFallingDebrisInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ceres debris allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ceres debris mechanics lookups allocate no storage");

        Console.WriteLine(
            "Ceres falling-debris instruction mechanics: four compiled words, both real " +
            "producers, terminal sleeps, shared deletion, and two installed sprite frames match native OAM.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            return enemies;
        }

        void RunForcedTick(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }
    }

    /// <summary>Warms and repeatedly reads the two compiled debris mechanics entries for an allocation measurement.</summary>
    private static int ProbeCeresFallingDebrisInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? CeresFallingDebrisInstructionProgramDefinitions.Light
                    : CeresFallingDebrisInstructionProgramDefinitions.Dark);
        }
        return checksum;
    }

    /// <summary>Wraps cartridge memory to reject compiled mechanics reads and record executed presentation-word reads.</summary>
    /// <param name="source">Underlying address space used for permitted reads and forwarded writes.</param>
    private sealed class CeresFallingDebrisInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the presentation words whose bytes production execution requested.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets the number of attempted reads from compiled mechanics tables.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an imported cartridge read through the same checks as ordinary address-space reads.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The underlying byte unless the address belongs to a forbidden mechanics table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from compiled mechanics bytes, records presentation-word access, and forwards other reads.</summary>
        /// <param name="address">Address requested by production code.</param>
        /// <returns>The byte read from the wrapped source for permitted addresses.</returns>
        public byte ReadByte(int address)
        {
            if (CeresFallingDebrisInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres debris mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = CeresFallingDebrisInstructionProgramDefinitions
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

        /// <summary>Forwards a write to the wrapped address space without altering it.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store at the destination.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
