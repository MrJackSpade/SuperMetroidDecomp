using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Torizo landing-dust checks against the installed retail ROM.</summary>
    private static void VerifyTorizoLandingDustInstructionProgramDefinitions() =>
        Suite(nameof(VerifyTorizoLandingDustInstructionProgramDefinitions), () => VerifyTorizoLandingDustInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>
    /// Compares compiled mechanics and selectors with the cartridge, then exercises both
    /// foot-dust producers and their rising, four-pose programs through production processing.
    /// </summary>
    /// <param name="rom">Retail address space supplying the reference mechanics and selector words.</param>
    private static void VerifyTorizoLandingDustInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < TorizoLandingDustInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                TorizoLandingDustInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Torizo landing-dust mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new TorizoLandingDustInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoLandingDust", flags)!
            .CreateDelegate<Action<RoomEnemySlot, bool>>(enemies);

        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.XPosition = 300;
        torizo.YPosition = 120;
        spawn(torizo, true);
        spawn(torizo, false);
        RoomEnemyProjectileSlot right = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoRightFootDust);
        RoomEnemyProjectileSlot left = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoLeftFootDust);

        AssertEqual((ushort)324, right.XPosition, "right-foot dust producer X position");
        AssertEqual((ushort)276, left.XPosition, "left-foot dust producer X position");
        AssertEqual((ushort)168, right.YPosition, "right-foot dust producer Y position");
        AssertEqual((ushort)168, left.YPosition, "left-foot dust producer Y position");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.RightFoot,
            right.InstructionPointer, "right-foot dust program selection");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.LeftFoot,
            left.InstructionPointer, "left-foot dust program selection");

        Run(right, 4);
        Run(left, 4);
        AssertEqual((ushort)156, right.YPosition,
            "right-foot dust rises exactly twelve pixels across its four poses");
        AssertEqual((ushort)156, left.YPosition,
            "left-foot dust rises exactly twelve pixels across its four poses");
        Run(right, 1);
        Run(left, 1);
        AssertTrue(!right.IsActive && !left.IsActive,
            "both landing-dust programs delete after their fourth pose");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "TorizoLandingDust execution performs no live spritemap operand reads");
        AssertEqual(TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "TorizoLandingDust executes every native visual operand");
        for (int index = 0; index < TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"TorizoLandingDust executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "TorizoLandingDust has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "TorizoLandingDust compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Torizo landing-dust mechanics byte");
        AssertThrows<InvalidDataException>(
            () => TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(0xaf9f),
            "Torizo landing-dust spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(0xaf92),
            "Torizo landing-dust callback body is rejected as mechanics");

        _ = ProbeTorizoLandingDustInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeTorizoLandingDustInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Torizo landing-dust allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Torizo landing-dust mechanics lookups allocate no storage");

        Console.WriteLine(
            "Torizo landing-dust instruction mechanics: sixteen compiled words, both " +
            "real foot producers, exact twelve-pixel rise/deletion, and eight installed " +
            "sprite frames match native OAM with zero live operand reads.");

        void Run(RoomEnemyProjectileSlot projectile, int steps)
        {
            for (int step = 0; step < steps; step++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, null, (ushort)0, (ushort)0]);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    /// <summary>Repeatedly resolves the right- and left-foot program words for the warmed allocation check.</summary>
    /// <returns>A checksum that keeps the compiled mechanics results observable.</returns>
    private static int ProbeTorizoLandingDustInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += TorizoLandingDustInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? TorizoLandingDustInstructionProgramDefinitions.RightFoot
                    : TorizoLandingDustInstructionProgramDefinitions.LeftFoot);
        }
        return checksum;
    }

    /// <summary>
    /// Rejects reads from compiled landing-dust mechanics and records presentation operand
    /// reads that should be resolved through the compiled visual-selector catalog.
    /// </summary>
    /// <param name="source">Underlying address space used for permitted reads and forwarded writes.</param>
    private sealed class TorizoLandingDustInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the distinct presentation operand addresses read from the cartridge.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Gets the number of rejected reads from compiled landing-dust mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-time cartridge read through the mechanics read guard.</summary>
        /// <param name="address">Absolute cartridge address requested by the importer.</param>
        /// <returns>The source byte when the address is not compiled mechanics data.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects mechanics reads, records selector reads, and forwards other bytes.</summary>
        /// <param name="address">Absolute address requested by production code.</param>
        /// <returns>The source byte when the address is permitted.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled landing-dust mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (TorizoLandingDustInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Torizo landing-dust byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = TorizoLandingDustInstructionProgramDefinitions
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
