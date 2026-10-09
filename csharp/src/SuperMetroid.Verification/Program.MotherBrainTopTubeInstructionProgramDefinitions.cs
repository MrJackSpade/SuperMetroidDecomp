using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Loads the retail ROM and runs the Mother Brain ceiling-tube program checks.</summary>
    private static void VerifyMotherBrainTopTubeInstructionProgramDefinitions() =>
        Suite(nameof(VerifyMotherBrainTopTubeInstructionProgramDefinitions), () => VerifyMotherBrainTopTubeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>
    /// Verifies compiled tube mechanics and visual operands, then exercises all four producers,
    /// their terminal sleeps, and the shared deletion path.
    /// </summary>
    /// <param name="rom">The retail address space used to compare native instruction and sprite data.</param>
    private static void VerifyMotherBrainTopTubeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < MotherBrainTopTubeInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainTopTubeInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Mother Brain ceiling-tube mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new MotherBrainTopTubeInstructionReadGuard(rom);
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainTopTube", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        foreach ((RoomEnemyProjectileKind Kind, ushort Program, ushort X, ushort Y) item in
                 new[]
                 {
                     (RoomEnemyProjectileKind.MotherBrainTopRightTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopRight,
                         (ushort)152, (ushort)47),
                     (RoomEnemyProjectileKind.MotherBrainTopLeftTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopLeft,
                         (ushort)104, (ushort)47),
                     (RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft,
                         (ushort)120, (ushort)59),
                     (RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube,
                         MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight,
                         (ushort)136, (ushort)59),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            spawn.Invoke(enemies, [item.Kind, item.X, item.Y]);
            RoomEnemyProjectileSlot tube = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == item.Kind);
            AssertEqual(item.Program, tube.InstructionPointer,
                $"real {item.Kind} producer selects its named pose");
            AssertEqual(item.X, tube.XPosition, $"real {item.Kind} producer X position");
            AssertEqual(item.Y, tube.YPosition, $"real {item.Kind} producer Y position");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), tube.InstructionPointer,
                $"{item.Kind} displays its pose before terminal sleep");
            RunForcedTick(enemies, tube);
            AssertEqual(unchecked((ushort)(item.Program + 4)), tube.InstructionPointer,
                $"{item.Kind} terminal sleep retains its own opcode");
        }

        RoomEnemySystem shotSystem = NewSystem();
        spawn.Invoke(shotSystem,
            [RoomEnemyProjectileKind.MotherBrainTopRightTube, (ushort)152, (ushort)47]);
        RoomEnemyProjectileSlot shot = shotSystem.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainTopRightTube);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "Mother Brain ceiling-tube shot reaction reaches shared compiled deletion");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Projectile presentation performs zero live cartridge reads");
        AssertEqual(MotherBrainTopTubeInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared ceiling-tube mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MotherBrainTopTubeInstructionProgramDefinitions.ReadMechanicsWord(0xcc45),
            "ceiling-tube spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainTopTubeInstructionProgramDefinitions.ReadMechanicsWord(0xcc5b),
            "ceiling-tube definition data is rejected as mechanics");

        _ = ProbeMotherBrainTopTubeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainTopTubeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "ceiling-tube allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed ceiling-tube mechanics lookups allocate no storage");

        Console.WriteLine(
            "Mother Brain ceiling-tube instruction mechanics: eight compiled words, all " +
            "four real producers, terminal sleeps, shared deletion, and four native " +
            "sprite compositions pass with zero live operand reads.");

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

    /// <summary>
    /// Repeatedly looks up two tube mechanics words to measure warmed definition-lookup allocations.
    /// </summary>
    /// <returns>A checksum that consumes the values returned by the lookups.</returns>
    private static int ProbeMotherBrainTopTubeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MotherBrainTopTubeInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MotherBrainTopTubeInstructionProgramDefinitions.TopRight
                    : MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight);
        }
        return checksum;
    }

    /// <summary>
    /// Wraps the cartridge bus to reject reads of compiled private or shared tube mechanics
    /// and record reads from installed presentation words during production execution.
    /// </summary>
    /// <param name="source">The underlying address space for permitted reads and forwarded writes.</param>
    private sealed class MotherBrainTopTubeInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word addresses whose bytes were requested through the guard.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempts to read compiled tube mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Runs cartridge reads through the guard's mechanics rejection and presentation tracking.</summary>
        /// <param name="address">The absolute cartridge address to read.</param>
        /// <returns>The underlying byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, tracks presentation words, and forwards other reads.</summary>
        /// <param name="address">The absolute address to read.</param>
        /// <returns>The byte supplied by the wrapped address space.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled tube or shared projectile mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (MotherBrainTopTubeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Mother Brain ceiling-tube mechanics byte " +
                    $"${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MotherBrainTopTubeInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = MotherBrainTopTubeInstructionProgramDefinitions
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

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">The absolute address receiving the write.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
