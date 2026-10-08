using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPolypRockInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyPolypRockInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < PolypRockInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PolypRockInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPolypRockInstructionWord(rom, definition.Address),
                $"Polyp-rock mechanics word $86:{definition.Address:X4}");
        }

        var guard = new PolypRockInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", instanceFlags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnPolypRock", instanceFlags)!;

        var source = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            XSubposition = 0x1234,
            YSubposition = 0x5678,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        spawn.Invoke(enemies, [source, (ushort)0x0010, (ushort)0xff00]);
        RoomEnemyProjectileSlot rock = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PolypRock);
        AssertEqual(PolypRockInstructionProgramDefinitions.Initial,
            rock.InstructionPointer,
            "real Polyp-rock producer selects the named single-frame program");

        RunForcedTick(rock);
        AssertEqual(PolypRockInstructionProgramDefinitions.PresentationWord,
            rock.PresentationOperandAddress,
            "Polyp rock selects its installed presentation binding without cartridge reads");
        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidOperationException("Polyp rock fixture requires installed projectile artwork.");
        Suite(nameof(VerifyExecutedProjectileFrame), () => VerifyExecutedProjectileFrame(rom, rock, spriteArtwork, new HashSet<ushort>()));
        AssertEqual(PolypRockInstructionProgramDefinitions.Sleep,
            rock.InstructionPointer,
            "Polyp rock reaches its terminal sleep after the authored frame");
        RunForcedTick(rock);

        AssertEqual(PolypRockInstructionProgramDefinitions.Sleep,
            rock.InstructionPointer,
            "Polyp rock remains at its authored terminal sleep");
        AssertEqual((ushort)0, rock.InstructionTimer,
            "Polyp-rock sleep leaves the instruction timer stopped");

        rock.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(rock);

        AssertTrue(!rock.IsActive,
            "Polyp-rock shot reaction reaches the compiled shared delete program");

        AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86,
                PolypRockInstructionProgramDefinitions.PresentationWord, out ushort selector),
            "Polyp-rock selector is compiled");
        AssertEqual(ReadPolypRockInstructionWord(rom,
                PolypRockInstructionProgramDefinitions.PresentationWord), selector,
            "compiled Polyp-rock selector equals the native operand");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Polyp-rock and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PolypRockInstructionProgramDefinitions.ReadMechanicsWord(
                PolypRockInstructionProgramDefinitions.PresentationWord),
            "Polyp-rock spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PolypRockInstructionProgramDefinitions.ReadMechanicsWord(0xbbdb),
            "adjacent Polyp-rock initializer is rejected as mechanics");

        _ = ProbePolypRockInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePolypRockInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Polyp-rock allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Polyp-rock mechanics lookups allocate no storage");

        Console.WriteLine(
            "Polyp-rock instruction mechanics: two compiled words, the real producer, " +
            "terminal sleep, shared shot deletion, and installed frame selection pass with " +
            "mechanics bytes forbidden.");

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    private static int ProbePolypRockInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PolypRockInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PolypRockInstructionProgramDefinitions.Initial
                    : PolypRockInstructionProgramDefinitions.Sleep);
        }
        return checksum;
    }

    private static ushort ReadPolypRockInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class PolypRockInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (PolypRockInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Polyp-rock mechanics byte ${address:X6}.");
            }

            int presentation = EnemyProjectileCodePointers.BankBase |
                PolypRockInstructionProgramDefinitions.PresentationWord;
            if (address == presentation || address == presentation + 1)
                throw new InvalidOperationException("Production read the compiled Polyp-rock visual operand.");

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
