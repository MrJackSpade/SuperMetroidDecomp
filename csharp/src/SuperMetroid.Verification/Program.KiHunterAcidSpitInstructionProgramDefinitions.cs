using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyKiHunterAcidSpitInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKiHunterAcidSpitInstructionProgramDefinitions), () => VerifyKiHunterAcidSpitInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyKiHunterAcidSpitInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKiHunterAcidSpitInstructionWord(rom, definition.Address),
                $"KiHunter acid-spit mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("KiHunter fixture requires installed projectile sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = new KiHunterAcidSpitInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnKiHunterAcidSpit", flags)!;
        MethodInfo move = typeof(RoomEnemySystem).GetMethod(
            "RunKiHunterAcidMovement", BindingFlags.NonPublic | BindingFlags.Static)!;

        var body = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        spawn.Invoke(enemies, [body, false]);
        spawn.Invoke(enemies, [body, true]);

        RoomEnemyProjectileSlot left = Find(RoomEnemyProjectileKind.KiHunterAcidSpitLeft);
        RoomEnemyProjectileSlot right = Find(RoomEnemyProjectileKind.KiHunterAcidSpitRight);
        Suite(nameof(VerifyIntroduction), () => VerifyIntroduction(
            left,
            KiHunterAcidSpitInstructionProgramDefinitions.Left,
            (ushort)EnemyProjectilePreInstruction.KiHunterAcid_Left));
        Suite(nameof(VerifyIntroduction), () => VerifyIntroduction(
            right,
            KiHunterAcidSpitInstructionProgramDefinitions.Right,
            (ushort)EnemyProjectilePreInstruction.KiHunterAcid_Right));

        var blocks = new ushort[64];
        Array.Fill(blocks, (ushort)0x8000, 32, 32);
        var level = new RoomLevelData(
            8, 8, blocks, new byte[64], new ushort[64], new byte[8]);
        left.XPosition = 64;
        left.YPosition = 63;
        left.XRadius = 2;
        left.YRadius = 2;
        left.YVelocity = 0x0100;
        move.Invoke(enemies, [left, level]);
        AssertEqual(KiHunterAcidSpitInstructionProgramDefinitions.HitFloor,
            left.InstructionPointer,
            "real solid-floor collision installs the named acid-splash program");

        RunForcedTicks(left, 5);
        AssertEqual((ushort)0xcf6c, left.InstructionPointer,
            "KiHunter acid splash reaches deletion after all five authored frames");
        RunForcedTicks(left, 1);
        AssertTrue(!left.IsActive,
            "KiHunter acid splash deletes after its complete floor impact");

        right.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(right, 1);
        AssertTrue(!right.IsActive,
            "KiHunter acid shot reaction reaches the compiled shared delete program");

        AssertEqual(KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "both introductions and impact execute every native visual operand");
        AssertEqual(0,
            guard.ObservedPresentationWords.Count,
            "KiHunter acid-spit execution performs no live spritemap operand reads");
        for (int index = 0;
             index < KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KiHunterAcidSpitInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                $"KiHunter acid presentation $86:{address:X4} has an installed selector");
            AssertEqual(ReadKiHunterAcidSpitInstructionWord(rom, address), selector,
                $"installed KiHunter acid selector $86:{address:X4} matches the cartridge");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled KiHunter acid and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => KiHunterAcidSpitInstructionProgramDefinitions.ReadMechanicsWord(0xcf36),
            "KiHunter acid spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KiHunterAcidSpitInstructionProgramDefinitions.ReadMechanicsWord(0xcf90),
            "adjacent KiHunter acid initializer is rejected as mechanics");

        _ = ProbeKiHunterAcidSpitInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKiHunterAcidSpitInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "KiHunter acid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed KiHunter acid mechanics lookups allocate no storage");

        Console.WriteLine(
            "KiHunter acid-spit instruction mechanics: twenty-seven compiled words, both " +
            "real directional producers, terminal sleeps, real floor impact, complete " +
            "splash, shared shot deletion, and nineteen native selectors with zero live reads pass.");

        RoomEnemyProjectileSlot Find(RoomEnemyProjectileKind kind) =>
            enemies.EnemyProjectiles.Single(projectile => projectile.Kind == kind);

        void VerifyIntroduction(
            RoomEnemyProjectileSlot projectile,
            ushort initial,
            ushort installedPreInstruction)
        {
            AssertEqual(initial, projectile.InstructionPointer,
                $"real {projectile.Kind} producer selects its named introduction");
            RunForcedTicks(projectile, 7);
            AssertEqual(unchecked((ushort)(initial + 0x20)), projectile.InstructionPointer,
                $"{projectile.Kind} reaches its terminal sleep after all seven frames");
            AssertEqual(installedPreInstruction, projectile.PreInstruction,
                $"{projectile.Kind} installs its one-shot movement callback");
            RunForcedTicks(projectile, 1);
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{projectile.Kind} terminal sleep stops its instruction timer");
        }

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
                if (projectile.IsActive && projectile.InstructionTimer != 0)
                {
                    ushort operand = unchecked((ushort)(projectile.InstructionPointer - 2));
                    AssertEqual(operand, projectile.PresentationOperandAddress,
                        "executed KiHunter frame retains its native presentation operand");
                    executedOperands.Add(operand);
                    var expected = new OamBuffer();
                    var actual = new OamBuffer();
                    expected.BeginFrame();
                    actual.BeginFrame();
                    DrawImportedEnemyProjectileSpritemap(rom, expected,
                        ReadKiHunterAcidSpitInstructionWord(rom, operand), 128, 112, 0, true);
                    actual.AddEnemySpritemap(spriteArtwork.GetProgramFrame(operand).Span,
                        128, 112, 0, 0, clipVerticalWrap: true, originYIsOnScreen: true);
                    AssertEqual(expected.NextByteOffset, actual.NextByteOffset,
                        $"KiHunter frame $86:{operand:X4} native sprite part count");
                    AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) &&
                        expected.HighTable.SequenceEqual(actual.HighTable),
                        $"KiHunter frame $86:{operand:X4} installed composition matches native OAM");
                }
            }
        }
    }

    private static int ProbeKiHunterAcidSpitInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KiHunterAcidSpitInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KiHunterAcidSpitInstructionProgramDefinitions.Left
                    : KiHunterAcidSpitInstructionProgramDefinitions.HitFloor);
        }
        return checksum;
    }

    private static ushort ReadKiHunterAcidSpitInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class KiHunterAcidSpitInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (KiHunterAcidSpitInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled KiHunter acid mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = KiHunterAcidSpitInstructionProgramDefinitions
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
