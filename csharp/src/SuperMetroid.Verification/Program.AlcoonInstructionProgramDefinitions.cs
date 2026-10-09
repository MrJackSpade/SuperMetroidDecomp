using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Room header used to load the retail Alcoon instruction and visual audit fixture.</summary>
    private const ushort AlcoonInstructionAuditRoom = 0x93aa;

    /// <summary>Compares all compiled Alcoon mechanics words and their byte ownership with the retail cartridge.</summary>
    /// <param name="rom">Cartridge address space used to read the reference instruction words.</param>
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
        AssertEqual(addresses.Length, AlcoonInstructionProgramDefinitionsTooling.MechanicsWordCount, "Alcoon word count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadAlcoonInstructionWord(rom, address);
            var actual = AlcoonInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Alcoon original word position");
            AssertEqual(expected, actual.Value, "Alcoon original enumerated word");
            AssertEqual(expected, AlcoonInstructionProgramDefinitions.ReadMechanicsWord(address), "Alcoon direct word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = bytes.Contains(address);
            AssertEqual(expected, AlcoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa80000 | address),
                "Alcoon full bank byte ownership including odd word starts");
            AssertEqual(expected, AlcoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1a80000 | address),
                "Alcoon preserves high-bit mask");
            AssertTrue(!AlcoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa70000 | address),
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
            AssertThrows<IndexOutOfRangeException>(() => AlcoonInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "Alcoon mechanics ordinal bounds");
    }

    /// <summary>Lists the native instruction operands that select Alcoon's presentation frames.</summary>
    /// <returns>The ordered bank-$A8 addresses of the presentation words.</returns>
    private static ushort[] AlcoonPresentationAddressOracle() =>
        [
            0xdbeb,0xdbf1,0xdbf7,0xdbfd,
            0xdc05,0xdc09,0xdc0d,0xdc11,0xdc17,0xdc1b,0xdc1f,0xdc23,
            0xdc27,0xdc2d,0xdc31,0xdc35,0xdc39,0xdc3d,0xdc43,0xdc49,0xdc4d,0xdc53,
            0xdc5b,0xdc61,0xdc67,0xdc6d,
            0xdc75,0xdc79,0xdc7d,0xdc81,0xdc87,0xdc8b,0xdc8f,0xdc93,
            0xdc97,0xdc9d,0xdca1,0xdca5,0xdca9,0xdcad,0xdcb3,0xdcb9,0xdcbd,0xdcc3,
        ];

    /// <summary>Checks the compiled presentation-word count, address order, complete membership domain, and ordinal bounds.</summary>
    private static void VerifyAlcoonPresentationAddressMapping()
    {
        ushort[] expected = AlcoonPresentationAddressOracle();
        AssertEqual(expected.Length, AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount, "Alcoon visual count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], AlcoonInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Alcoon original presentation operand position");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), AlcoonInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Alcoon full presentation word membership domain");
        foreach (int index in new[] { int.MinValue, -1, 44, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AlcoonInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                "Alcoon presentation ordinal bounds");
    }

    /// <summary>Compares Alcoon's visual operands with native spritemap pointers and checks selector rejection for gaps.</summary>
    /// <param name="rom">Cartridge address space containing the expected visual operand words.</param>
    private static void VerifyAlcoonVisualSelectorMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = AlcoonPresentationAddressOracle();
        foreach (ushort address in addresses)
        {
            ushort expected = ReadAlcoonInstructionWord(rom, address);
            AssertEqual(expected, EnemySpritemapDefinitions.AlcoonFrameAt(address), "Alcoon direct native visual");
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(RoomEnemySystem.AlcoonDefinition, address, out ushort frame),
                "Alcoon actor visual dispatch");
            AssertEqual(expected, frame, "Alcoon actor selected visual");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa8, address, out ushort shared), "Alcoon shared visual dispatch");
            AssertEqual(expected, shared, "Alcoon shared selected visual");
        }
        var valid = addresses.ToHashSet();
        for (int address = 0xdbe5; address <= 0xdcc9; address++)
            if (!valid.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.AlcoonFrameAt((ushort)address),
                    "Alcoon rejects control, misaligned and adjacent visual operands");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa8, (ushort)address, out ushort missing),
                    "Alcoon shared selector rejects holes");
                AssertEqual((ushort)0, missing, "Alcoon missing shared selector clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.AlcoonFrameAt(address),
                "Alcoon distant invalid visual operand");
    }

    /// <summary>Loads the retail ROM and exercises Alcoon's compiled instruction programs in their room fixture.</summary>
    private static void VerifyAlcoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs Alcoon's native room programs while checking movement callbacks and that compiled data replaces ROM reads.</summary>
    /// <param name="rom">Retail cartridge address space used to construct the room and reference its assets.</param>
    /// <param name="installedArt">Optional installed artwork catalog for the stricter visual-source isolation path.</param>
    private static void VerifyAlcoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? installedArt = null)
    {
        Suite(nameof(VerifyAlcoonMechanicsMapping), () => VerifyAlcoonMechanicsMapping(rom));
        Suite(nameof(VerifyAlcoonPresentationAddressMapping), () => VerifyAlcoonPresentationAddressMapping());

        var guard = new AlcoonInstructionReadGuard(rom, forbidPresentation: true);
        CartridgeRoomHeader room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(rom, AlcoonInstructionAuditRoom);
        var installation = RepositoryInstallation.Installation;
        CartridgeRoomAssets assets = LoadFixtureRoomAssets(rom, room,
            installation.LoadRoomCharacters(), installation.LoadRoomPalettes(),
            installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem { TileArtwork = installedArt ?? RepositoryInstallation.EnemyTiles };
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
                AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount)
            .Select(AlcoonInstructionProgramDefinitionsTooling.PresentationWordAddress)
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

    /// <summary>Repeatedly resolves compiled Alcoon instruction words so the caller can measure warmed lookup allocations.</summary>
    /// <returns>A checksum that makes the word lookups observable.</returns>
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

    /// <summary>Reads one little-endian instruction word from the Alcoon bank in the supplied cartridge space.</summary>
    /// <param name="source">Cartridge address space containing bank-$A8 data.</param>
    /// <param name="address">Bank-local address of the first byte.</param>
    /// <returns>The two bytes combined into a 16-bit word.</returns>
    private static ushort ReadAlcoonInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects compiled mechanics reads and tracks or rejects native presentation reads through a wrapped bus.</summary>
    /// <param name="source">Underlying address space for reads and writes that the guard permits.</param>
    /// <param name="forbidPresentation"><see langword="true"/> to throw on presentation-word reads; otherwise record them for auditing.</param>
    private sealed class AlcoonInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation operands observed when the guard is configured to record rather than reject them.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Count of attempts to read bytes owned by the compiled mechanics catalog.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge reads through the guard's mechanics and presentation checks.</summary>
        /// <param name="address">SNES bus address to read.</param>
        /// <returns>The byte returned by the wrapped address space for an allowed read.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects mechanics reads, then records or rejects presentation operands before forwarding other reads.</summary>
        /// <param name="address">SNES bus address to read.</param>
        /// <returns>The byte returned by the wrapped address space for an allowed read.</returns>
        public byte ReadByte(int address)
        {
            if (AlcoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Alcoon mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        AlcoonInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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

        /// <summary>Forwards a memory write to the wrapped address space.</summary>
        /// <param name="address">SNES bus address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
