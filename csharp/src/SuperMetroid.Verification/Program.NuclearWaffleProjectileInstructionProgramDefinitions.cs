using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNuclearWaffleProjectileInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNuclearWaffleProjectileInstructionProgramDefinitions), () => VerifyNuclearWaffleProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyNuclearWaffleProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < NuclearWaffleProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NuclearWaffleProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNuclearWaffleProjectileInstructionWord(rom, definition.Address),
                $"Nuclear Waffle projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new NuclearWaffleProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeNuclearWaffle", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        RoomEnemySlot owner = enemies.Slots[0];
        owner.Parameter1 = 0x2001;
        owner.Parameter2 = 0x2000;
        owner.XPosition = 0x0180;
        owner.YPosition = 0x0200;
        initialize(owner);

        RoomEnemyProjectileSlot[] projectiles = enemies.NuclearWaffleStates[0]!
            .ProjectileSegments
            .OfType<RoomEnemyProjectileSlot>()
            .ToArray();
        AssertEqual(4, projectiles.Length,
            "real Nuclear Waffle initialization spawns four projectile body links");
        var installed = new HashSet<ushort>();
        foreach (RoomEnemyProjectileSlot projectile in projectiles)
        {
            AssertEqual(NuclearWaffleProjectileInstructionProgramDefinitions.Initial,
                projectile.InstructionPointer,
                "real Nuclear Waffle projectile producer selects the named body loop");
            RunForcedTicks(projectile, 13);
            AssertEqual(
                unchecked((ushort)(
                    NuclearWaffleProjectileInstructionProgramDefinitions.Initial + 4)),
                projectile.InstructionPointer,
                "Nuclear Waffle projectile completes all twelve frames and loops");
            AssertTrue(projectile.IsActive,
                "Nuclear Waffle projectile body loop remains active");
        }

        projectiles[0].InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(projectiles[0], 1);
        AssertTrue(!projectiles[0].IsActive,
            "Nuclear Waffle projectile reaches the compiled shared delete program");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "NuclearWaffleProjectile execution performs no live spritemap operand reads");
        AssertEqual(NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "NuclearWaffleProjectile executes every native visual operand");
        for (int index = 0; index < NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"NuclearWaffleProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "NuclearWaffleProjectile has a compiled visual selector");
            AssertEqual(ReadNuclearWaffleProjectileInstructionWord(rom, address), selector,
                "NuclearWaffleProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Nuclear Waffle projectile and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xbb60),
            "Nuclear Waffle projectile spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xbb92),
            "adjacent Nuclear Waffle projectile initializer is rejected as mechanics");

        _ = ProbeNuclearWaffleProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNuclearWaffleProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Nuclear Waffle projectile allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Nuclear Waffle projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Nuclear Waffle projectile instruction mechanics: fourteen compiled words, " +
            "four real articulated links, complete twelve-frame loops, shared deletion, " +
            "and all twelve installed sprite frames pass with mechanics and visual operand ROM reads forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
                if (projectile.IsActive)
                {
                    AssertEqual((ushort)(NuclearWaffleProjectileInstructionProgramDefinitions.Initial + 2 + tick % 12 * 4),
                        projectile.PresentationOperandAddress, "actual Nuclear Waffle body selects exact native timed operand");
                    installed.Add(projectile.PresentationOperandAddress);
                }
            }
        }
    }

    private static int ProbeNuclearWaffleProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? NuclearWaffleProjectileInstructionProgramDefinitions.Initial
                    : NuclearWaffleProjectileInstructionProgramDefinitions.LoopCommand);
        }
        return checksum;
    }

    private static ushort ReadNuclearWaffleProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class NuclearWaffleProjectileInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (NuclearWaffleProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling
                    .IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Nuclear Waffle projectile mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NuclearWaffleProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = NuclearWaffleProjectileInstructionProgramDefinitions
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
