using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private const ushort AlcoonInstructionAuditRoom = 0x93aa;

    private static void VerifyAlcoonMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0xdbe7,0xdbe9,0xdbed,0xdbef,0xdbf3,0xdbf5,0xdbf9,0xdbfb,0xdbff,0xdc01,
            0xdc03,0xdc07,0xdc0b,0xdc0f,0xdc13,0xdc15,0xdc19,0xdc1d,0xdc21,0xdc25,
            0xdc29,0xdc2b,0xdc2f,0xdc33,0xdc37,0xdc3b,0xdc3f,0xdc41,0xdc45,0xdc47,
            0xdc4b,0xdc4f,0xdc51,0xdc55,
            0xdc57,0xdc59,0xdc5d,0xdc5f,0xdc63,0xdc65,0xdc69,0xdc6b,0xdc6f,0xdc71,
            0xdc73,0xdc77,0xdc7b,0xdc7f,0xdc83,0xdc85,0xdc89,0xdc8d,0xdc91,0xdc95,
            0xdc99,0xdc9b,0xdc9f,0xdca3,0xdca7,0xdcab,0xdcaf,0xdcb1,0xdcb5,0xdcb7,
            0xdcbb,0xdcbf,0xdcc1,0xdcc5,
        ];
        AssertEqual(addresses.Length, AlcoonInstructionProgramDefinitions.MechanicsWordCount, "Alcoon word count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadAlcoonInstructionWord(rom, address);
            var actual = AlcoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Alcoon original word position");
            AssertEqual(expected, actual.Value, "Alcoon original enumerated word");
            AssertEqual(expected, AlcoonInstructionProgramDefinitions.ReadMechanicsWord(address), "Alcoon direct word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = bytes.Contains(address);
            AssertEqual(expected, AlcoonInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa80000 | address),
                "Alcoon full bank byte ownership including odd word starts");
            AssertEqual(expected, AlcoonInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1a80000 | address),
                "Alcoon preserves high-bit mask");
            AssertTrue(!AlcoonInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa70000 | address),
                "Alcoon rejects other bank");
        }
        var words = addresses.ToHashSet();
        for (int address = 0xdbe5; address <= 0xdcc9; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => AlcoonInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Alcoon rejects presentation words, misalignment and adjacent data");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AlcoonInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Alcoon rejects distant invalid word");
        foreach (int index in new[] { int.MinValue, -1, 68, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AlcoonInstructionProgramDefinitions.MechanicsWord(index),
                "Alcoon mechanics ordinal bounds");
    }

    private static void VerifyAlcoonPresentationAddressMapping()
    {
        ushort[] expected =
        [
            0xdbeb,0xdbf1,0xdbf7,0xdbfd,
            0xdc05,0xdc09,0xdc0d,0xdc11,0xdc17,0xdc1b,0xdc1f,0xdc23,
            0xdc27,0xdc2d,0xdc31,0xdc35,0xdc39,0xdc3d,0xdc43,0xdc49,0xdc4d,0xdc53,
            0xdc5b,0xdc61,0xdc67,0xdc6d,
            0xdc75,0xdc79,0xdc7d,0xdc81,0xdc87,0xdc8b,0xdc8f,0xdc93,
            0xdc97,0xdc9d,0xdca1,0xdca5,0xdca9,0xdcad,0xdcb3,0xdcb9,0xdcbd,0xdcc3,
        ];
        AssertEqual(expected.Length, AlcoonInstructionProgramDefinitions.PresentationWordCount, "Alcoon visual count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], AlcoonInstructionProgramDefinitions.PresentationWordAddress(index),
                "Alcoon original presentation operand position");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), AlcoonInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Alcoon full presentation word membership domain");
        foreach (int index in new[] { int.MinValue, -1, 44, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AlcoonInstructionProgramDefinitions.PresentationWordAddress(index),
                "Alcoon presentation ordinal bounds");
    }

    private static void VerifyAlcoonInstructionProgramDefinitions()
    {
        VerifyAlcoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyAlcoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? installedArt = null)
    {
        VerifyAlcoonMechanicsMapping(rom);
        VerifyAlcoonPresentationAddressMapping();

        var guard = new AlcoonInstructionReadGuard(rom, forbidPresentation: true);
        CartridgeRoomHeader room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(rom, AlcoonInstructionAuditRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(rom, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem { TileArtwork = installedArt };
        enemies.Load(
            guard,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData);

        RoomEnemySlot actor = enemies.Slots[0];
        AlcoonEnemyState state = enemies.AlcoonStates[0] ??
            throw new InvalidDataException("Real Alcoon initializer omitted slot-zero state.");
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = unchecked((ushort)(actor.XPosition + 1)),
            YPosition = state.LandingYPosition,
        };
        samus.RefreshCollisionRadii(rom);
        samus.InitializeAnimation(rom);

        bool sawLeftFireball = false;
        bool sawRightFireball = false;
        var selectedPresentationOperands = new HashSet<ushort>();
        for (int frame = 0; frame < 4_096 &&
             (installedArt is not null || selectedPresentationOperands.Count < 42 ||
              !sawLeftFireball || !sawRightFireball); frame++)
        {
            int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
            ushort cameraX = unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX));
            enemies.StepFrame(cameraX, 0, false, samus, level: assets.LevelData);
            foreach (RoomEnemySlot slot in enemies.Slots)
            {
                if (slot.EnemyDefinitionPointer != RoomEnemySystem.AlcoonDefinition)
                    continue;
                ushort selectedOperand = unchecked((ushort)(slot.CurrentInstruction - 2));
                if (AlcoonInstructionProgramDefinitions.IsPresentationWord(selectedOperand))
                    selectedPresentationOperands.Add(selectedOperand);
            }

            samus.XPosition = unchecked((ushort)(actor.XPosition +
                (unchecked((short)state.XVelocity) < 0 ? -32 : 32)));
            samus.YPosition = state.LandingYPosition;
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball))
            {
                sawLeftFireball |= unchecked((short)projectile.XVelocity) < 0;
                sawRightFireball |= unchecked((short)projectile.XVelocity) > 0;
            }
            enemies.StepEnemyProjectiles(assets.LevelData, samus: null, cameraX: 0, cameraY: 0);
        }

        AssertTrue(sawLeftFireball && sawRightFireball,
            "real Alcoon programs execute fire callbacks in both facings");
        string missedOperands = string.Join(", ", Enumerable.Range(0,
                AlcoonInstructionProgramDefinitions.PresentationWordCount)
            .Select(AlcoonInstructionProgramDefinitions.PresentationWordAddress)
            .Where(address => !selectedPresentationOperands.Contains(address))
            .Select(address => $"$A8:{address:X4}"));
        AssertEqual(42, selectedPresentationOperands.Count,
            $"real Alcoon visits every reachable presentation operand; missing={missedOperands}");
        AssertTrue(!selectedPresentationOperands.Contains(0xdc49) &&
                   !selectedPresentationOperands.Contains(0xdcb9),
            "native start-walking callbacks skip the two trailing unreachable frames");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Alcoon reads no cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Alcoon mechanics byte");

        _ = ProbeAlcoonInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeAlcoonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Alcoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Alcoon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(installedArt is null
            ? "Alcoon instruction mechanics: 68 compiled words, all ten authored programs, " +
              "42 reachable visual operands, both fire directions, and native callback " +
              "handoffs pass with mechanics bytes forbidden."
            : "Installed Alcoon: real-room movement and both fire directions pass " +
              "with mechanics and visual source bytes forbidden.");
    }

    private static int ProbeAlcoonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += AlcoonInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 10) switch
                {
                    0 => AlcoonInstructionProgramDefinitions.WalkingLeft,
                    1 => AlcoonInstructionProgramDefinitions.FireLeft,
                    2 => AlcoonInstructionProgramDefinitions.AirborneLeftLookingUp,
                    3 => AlcoonInstructionProgramDefinitions.AirborneLeftLookingForward,
                    4 => AlcoonInstructionProgramDefinitions.WalkingRight,
                    5 => AlcoonInstructionProgramDefinitions.FireRight,
                    6 => AlcoonInstructionProgramDefinitions.AirborneRightLookingUp,
                    7 => AlcoonInstructionProgramDefinitions.AirborneRightLookingForward,
                    8 => 0xdbff,
                    _ => 0xdc6f,
                });
        }
        return checksum;
    }

    private static ushort ReadAlcoonInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class AlcoonInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (AlcoonInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Alcoon mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < AlcoonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        AlcoonInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Alcoon read cartridge visual selector $A8:{presentation:X4}.");
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
