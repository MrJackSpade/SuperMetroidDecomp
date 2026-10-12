using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;

internal static class CeresBabyInstructionProgramDefinitionsVerification
{
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyCeresBabyInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Ceres Baby instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CeresBabyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | definition.Address),
                $"Ceres Baby mechanics word $A6:{definition.Address:X4}");
        }
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.SpritemapOperandCount;
             index++)
        {
            ushort address =
                CeresBabyInstructionProgramDefinitions.SpritemapOperandAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | address),
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
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | address),
                $"compiled Ceres Baby palette operand $A6:{address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewCeresBabyInstructionReadGuard(rom);
        byte[] colorJson = CeresRidleyColorExtractor.Extract(rom);
        var enemies = new RoomEnemySystem
        {
            Presentation = { CeresRidleyColors = CeresRidleyColorCatalog.Load(
                new MemoryStream(colorJson, writable: false)) },
        };
        enemies.SlotTable.Slots[0].EnemyDefinitionPointer = EnemyDefinitionId.RidleyCeres;
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = new SnesCgram();
        // $A6:BFCF samples the live RNG word; it never calls GenerateRandomNumber.
        ushort random = 0;
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => random);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => throw new InvalidOperationException(
                "The Ceres Baby draw instructions must not advance the RNG."));
        Func<RidleyEnemyState, ushort> advance = enemies.CeresRidleyBehavior.AdvanceCeresBabyDrawInstruction;

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
        enemies.Presentation.CeresRidleyColors = CeresRidleyColorCatalog.Load(
            new MemoryStream(CeresRidleyColorCatalog.Write(colorDocument), writable: false));
        var editedPaletteState = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.ExpressiveLoop,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 1,
            BabyPosition = new Position16<RoomPixels>(100, 80),
        };
        ushort editedPalettePose = advance(editedPaletteState);
        AssertEqual(enemies.Presentation.CeresRidleyColors.ResolveBaby(1, 0),
            ((SnesCgram)enemies.Presentation._cgram).Colors[CeresRidleyPaletteRomData.BabyCgramIndex],
            "edited Ceres Baby color reaches the native CGRAM slot");
        AssertEqual(CeresBabyInstructionProgramDefinitions.HorizontalFrame,
            editedPalettePose, "palette edit does not change Baby pose selection");
        AssertEqual((ushort)100, editedPaletteState.BabyPosition.X,
            "palette edit does not change Baby world position");
        // $A6:DBFE installs the frame reached through the ASM instructions with an
        // elapsed timer of one; the palette edit must not alter that.
        AssertEqual((ushort)1, editedPaletteState.BabyInstructionTimer,
            "palette edit does not change Baby animation timer");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited Ceres Baby color never rereads ROM visuals");

        byte[] stockJson = EnemySpritemapFiles.Extract(rom);
        EnemySpritemapCatalog installed = EnemySpritemapCatalog.Load(
            new MemoryStream(stockJson, writable: false));
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), Spritemaps = installed });
        enemies.BindFixtureBus(NewCeresBabyInstructionReadGuard(rom, blockBabyFrames: true));
        var baby = new RidleyEnemyState
        {
            MovementAnimationEnabled = 0,
            BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial,
            BabyInstructionTimer = 1,
            BabyPosition = new Position16<RoomPixels>(100, 80),
        };
        typeof(RoomEnemySystem).GetField("<Ridley>k__BackingField", flags)!.SetValue(enemies, baby);
        var nativeBaby = new OamBuffer();
        nativeBaby.BeginFrame();
        DrawImportedEnemySpritemap(rom, nativeBaby, new EnemySpritemapDraw { Bank = CeresBabyInstructionProgramDefinitions.Bank, Pointer = CeresBabyInstructionProgramDefinitions.HorizontalFrame, Origin = new(100, 80), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = true });
        nativeBaby.FinalizeFrame();
        var installedBaby = new OamBuffer();
        installedBaby.BeginFrame();
        enemies.CeresRidleyBehavior.DrawCeresRidleyImmediateBabyAndDoor(installedBaby, new(0, 0));
        installedBaby.FinalizeFrame();
        AssertTrue(nativeBaby.ImageEquals(installedBaby),
            "private Ceres Baby draw matches native OAM with sprite ROM reads forbidden");
        AssertEqual(CeresBabyInstructionProgramDefinitions.HorizontalFrame,
            baby.BabyCurrentSpritemap, "installed art retains the authored Baby pose");
        // `Initial` opens with GotoXIfMoving, so the first draw runs that ASM instruction
        // and installs the following frame through $A6:DBFE with an elapsed timer of one.
        AssertEqual((ushort)1, baby.BabyInstructionTimer,
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
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), Spritemaps = EnemySpritemapCatalog.Load(
                new MemoryStream(editedJson, writable: false)) });
        baby.BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial;
        baby.BabyInstructionTimer = 1;
        var editedBaby = new OamBuffer();
        editedBaby.BeginFrame();
        enemies.CeresRidleyBehavior.DrawCeresRidleyImmediateBabyAndDoor(editedBaby, new(0, 0));
        editedBaby.FinalizeFrame();
        AssertEqual(nativeBaby.GetEntry(0).X, editedBaby.GetEntry(0).X,
            "Baby art edit preserves the authored world X position");
        AssertEqual(nativeBaby.GetEntry(0).Y + 1, editedBaby.GetEntry(0).Y,
            "Baby art edit changes the visible Y offset");
        AssertEqual((ushort)1, baby.BabyInstructionTimer,
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
        // Cross the tiered-JIT hot-loop threshold before measuring steady-state
        // lookups; that one-time runtime transition is not a per-frame allocation.
        for (int index = 0; index < 65536; index++)
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

    private static CartridgeReadGuard NewCeresBabyInstructionReadGuard(
        ISnesAddressSpace source, bool blockBabyFrames = false) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    CeresBabyInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CeresBabyInstructionProgramDefinitions.IsCompiledSpritemapByte(address) ||
                    CeresBabyInstructionProgramDefinitions.IsCompiledPaletteByte(address) ||
                    address is >= CeresRidleyPaletteRomData.BabyColors and <
                        CeresRidleyPaletteRomData.BabyColors +
                        CeresRidleyPaletteRomData.BabyRowCount *
                        CeresRidleyPaletteRomData.BabyColorCount * sizeof(ushort) ||
                    (blockBabyFrames && address is >= 0xa6bffd and < 0xa6c04e),
                address => $"Production read compiled Ceres Baby control/selector byte ${address:X6}.");

    [VerificationCommand("--ceres-baby-instruction-mechanics")]
    internal static int CeresBabyInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCeresBabyInstructionProgramDefinitions), () => VerifyCeresBabyInstructionProgramDefinitions());
        return 0;
    }
}
