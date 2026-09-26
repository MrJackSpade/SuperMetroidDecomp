using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresBabyInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Ceres Baby instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CeresBabyInstructionMechanicsWord definition =
                CeresBabyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadCeresBabyProgramWord(rom, definition.Address),
                $"Ceres Baby mechanics word $A6:{definition.Address:X4}");
        }
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.SpritemapOperandCount;
             index++)
        {
            ushort address =
                CeresBabyInstructionProgramDefinitions.SpritemapOperandAddress(index);
            AssertEqual(ReadCeresBabyProgramWord(rom, address),
                CeresBabyInstructionProgramDefinitions.ReadSpritemapOperand(address),
                $"compiled Ceres Baby spritemap operand $A6:{address:X4}");
        }
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.PaletteOperandCount;
             index++)
        {
            ushort address =
                CeresBabyInstructionProgramDefinitions.PaletteOperandAddress(index);
            int row = CeresBabyInstructionProgramDefinitions.ReadPaletteRow(address);
            AssertEqual(unchecked((ushort)(
                    CeresRidleyPaletteRomData.BabyColors + row *
                    CeresRidleyPaletteRomData.BabyColorCount * sizeof(ushort))),
                ReadCeresBabyProgramWord(rom, address),
                $"compiled Ceres Baby palette operand $A6:{address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new CeresBabyInstructionReadGuard(rom);
        byte[] colorJson = CeresRidleyColorExtractor.Extract(rom);
        var enemies = new RoomEnemySystem
        {
            CeresRidleyColors = CeresRidleyColorCatalog.Load(
                new MemoryStream(colorJson, writable: false)),
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        ushort random = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => random));
        var advance = typeof(RoomEnemySystem).GetMethod(
                "AdvanceCeresBabyDrawInstruction", flags)!
            .CreateDelegate<Func<RidleyEnemyState, ushort>>(enemies);

        var completeLoop = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 0,
        };
        var observedSpritemaps = new HashSet<ushort>();
        for (int call = 0; call < 500; call++)
            observedSpritemaps.Add(advance(completeLoop));

        AssertTrue(observedSpritemaps.Contains(CeresBabyInstructionProgramDefinitions.HorizontalFrame),
            "production Ceres Baby loop displays horizontal-squish spritemap");
        AssertTrue(observedSpritemaps.Contains(CeresBabyInstructionProgramDefinitions.RoundFrame),
            "production Ceres Baby loop displays round spritemap");
        AssertTrue(observedSpritemaps.Contains(CeresBabyInstructionProgramDefinitions.VerticalFrame),
            "production Ceres Baby loop displays vertical-squish spritemap");

        // A stationary odd-RNG call takes the authored 50% branch back to the initial
        // program; a moving call bypasses that random branch and enters the expressive
        // palette loop. These are the two conditional control-flow edges in the stream.
        random = 1;
        var randomBranch = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.ExpressiveLoop,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 0,
        };
        _ = advance(randomBranch);
        AssertEqual((ushort)0xbf35, randomBranch.BabyInstruction,
            "stationary odd-RNG branch returns to initial Ceres Baby frames");

        random = 0;
        var movingBranch = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 1,
        };
        _ = advance(movingBranch);
        AssertEqual((ushort)0xbf61, movingBranch.BabyInstruction,
            "moving Ceres Baby branch enters expressive palette frames");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Ceres Baby interpreter avoids compiled mechanics and palette/pose bytes");

        CeresRidleyColorDocument colorDocument =
            JsonSerializer.Deserialize<CeresRidleyColorDocument>(colorJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        PaletteRgb5 priorBabyColor = colorDocument.Baby![1][0];
        colorDocument.Baby[1][0] = priorBabyColor with
        {
            Blue = priorBabyColor.Blue == 31 ? 30 : priorBabyColor.Blue + 1,
        };
        enemies.CeresRidleyColors = CeresRidleyColorCatalog.Load(
            new MemoryStream(CeresRidleyColorCatalog.Write(colorDocument), writable: false));
        var editedPaletteState = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.ExpressiveLoop,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 1,
            BabyXPosition = 100,
            BabyYPosition = 80,
        };
        ushort editedPalettePose = advance(editedPaletteState);
        AssertEqual(enemies.CeresRidleyColors.ResolveBaby(1, 0),
            ((SnesCgram)typeof(RoomEnemySystem).GetField("_cgram", flags)!
                .GetValue(enemies)!).Colors[CeresRidleyPaletteRomData.BabyCgramIndex],
            "edited Ceres Baby color reaches the native CGRAM slot");
        AssertEqual(CeresBabyInstructionProgramDefinitions.HorizontalFrame,
            editedPalettePose, "palette edit does not change Baby pose selection");
        AssertEqual((ushort)100, editedPaletteState.BabyXPosition,
            "palette edit does not change Baby world position");
        AssertEqual((ushort)2, editedPaletteState.BabyInstructionTimer,
            "palette edit does not change Baby animation timer");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited Ceres Baby color never rereads ROM visuals");

        byte[] stockJson = EnemySpritemapFiles.Extract(rom);
        EnemySpritemapCatalog installed = EnemySpritemapCatalog.Load(
            new MemoryStream(stockJson, writable: false));
        enemies.TileArtwork = new EnemyTileArtworkCatalog(
            new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>(),
            spritemaps: installed);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            new CeresBabyInstructionReadGuard(rom, blockBabyFrames: true));
        var baby = new RidleyEnemyState
        {
            MovementAnimationEnabled = 0,
            BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial,
            BabyInstructionTimer = 1,
            BabyXPosition = 100,
            BabyYPosition = 80,
        };
        typeof(RoomEnemySystem).GetField("_ridleyState", flags)!.SetValue(enemies, baby);
        var nativeBaby = new OamBuffer();
        nativeBaby.BeginFrame();
        nativeBaby.AddEnemySpritemap(rom, CeresBabyInstructionProgramDefinitions.Bank,
            CeresBabyInstructionProgramDefinitions.HorizontalFrame, 100, 80,
            paletteBits: 0, baseTileIndex: 0,
            clipVerticalWrap: true, originYIsOnScreen: true);
        nativeBaby.FinalizeFrame();
        var installedBaby = new OamBuffer();
        installedBaby.BeginFrame();
        enemies.DrawCeresRidleyImmediateBabyAndDoor(installedBaby, 0, 0);
        installedBaby.FinalizeFrame();
        AssertTrue(nativeBaby.LowTable.SequenceEqual(installedBaby.LowTable) &&
                   nativeBaby.HighTable.SequenceEqual(installedBaby.HighTable),
            "private Ceres Baby draw matches native OAM with sprite ROM reads forbidden");
        AssertEqual(CeresBabyInstructionProgramDefinitions.HorizontalFrame,
            baby.BabyCurrentSpritemap, "installed art retains the authored Baby pose");
        AssertEqual((ushort)2, baby.BabyInstructionTimer,
            "installed art retains Baby's native instruction timing");

        EnemySpritemapDocument visual = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SpriteVisualPart first = visual.Frames["ceres_baby_horizontal"][0];
        visual.Frames["ceres_baby_horizontal"][0] = first with
        {
            OffsetY = first.OffsetY + 1,
        };
        byte[] editedJson = JsonSerializer.SerializeToUtf8Bytes(visual,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        enemies.TileArtwork = new EnemyTileArtworkCatalog(
            new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>(),
            spritemaps: EnemySpritemapCatalog.Load(
                new MemoryStream(editedJson, writable: false)));
        baby.BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial;
        baby.BabyInstructionTimer = 1;
        var editedBaby = new OamBuffer();
        editedBaby.BeginFrame();
        enemies.DrawCeresRidleyImmediateBabyAndDoor(editedBaby, 0, 0);
        editedBaby.FinalizeFrame();
        AssertEqual(nativeBaby.GetEntry(0).X, editedBaby.GetEntry(0).X,
            "Baby art edit preserves the authored world X position");
        AssertEqual(nativeBaby.GetEntry(0).Y + 1, editedBaby.GetEntry(0).Y,
            "Baby art edit changes the visible Y offset");
        AssertEqual((ushort)2, baby.BabyInstructionTimer,
            "Baby art edit cannot change the instruction clock");

        AssertThrows<InvalidDataException>(
            () => CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(0xbf37),
            "Ceres Baby spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(0xbfc9),
            "Ceres Baby restored pointer cannot enter adjacent callback code");
        AssertThrows<InvalidDataException>(
            () => CeresBabyInstructionProgramDefinitions.ReadSpritemapOperand(0xbf5f),
            "Ceres Baby palette operand cannot be mistaken for a spritemap selector");

        _ = CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(
            CeresBabyInstructionProgramDefinitions.Initial);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(
                CeresBabyInstructionProgramDefinitions.Initial);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Ceres Baby allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Ceres Baby mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"  Ceres Baby instruction mechanics: " +
            $"{CeresBabyInstructionProgramDefinitions.MechanicsWordCount} words and " +
            "twenty compiled spritemap selectors and thirteen compiled palette " +
            "selectors pass through the complete production loop.");
    }

    private static ushort ReadCeresBabyProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa60000 | address) |
            source.ReadByte(0xa60000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class CeresBabyInstructionReadGuard(
        ISnesAddressSpace source, bool blockBabyFrames = false) :
        ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CeresBabyInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CeresBabyInstructionProgramDefinitions.IsCompiledSpritemapByte(address) ||
                CeresBabyInstructionProgramDefinitions.IsCompiledPaletteByte(address) ||
                address is >= CeresRidleyPaletteRomData.BabyColors and <
                    CeresRidleyPaletteRomData.BabyColors +
                    CeresRidleyPaletteRomData.BabyRowCount *
                    CeresRidleyPaletteRomData.BabyColorCount * sizeof(ushort) ||
                (blockBabyFrames && address is >= 0xa6bffd and < 0xa6c04e))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres Baby control/selector byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
