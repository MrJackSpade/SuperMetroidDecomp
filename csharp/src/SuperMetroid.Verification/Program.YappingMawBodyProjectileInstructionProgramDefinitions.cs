using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyYappingMawBodyProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYappingMawBodyProjectileInstructionProgramDefinitions), () => VerifyYappingMawBodyProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyYappingMawBodyProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            YappingMawBodyProjectileInstructionMechanicsWord definition =
                YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadYappingMawBodyProjectileInstructionWord(rom, definition.Address),
                $"Yapping Maw body-projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = runtimeFixtureInstallation.Value.LoadEnemyTiles().ProjectileSpritemaps
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

    private static ushort ReadYappingMawBodyProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class YappingMawBodyProjectileInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (YappingMawBodyProjectileInstructionProgramDefinitions
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
