using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers verification of compiled Yapping Maw body-projectile instructions against the retail ROM.</summary>
    private static void VerifyYappingMawBodyProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYappingMawBodyProjectileInstructionProgramDefinitions), () => VerifyYappingMawBodyProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled mechanics and runs both facing producers through body animation, sleep, and shared deletion.</summary>
    /// <param name="rom">Retail address space used to verify instruction words, frame durations, and compiled sprite selectors.</param>
    private static void VerifyYappingMawBodyProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadYappingMawBodyProjectileInstructionWord(rom, definition.Address),
                $"Yapping Maw body-projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new YappingMawBodyProjectileInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeYappingMaw", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        RoomEnemyProjectileSlot? deletionCandidate = null;

        foreach ((ushort parameter, ushort expected, string facing) in new[]
        {
            ((ushort)0, YappingMawBodyProjectileInstructionProgramDefinitions.FacingDown,
                "down"),
            ((ushort)1, YappingMawBodyProjectileInstructionProgramDefinitions.FacingUp,
                "up"),
        })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot owner = enemies.Slots[0];
            owner.EnemyDefinitionPointer = RoomEnemySystem.YappingMawDefinition;
            owner.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            owner.XPosition = 128;
            owner.YPosition = 112;
            owner.Parameter1 = 128;
            owner.Parameter2 = parameter;
            initialize.Invoke(enemies, [owner]);

            RoomEnemyProjectileSlot[] bodies = enemies.YappingMawStates[0]!
                .BodyProjectiles
                .OfType<RoomEnemyProjectileSlot>()
                .ToArray();
            AssertEqual(4, bodies.Length,
                $"real {facing}-facing Yapping Maw producer spawns four body links");
            foreach (RoomEnemyProjectileSlot body in bodies)
            {
                AssertEqual(expected, body.InstructionPointer,
                    $"real {facing}-facing Yapping Maw body selects its named pose");
                RunForcedTick(enemies, body);
                AssertEqual(unchecked((ushort)(expected + 2)), body.PresentationOperandAddress,
                    $"actual {facing}-facing body selects its installed presentation operand");
                AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, body.SpritemapPointer,
                    $"actual {facing}-facing body defers artwork resolution to the draw pass");
                AssertEqual(unchecked((ushort)(expected + 4)), body.InstructionPointer,
                    $"{facing}-facing Yapping Maw body schedules its terminal sleep");
                RunForcedTick(enemies, body);
                AssertEqual(unchecked((ushort)(expected + 2)), body.PresentationOperandAddress,
                    $"actual {facing}-facing body selects its installed presentation operand");
                AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, body.SpritemapPointer,
                    $"actual {facing}-facing body defers artwork resolution to the draw pass");
                AssertEqual(unchecked((ushort)(expected + 4)), body.InstructionPointer,
                    $"{facing}-facing Yapping Maw body sleeps at the authored opcode");
                AssertEqual((ushort)0, body.InstructionTimer,
                    $"{facing}-facing Yapping Maw sleep leaves the timer stopped");
            }

            deletionCandidate ??= bodies[0];

            void RunForcedTick(
                RoomEnemySystem system,
                RoomEnemyProjectileSlot body)
            {
                body.InstructionTimer = 1;
                process.Invoke(system, [body, new SamusState(), (ushort)0, (ushort)0]);
                VerifyExecutedProjectileFrame(rom, body, spriteArtwork, executedOperands);
            }
        }

        var deletionSystem = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(deletionSystem, guard);
        RoomEnemyProjectileSlot deletion = deletionCandidate!;
        deletion.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        deletion.InstructionTimer = 1;
        process.Invoke(deletionSystem, [deletion, new SamusState(), (ushort)0, (ushort)0]);
        AssertTrue(!deletion.IsActive,
            "Yapping Maw body shot reaction reaches the compiled shared delete program");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "YappingMawBodyProjectile execution performs no live spritemap operand reads");
        AssertEqual(YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "YappingMawBodyProjectile executes every native visual operand");
        for (int index = 0; index < YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"YappingMawBodyProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "YappingMawBodyProjectile has a compiled visual selector");
            AssertEqual(ReadYappingMawBodyProjectileInstructionWord(rom, address), selector,
                "YappingMawBodyProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Yapping Maw body and shared-delete mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => YappingMawBodyProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                0xec58),
            "Yapping Maw body spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => YappingMawBodyProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                0xec62),
            "adjacent Yapping Maw body initializer is rejected as mechanics");

        _ = ProbeYappingMawBodyProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYappingMawBodyProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Yapping Maw body allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yapping Maw body mechanics lookups allocate no storage");

        Console.WriteLine(
            "Yapping Maw body-projectile instruction mechanics: four compiled words, " +
            "both real facing producers, eight terminal sleeps, shared shot deletion, " +
            "and both installed sprite frames pass with mechanics and visual operand ROM reads forbidden.");
    }

    /// <summary>Repeats facing-program lookups so verification can measure warmed mechanics access allocations.</summary>
    /// <returns>A checksum of the looked-up instruction words.</returns>
    private static int ProbeYappingMawBodyProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YappingMawBodyProjectileInstructionProgramDefinitions
                .ReadMechanicsWord((index & 1) == 0
                    ? YappingMawBodyProjectileInstructionProgramDefinitions.FacingDown
                    : YappingMawBodyProjectileInstructionProgramDefinitions.FacingUp);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian body-projectile instruction word from the projectile-code bank.</summary>
    /// <param name="source">Retail address space containing the native instruction stream.</param>
    /// <param name="address">Bank-local address of the first byte in the word.</param>
    /// <returns>The source bytes combined into an unsigned word.</returns>
    private static ushort ReadYappingMawBodyProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects reads from compiled body-projectile and shared-delete mechanics while observing presentation operand reads.</summary>
    /// <param name="source">Address space used for permitted reads and all writes.</param>
    private sealed class YappingMawBodyProjectileInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation operand addresses observed during production projectile execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of attempted reads from compiled body-projectile or shared-delete mechanics bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the runtime mechanics and presentation tracking guard.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The wrapped byte when the address is not guarded mechanics data.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records presentation operand access, and forwards other addresses.</summary>
        /// <param name="address">Cartridge address requested by production code.</param>
        /// <returns>The wrapped byte for an address outside compiled mechanics data.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to compiled body-projectile or shared-delete mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (YappingMawBodyProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Yapping Maw body mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < YappingMawBodyProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = YappingMawBodyProjectileInstructionProgramDefinitions
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

        /// <summary>Forwards writes unchanged because the guard only restricts reads.</summary>
        /// <param name="address">Cartridge address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
