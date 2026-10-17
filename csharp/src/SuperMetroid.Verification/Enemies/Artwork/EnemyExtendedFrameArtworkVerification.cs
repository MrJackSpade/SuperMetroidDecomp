using SuperMetroid.Tooling;
using SuperMetroid.Core;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Areas.Ceres.CeresSteamCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireTongueCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkCommonEmptyVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkSporeSpawnSelectorsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Enemies.MaridiaSnails.MaridiaLargeSnailCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed extended enemy OAM compositions, editable frame migration, and guarded visual reads against native data.</summary>
internal static class EnemyExtendedFrameArtworkVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>The one pose Torizo's shared falling-left lists show.</summary>
    private static ushort TorizoFallingLeftFrame => TorizoAnimations.Spritemaps(TorizoBodyProgram.FacingLeftFalling1).Single();

    /// <summary>
    /// Collects the Draygon presentation selectors that do not name an installed OAM frame and
    /// checks each such native frame holds a BG2 command component.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon's bank-$A5 presentation selectors and BG2 frame commands")]
    private static HashSet<ushort> VerifyNativeDraygonBg2FrameSelectors(
        ISnesAddressSpace rom, HashSet<ushort> draygonOamPointers)
    {
        var draygonBg2Pointers = new HashSet<ushort>();
        // The Draygon oracle matches each typed frame to the operand of its native list frame.
        for (int index = 0; index < DraygonFrameDefinitions.FrameCount; index++)
        {
            ushort pointer = new DraygonFrame(index).Spritemap;
            if (draygonOamPointers.Contains(pointer))
                continue;
            draygonBg2Pointers.Add(pointer);
            int count = rom.ReadCartridgeByte(0xa50000 | pointer);
            AssertTrue(Enumerable.Range(0, count).Any(component =>
            {
                ushort record = unchecked((ushort)(pointer + 2 + component * 8 + 4));
                ushort sprite = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa50000 | record);
                return CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa50000 | sprite)
                    == 0xfffe;
            }), $"unextracted Draygon frame $A5:{pointer:X4} is a BG2 command");
        }
        return draygonBg2Pointers;
    }

    /// <summary>
    /// The stock catalog from <paramref name="stockDirectory"/> with its extended frames replaced, as the installation
    /// loader selects an extended-frame override; nothing is derived from the frames when the catalog is built.
    /// </summary>
    private static EnemyTileArtworkCatalog StockWithExtendedFrames(string stockDirectory, EnemyExtendedFrameCatalog extendedFrames)
    {
        EnemyTileArtworkCatalog catalog = EnemyTileArtworkFiles.Load(stockDirectory, null);
        catalog.ExtendedFrames = Ensure.NotNull(extendedFrames);
        return catalog;
    }

    /// <summary>Compares installed Pirate and boss extended frames to native OAM, then checks overrides, collision separation, and schema upgrades.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledEnemyExtendedFrames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.ExtendedFrames is not null,
            "installed enemy catalog contains extended visual compositions");
        var guard = NewExtendedVisualReadGuard(rom, out Action<byte, ushort> guardBlockFrame);
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            guardBlockFrame(frame.Bank, frame.Pointer);
            AssertTrue((stock.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGet(frame.Bank, frame.Pointer, out _),
                $"installed extended frame {frame.Name} exists");
        }
        ushort emptyPointer = EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap;
        guardBlockFrame(EnemyExtendedFrameDefinitions.Bank, emptyPointer);
        AssertTrue((stock.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGet(EnemyExtendedFrameDefinitions.Bank,
                emptyPointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> empty) &&
                   empty.IsEmpty,
            "walking Pirate initial empty frame is a compiled draw identity");
        OamBuffer nativeEmpty = DrawReferenceExtendedFrame(rom, EnemyExtendedFrameDefinitions.Bank, emptyPointer,
            new(0x0040, 0x0080));
        OamBuffer installedEmpty = DrawExtended(stock, emptyPointer,
            new(0x0040, 0x0080));
        AssertTrue(nativeEmpty.ImageEquals(installedEmpty) &&
                   nativeEmpty.NextByteOffset == installedEmpty.NextByteOffset,
            "walking Pirate common empty frame draws without ROM reads");
        Suite(nameof(VerifySharedEmptyExtendedFrames), () => VerifySharedEmptyExtendedFrames(rom, stock));
        Suite(nameof(VerifyInstalledCeresSteamInstructionFrames), () => VerifyInstalledCeresSteamInstructionFrames(stock));
        Suite(nameof(VerifyInstalledOumVisualSelectors), () => VerifyInstalledOumVisualSelectors(stock));
        Suite(nameof(VerifyInstalledCrocomireTongueVisualSelectors), () => VerifyInstalledCrocomireTongueVisualSelectors(stock));
        AssertEqual(EnemyExtendedFrameDefinitions.ExpectedFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Length,
            "walking/wall Pirate distinct extended-frame count");
        AssertEqual(EnemyExtendedFrameDefinitions.WallFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("wall_pirate_", StringComparison.Ordinal)),
            "all wall-Pirate visual frame identities are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.NinjaFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("ninja_pirate_", StringComparison.Ordinal)),
            "all ninja-Pirate visual frame identities are installed");
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            foreach ((ushort x, ushort y) in new (ushort, ushort)[]
                     {
                         (0x0040, 0x0080),
                         (0x01f8, 0x00fc),
                         (0x0000, 0x0000),
                     })
            {
                OamBuffer native = DrawReferenceExtendedFrame(rom,
                    frame.Bank, frame.Pointer, new(x, y));
                OamBuffer installed = DrawExtendedForBank(stock, frame.Bank, frame.Pointer, new(x, y));
                AssertTrue(native.ImageEquals(installed) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed extended {frame.Name} matches native OAM at {x:X4},{y:X4}");
            }
        }
        Console.WriteLine("  Extended enemy OAM: every installed composition matches native component offsets, clipping, and packed sprite bytes at all three fixture origins.");
        Suite(nameof(VerifyInstalledSporeSpawnSelectorPrograms), () => VerifyInstalledSporeSpawnSelectorPrograms(rom, stock));
        AssertEqual(EnemyExtendedFrameDefinitions.RidleyFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("ridley_body_", StringComparison.Ordinal)),
            "all Ceres/Lower Norfair Ridley body frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CeresSteamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("ceres_steam_oam_", StringComparison.Ordinal)),
            "all selected Ceres steam visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.OumFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("oum_oam_", StringComparison.Ordinal)),
            "all selected Oum visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CrocomireOamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("crocomire_oam_", StringComparison.Ordinal)),
            "all selected Crocomire tongue visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CrocomireBodyFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("crocomire_body_oam_", StringComparison.Ordinal)),
            "all selected Crocomire fight-body OAM frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.KraidArmFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("kraid_arm_oam_", StringComparison.Ordinal)),
            "all Kraid arm visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoAwakeningFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_awake_", StringComparison.Ordinal)),
            "all Golden Torizo awakening visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoWalkingFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_walk_left_", StringComparison.Ordinal)),
            "all Golden Torizo walking-left visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightwardFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_rightward_", StringComparison.Ordinal)),
            "all Golden Torizo turning/walking-right visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoJumpBackFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("torizo_jump_back_", StringComparison.Ordinal) &&
                    !frame.Name.StartsWith("torizo_jump_back_left_", StringComparison.Ordinal)),
            "all shared Torizo jump-back visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoJumpBackLeftNewFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("torizo_jump_back_left_", StringComparison.Ordinal)),
            "both new left-facing Torizo jump-back visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_right_orb_", StringComparison.Ordinal)),
            "all Golden Torizo right-orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoLeftOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_left_orb_", StringComparison.Ordinal)),
            "all Golden Torizo left-orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightSonicFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_right_sonic_", StringComparison.Ordinal)),
            "all Golden Torizo right-sonic visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoFallingLeftFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("torizo_falling_left_", StringComparison.Ordinal)),
            "shared Torizo falling-left visual frame is installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoLeftFootOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_left_foot_orb_", StringComparison.Ordinal)),
            "all new Golden Torizo left-foot orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.DraygonOamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal)),
            "all selected ordinary-OAM Draygon extended frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.SporeSpawnOamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)),
            "all selected Spore Spawn extended frames are installed");
        var draygonOamPointers = EnemyExtendedFrameDefinitions.Frames
            .Where(frame => frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        HashSet<ushort> draygonBg2Pointers = VerifyNativeDraygonBg2FrameSelectors(rom, draygonOamPointers);
        AssertEqual(34, draygonBg2Pointers.Count,
            "Draygon presentation selectors partition into 48 OAM and 34 BG2 frames");

        string fileName = EnemyExtendedFrameDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock extended enemy compositions are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        EnemyExtendedFrameDocument document =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string ridleyName = "ridley_body_E983";
        EnemyExtendedVisualComponent ridleyFirst = document.Frames[ridleyName][0];
        document.Frames[ridleyName][0] = ridleyFirst with
        {
            OffsetX = ridleyFirst.OffsetX + 1,
        };
        const string draygonOamName = "draygon_oam_A2DF";
        EnemyExtendedVisualComponent draygonFirst = document.Frames[draygonOamName][0];
        document.Frames[draygonOamName][0] = draygonFirst with
        {
            OffsetX = draygonFirst.OffsetX + 1,
        };
        const string steamName = "ceres_steam_oam_F142";
        EnemyExtendedVisualComponent steamFirst = document.Frames[steamName][0];
        document.Frames[steamName][0] = steamFirst with
        {
            OffsetX = steamFirst.OffsetX + 1,
        };
        const string oumName = "oum_oam_CB87";
        EnemyExtendedVisualComponent oumFirst = document.Frames[oumName][0];
        document.Frames[oumName][0] = oumFirst with
        {
            OffsetX = oumFirst.OffsetX + 1,
        };
        const string crocomireName = "crocomire_oam_C65E";
        EnemyExtendedVisualComponent crocomireFirst = document.Frames[crocomireName][0];
        document.Frames[crocomireName][0] = crocomireFirst with
        {
            OffsetX = crocomireFirst.OffsetX + 1,
        };
        string ridleyOverrideDirectory = Path.Combine(stockDirectory,
            "ridley-composition-overrides");
        Directory.CreateDirectory(ridleyOverrideDirectory);
        File.WriteAllBytes(Path.Combine(ridleyOverrideDirectory,
            EnemyExtendedFrameDefinitions.FileName),
            JsonSerializer.SerializeToUtf8Bytes(document, CamelCaseJson));
        EnemyTileArtworkCatalog editedRidley = EnemyTileArtworkFiles.Load(
            stockDirectory, ridleyOverrideDirectory);
        OamBuffer stockRidley = DrawExtendedForBank(stock, 0xa6, 0xe983, new(0x0040, 0x0080));
        OamBuffer movedRidley = DrawExtendedForBank(editedRidley, 0xa6, 0xe983, new(0x0040, 0x0080));
        OamBuffer stockDraygonOam = DrawExtendedForBank(stock, 0xa5, 0xa2df, new(0x0040, 0x0080));
        OamBuffer movedDraygonOam = DrawExtendedForBank(editedRidley, 0xa5, 0xa2df, new(0x0040, 0x0080));
        OamBuffer stockSteam = DrawExtendedForBank(stock, 0xa6, 0xf142, new(0x0040, 0x0080));
        OamBuffer movedSteam = DrawExtendedForBank(editedRidley, 0xa6, 0xf142, new(0x0040, 0x0080));
        OamBuffer stockOum = DrawExtendedForBank(stock, 0xa2, 0xcb87, new(0x0040, 0x0080));
        OamBuffer movedOum = DrawExtendedForBank(editedRidley, 0xa2, 0xcb87, new(0x0040, 0x0080));
        OamBuffer stockCrocomire = DrawExtendedForBank(stock, 0xa4, 0xc65e, new(0x0040, 0x0080));
        OamBuffer movedCrocomire = DrawExtendedForBank(editedRidley, 0xa4, 0xc65e, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockCrocomire.LowTable[0] + 1)),
            movedCrocomire.LowTable[0],
            "editable Crocomire tongue composition moves live OAM by one pixel");
        AssertEqual(unchecked((byte)(stockOum.LowTable[0] + 1)),
            movedOum.LowTable[0],
            "editable Oum composition moves live OAM by one pixel");
        AssertEqual(unchecked((byte)(stockSteam.LowTable[0] + 1)),
            movedSteam.LowTable[0],
            "editable Ceres steam composition moves live OAM by one pixel");
        AssertEqual(unchecked((byte)(stockDraygonOam.LowTable[0] + 1)),
            movedDraygonOam.LowTable[0],
            "editable Draygon extended OAM offset changes live drawing");
        AssertEqual(stockDraygonOam.LowTable[1], movedDraygonOam.LowTable[1],
            "Draygon extended visual X edit leaves Y placement unchanged");
        AssertEqual(unchecked((byte)(stockRidley.LowTable[0] + 1)),
            movedRidley.LowTable[0],
            "editable Ridley body component moves live OAM by one pixel");
        document.Frames[ridleyName][0] = ridleyFirst;
        const string editedName = "walking_pirate_walk_left_0";
        EnemyExtendedVisualComponent first = document.Frames[editedName][0];
        document.Frames[editedName][0] = first with
        {
            OffsetX = first.OffsetX + 1,
        };
        string overrideDirectory = Path.Combine(stockDirectory,
            "extended-composition-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        // Override cases check the spritemap catalog against stock loaded once, exactly as the
        // installation loader does for this file; two full loads above prove the disk wiring.
        EnemyExtendedFrameCatalog LoadExtendedFrameOverride()
        {
            using FileStream json = File.OpenRead(overridePath);
            return EnemyExtendedFrameCatalog.Load(json, stock.ExtendedFrames);
        }
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort editedPointer = EnemyExtendedFrameDefinitions.Frames
            .Single(frame => frame.Name == editedName).Pointer;
        OamBuffer stockOam = DrawExtended(stock, editedPointer, new(0x0040, 0x0080));
        OamBuffer editedOam = DrawExtended(edited, editedPointer, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0],
            "editable walking Pirate component moves live OAM by one pixel");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "walking Pirate X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, editedPointer),
            GetTouchCallback(edited, editedPointer),
            "editable walking Pirate component does not move the compiled hitbox");
        AssertTrue((EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory) .ExtendedFrames.")).TryGet(EnemyExtendedFrameDefinitions.Bank,
                    editedPointer, out _),
            "extended composition override survives catalog reload");

        // Existing v1 overrides were authored before wall-Pirate frames were
        // extracted. Preserve their validated walking edits and fill only the
        // new identities from the hash-checked current stock catalog.
        var legacyDocument = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.First,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.WalkingFrameCount),
        };
        byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(legacyDocument,
            CamelCaseJson);
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(legacyJson, writable: false)),
            "legacy extended override needs complete verified stock to merge");
        File.WriteAllBytes(overridePath, legacyJson);
        EnemyTileArtworkCatalog legacy = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer legacyWalking = DrawExtended(legacy, editedPointer, new(0x0040, 0x0080));
        AssertTrue(legacyWalking.ImageEquals(editedOam),
            "v1 walking-Pirate artwork edit survives the v2 catalog update");

        const string wallName = "wall_pirate_climb_left_0";
        ushort wallPointer = EnemyExtendedFrameDefinitions.Frames
            .Single(frame => frame.Name == wallName).Pointer;
        OamBuffer stockWall = DrawExtended(stock, wallPointer, new(0x0040, 0x0080));
        OamBuffer legacyWall = DrawExtended(legacy, wallPointer, new(0x0040, 0x0080));
        AssertTrue(stockWall.ImageEquals(legacyWall),
            "v1 override inherits stock wall-Pirate frames");

        EnemyExtendedVisualComponent wallFirst = document.Frames[wallName][0];
        document.Frames[wallName][0] = wallFirst with
        {
            OffsetX = wallFirst.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        EnemyTileArtworkCatalog editedWall = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedWall = DrawExtended(editedWall, wallPointer, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockWall.LowTable[0] + 1)),
            movedWall.LowTable[0],
            "editable wall-Pirate component moves live OAM by one pixel");
        AssertEqual(stockWall.LowTable[1], movedWall.LowTable[1],
            "wall-Pirate X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, wallPointer),
            GetTouchCallback(editedWall, wallPointer),
            "editable wall-Pirate component does not move its compiled hitbox");
        AssertTrue(LoadExtendedFrameOverride().TryGet(EnemyExtendedFrameDefinitions.Bank,
                    wallPointer, out _),
            "edited wall-Pirate composition survives catalog reload");

        // The second published schema includes edited walking and wall frames,
        // but no Ninja identities. It must inherit only those new frames from
        // current stock without dropping either family's prior edit.
        var versionTwoDocument = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.Previous,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.WalkingFrameCount + EnemyExtendedFrameDefinitions.WallFrameCount),
        };
        byte[] versionTwoJson = JsonSerializer.SerializeToUtf8Bytes(versionTwoDocument,
            CamelCaseJson);
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(versionTwoJson, writable: false)),
            "v2 extended override requires complete verified v3 stock");
        File.WriteAllBytes(overridePath, versionTwoJson);
        EnemyTileArtworkCatalog versionTwo = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer versionTwoWalking = DrawExtended(versionTwo, editedPointer, new(0x0040, 0x0080));
        OamBuffer versionTwoWall = DrawExtended(versionTwo, wallPointer, new(0x0040, 0x0080));
        AssertTrue(versionTwoWalking.LowTable.SequenceEqual(editedOam.LowTable) &&
                   versionTwoWall.LowTable.SequenceEqual(movedWall.LowTable),
            "v2 override preserves both walking and wall-Pirate art edits");

        EnemyExtendedFrameDefinition ninjaFrame = EnemyExtendedFrameDefinitions
            .Frames.First(frame => frame.Name.StartsWith(
                "ninja_pirate_", StringComparison.Ordinal));
        string ninjaName = ninjaFrame.Name;
        ushort ninjaPointer = ninjaFrame.Pointer;
        OamBuffer stockNinja = DrawExtended(stock, ninjaPointer, new(0x0040, 0x0080));
        OamBuffer versionTwoNinja = DrawExtended(versionTwo, ninjaPointer, new(0x0040, 0x0080));
        OamBuffer versionOneNinja = DrawExtended(legacy, ninjaPointer, new(0x0040, 0x0080));
        AssertTrue(stockNinja.ImageEquals(versionTwoNinja) &&
                   stockNinja.ImageEquals(versionOneNinja),
            "v1 and v2 overrides inherit verified stock Ninja frames");

        EnemyExtendedVisualComponent ninjaFirst = document.Frames[ninjaName][0];
        document.Frames[ninjaName][0] = ninjaFirst with
        {
            OffsetX = ninjaFirst.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        EnemyTileArtworkCatalog editedNinja = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedNinja = DrawExtended(editedNinja, ninjaPointer, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockNinja.LowTable[0] + 1)),
            movedNinja.LowTable[0],
            "editable Ninja component moves live OAM by one pixel");
        AssertEqual(stockNinja.LowTable[1], movedNinja.LowTable[1],
            "Ninja X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, ninjaPointer),
            GetTouchCallback(editedNinja, ninjaPointer),
            "editable Ninja component does not move its compiled hitbox");
        AssertTrue(LoadExtendedFrameOverride().TryGet(EnemyExtendedFrameDefinitions.Bank,
                    ninjaPointer, out _),
            "edited Ninja composition survives catalog reload");

        var preBindings = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreDisplayBindings,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PirateFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            preBindings, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedBindings = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        AssertTrue(movedNinja.LowTable.SequenceEqual(DrawExtended(
                upgradedBindings, ninjaPointer, new(0x0040, 0x0080)).LowTable),
            "version-three extended override retains edited art and stock display bindings");

        // The previous current schema had all Pirate art and editable display
        // bindings but no Ridley body. Preserve both types of player edit when
        // filling its new Ridley identities from verified stock.
        var versionFour = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PirateDisplayBindings,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PirateFrameCount),
            DisplayFrames = HistoricalExtendedEntries((document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames.")), EnemyExtendedFrameDefinitions.PirateFrameCount),
        };
        const string sourceNameForLegacyBinding = "walking_pirate_walk_left_0";
        const string targetNameForLegacyBinding = "walking_pirate_walk_left_1";
        versionFour.DisplayFrames[sourceNameForLegacyBinding] = targetNameForLegacyBinding;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFour, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionFour = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        ushort legacySourcePointer = EnemyExtendedFrameDefinitions.Frames
            .Single(frame => frame.Name == sourceNameForLegacyBinding).Pointer;
        ushort legacyTargetPointer = EnemyExtendedFrameDefinitions.Frames
            .Single(frame => frame.Name == targetNameForLegacyBinding).Pointer;
        AssertTrue(DrawExtended(upgradedVersionFour, legacySourcePointer,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(DrawExtended(stock, legacyTargetPointer, new(0x0040, 0x0080)).LowTable),
            "version-four visual binding survives Ridley-frame migration");
        AssertTrue((upgradedVersionFour.ExtendedFrames ?? throw new InvalidOperationException("Verification requires upgradedVersionFour.ExtendedFrames.")).TryGet(0xa6, 0xe983, out _),
            "version-four override inherits stock Ridley body art");

        // The immediately preceding schema includes Ridley but not Draygon.
        // Keep edited Pirate/Ridley compositions and display bindings while
        // filling the new boss's OAM-only frames from hash-checked stock.
        var versionFive = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreDraygon,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreDraygonFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreDraygonFrameCount),
        };
        versionFive.DisplayFrames[sourceNameForLegacyBinding] = targetNameForLegacyBinding;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFive, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionFive = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        AssertTrue(DrawExtended(upgradedVersionFive, legacySourcePointer,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(DrawExtended(stock, legacyTargetPointer, new(0x0040, 0x0080)).LowTable),
            "version-five override retains its authored Pirate display binding");
        AssertTrue((upgradedVersionFive.ExtendedFrames ?? throw new InvalidOperationException("Verification requires upgradedVersionFive.ExtendedFrames.")).TryGet(0xa5, 0xa2df, out _),
            "version-five override inherits stock Draygon OAM art");

        // V6 published the Spore Spawn roots with Draygon-prefixed author keys.
        // Migrate the keys by unchanged bank/pointer identity so old artwork
        // and display bindings remain effective after the family correction.
        EnemyExtendedFrameDocument beforeSporeIdentity =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string currentSporeName = "spore_spawn_oam_EE6F";
        const string legacySporeName = "draygon_oam_EE6F";
        var versionSixFrames = HistoricalExtendedEntries(beforeSporeIdentity.Frames, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount).ToDictionary(entry => LegacyExtendedFrameName(entry.Key),
                entry => entry.Value, StringComparer.Ordinal);
        var versionSixBindings = HistoricalExtendedEntries((beforeSporeIdentity.DisplayFrames ?? throw new InvalidOperationException("Verification requires beforeSporeIdentity.DisplayFrames.")), EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount).ToDictionary(entry => LegacyExtendedFrameName(entry.Key),
                entry => LegacyExtendedFrameName(entry.Value),
                StringComparer.Ordinal);
        EnemyExtendedVisualComponent legacySporeFirst =
            versionSixFrames[legacySporeName][0];
        versionSixFrames[legacySporeName][0] = legacySporeFirst with
        {
            OffsetX = legacySporeFirst.OffsetX + 1,
        };
        versionSixBindings["draygon_oam_EE65"] = legacySporeName;
        var versionSix = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreSporeIdentity,
            Frames = versionSixFrames,
            DisplayFrames = versionSixBindings,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSix, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionSix = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockSpore = DrawExtendedForBank(stock, 0xa5, 0xee6f, new(0x0040, 0x0080));
        OamBuffer editedSpore = DrawExtendedForBank(upgradedVersionSix, 0xa5, 0xee6f, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockSpore.LowTable[0] + 1)),
            editedSpore.LowTable[0],
            "version-six Draygon-named Spore Spawn art edit survives migration");
        OamBuffer reboundSpore = DrawExtendedForBank(upgradedVersionSix, 0xa5, 0xee65, new(0x0040, 0x0080));
        AssertTrue(reboundSpore.ImageEquals(editedSpore),
            "version-six Spore Spawn display binding survives key migration");
        AssertTrue(beforeSporeIdentity.Frames.ContainsKey(currentSporeName) &&
                   !beforeSporeIdentity.Frames.ContainsKey(legacySporeName),
            "current stock exposes the corrected Spore Spawn author key");

        var versionSeven = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreCeresSteam,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSeven, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionSeven = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer inheritedSteam = DrawExtendedForBank(upgradedVersionSeven, 0xa6, 0xf142, new(0x0040, 0x0080));
        AssertTrue(inheritedSteam.ImageEquals(stockSteam),
            "version-seven override inherits verified stock Ceres steam art");
        AssertTrue(DrawExtended(upgradedVersionSeven, editedPointer,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(editedOam.LowTable),
            "version-seven Pirate edit survives the Ceres steam schema update");

        var versionEight = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreOum,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreOumFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreOumFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionEight, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionEight = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer inheritedOum = DrawExtendedForBank(upgradedVersionEight, 0xa2, 0xcb87, new(0x0040, 0x0080));
        AssertTrue(inheritedOum.ImageEquals(stockOum),
            "version-eight override inherits verified stock Oum art");
        AssertTrue(DrawExtendedForBank(upgradedVersionEight, 0xa6, 0xf142,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(movedSteam.LowTable),
            "version-eight Ceres steam edit survives the Oum schema update");

        var versionNine = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreCrocomire,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreCrocomireFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreCrocomireFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionNine, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionNine = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer inheritedCrocomire = DrawExtendedForBank(upgradedVersionNine, 0xa4, 0xc65e, new(0x0040, 0x0080));
        AssertTrue(inheritedCrocomire.ImageEquals(stockCrocomire),
            "version-nine override inherits verified stock Crocomire tongue art");
        AssertTrue(DrawExtendedForBank(upgradedVersionNine, 0xa2, 0xcb87,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(movedOum.LowTable),
            "version-nine Oum edit survives the Crocomire schema update");

        var versionTen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreCrocomireBody,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount,
            versionTen.Frames.Count, "version-ten extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTen, CamelCaseJson));
        EnemyExtendedFrameCatalog upgradedVersionTen = LoadExtendedFrameOverride();
        AssertTrue(upgradedVersionTen.TryGetDisplay(0xa4, 0xc2ec, out _),
            "version-ten override inherits Crocomire's mixed fight-body OAM");

        var versionEleven = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreBombTorizo,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount,
            versionEleven.Frames.Count, "version-eleven extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionEleven, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionEleven = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockDormantTorizo = DrawExtendedForBank(stock, 0xaa, 0x87d0, new(0x0040, 0x0080));
        OamBuffer inheritedDormantTorizo = DrawExtendedForBank(
            upgradedVersionEleven, 0xaa, 0x87d0, new(0x0040, 0x0080));
        AssertTrue(stockDormantTorizo.LowTable.SequenceEqual(
                       inheritedDormantTorizo.LowTable) &&
                   stockDormantTorizo.HighTable.SequenceEqual(
                       inheritedDormantTorizo.HighTable),
            "version-eleven override inherits verified stock dormant Torizo art");
        AssertTrue(DrawExtended(upgradedVersionEleven, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-eleven migration retains an edited Pirate frame");

        var versionTwelve = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizo,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount,
            versionTwelve.Frames.Count, "version-twelve extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwelve, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionTwelve = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockInitialGoldenTorizo = DrawExtendedForBank(stock, 0xaa, 0xaa30, new(0x0040, 0x0080));
        OamBuffer inheritedInitialGoldenTorizo = DrawExtendedForBank(
            upgradedVersionTwelve, 0xaa, 0xaa30, new(0x0040, 0x0080));
        AssertTrue(stockInitialGoldenTorizo.LowTable.SequenceEqual(
                       inheritedInitialGoldenTorizo.LowTable) &&
                   stockInitialGoldenTorizo.HighTable.SequenceEqual(
                       inheritedInitialGoldenTorizo.HighTable),
            "version-twelve override inherits verified stock initial Golden Torizo art");
        AssertTrue(DrawExtended(upgradedVersionTwelve, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-twelve migration retains an edited Pirate frame");

        var versionThirteen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreKraidArm,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreKraidArmFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreKraidArmFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreKraidArmFrameCount,
            versionThirteen.Frames.Count, "version-thirteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionThirteen, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionThirteen = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockKraidArm = DrawExtendedForBank(stock, 0xa7, 0x90fd, new(0x0040, 0x0080));
        OamBuffer inheritedKraidArm = DrawExtendedForBank(
            upgradedVersionThirteen, 0xa7, 0x90fd, new(0x0040, 0x0080));
        AssertTrue(stockKraidArm.ImageEquals(inheritedKraidArm),
            "version-thirteen override inherits verified stock Kraid arm art");
        AssertTrue(DrawExtended(upgradedVersionThirteen, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-thirteen migration retains an edited Pirate frame");

        var versionFourteen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizoAwakening,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount,
            versionFourteen.Frames.Count,
            "version-fourteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFourteen, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionFourteen = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockAwakeGoldenTorizo = DrawExtendedForBank(stock, 0xaa, 0xaa5e, new(0x0040, 0x0080));
        OamBuffer inheritedAwakeGoldenTorizo = DrawExtendedForBank(
            upgradedVersionFourteen, 0xaa, 0xaa5e, new(0x0040, 0x0080));
        AssertTrue(stockAwakeGoldenTorizo.LowTable.SequenceEqual(
                       inheritedAwakeGoldenTorizo.LowTable) &&
                   stockAwakeGoldenTorizo.HighTable.SequenceEqual(
                       inheritedAwakeGoldenTorizo.HighTable),
            "version-fourteen override inherits verified Golden Torizo awakening art");
        AssertTrue(DrawExtended(upgradedVersionFourteen, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-fourteen migration retains an edited Pirate frame");

        var versionFifteen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizoWalking,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount,
            versionFifteen.Frames.Count,
            "version-fifteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFifteen, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionFifteen = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockWalkingGoldenTorizo = DrawExtendedForBank(stock, 0xaa, 0xa4fa, new(0x0040, 0x0080));
        OamBuffer inheritedWalkingGoldenTorizo = DrawExtendedForBank(
            upgradedVersionFifteen, 0xaa, 0xa4fa, new(0x0040, 0x0080));
        AssertTrue(stockWalkingGoldenTorizo.LowTable.SequenceEqual(
                       inheritedWalkingGoldenTorizo.LowTable) &&
                   stockWalkingGoldenTorizo.HighTable.SequenceEqual(
                       inheritedWalkingGoldenTorizo.HighTable),
            "version-fifteen override inherits Golden Torizo walking art");
        AssertTrue(DrawExtended(upgradedVersionFifteen, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-fifteen migration retains an edited Pirate frame");

        var versionSixteen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizoRightward,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount,
            versionSixteen.Frames.Count,
            "version-sixteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSixteen, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionSixteen = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockRightwardGoldenTorizo = DrawExtendedForBank(stock, 0xaa, 0xa4f0, new(0x0040, 0x0080));
        OamBuffer inheritedRightwardGoldenTorizo = DrawExtendedForBank(
            upgradedVersionSixteen, 0xaa, 0xa4f0, new(0x0040, 0x0080));
        AssertTrue(stockRightwardGoldenTorizo.LowTable.SequenceEqual(
                       inheritedRightwardGoldenTorizo.LowTable) &&
                   stockRightwardGoldenTorizo.HighTable.SequenceEqual(
                       inheritedRightwardGoldenTorizo.HighTable),
            "version-sixteen override inherits Golden Torizo turning-right art");
        AssertTrue(DrawExtended(upgradedVersionSixteen, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-sixteen migration retains an edited Pirate frame");

        var versionSeventeen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreTorizoJumpBack,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount,
            versionSeventeen.Frames.Count,
            "version-seventeen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSeventeen, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionSeventeen = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockJumpBackTorizo = DrawExtendedForBank(stock, 0xaa, 0xb048, new(0x0040, 0x0080));
        OamBuffer inheritedJumpBackTorizo = DrawExtendedForBank(
            upgradedVersionSeventeen, 0xaa, 0xb048, new(0x0040, 0x0080));
        AssertTrue(stockJumpBackTorizo.LowTable.SequenceEqual(
                       inheritedJumpBackTorizo.LowTable) &&
                   stockJumpBackTorizo.HighTable.SequenceEqual(
                       inheritedJumpBackTorizo.HighTable),
            "version-seventeen override inherits Torizo jump-back art");
        AssertTrue(DrawExtended(upgradedVersionSeventeen, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-seventeen migration retains an edited Pirate frame");

        var versionEighteen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizoRightOrb,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount,
            versionEighteen.Frames.Count,
            "version-eighteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionEighteen, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionEighteen = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockRightOrbTorizo = DrawExtendedForBank(stock, 0xaa, 0xac88, new(0x0040, 0x0080));
        OamBuffer inheritedRightOrbTorizo = DrawExtendedForBank(
            upgradedVersionEighteen, 0xaa, 0xac88, new(0x0040, 0x0080));
        AssertTrue(stockRightOrbTorizo.LowTable.SequenceEqual(
                       inheritedRightOrbTorizo.LowTable) &&
                   stockRightOrbTorizo.HighTable.SequenceEqual(
                       inheritedRightOrbTorizo.HighTable),
            "version-eighteen override inherits Golden Torizo right-orb art");
        AssertTrue(DrawExtended(upgradedVersionEighteen, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-eighteen migration retains an edited Pirate frame");

        var versionNineteen = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizoRightSonic,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount,
            versionNineteen.Frames.Count,
            "version-nineteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionNineteen, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionNineteen = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockRightSonicTorizo = DrawExtendedForBank(stock, 0xaa, 0xabec, new(0x0040, 0x0080));
        OamBuffer inheritedRightSonicTorizo = DrawExtendedForBank(
            upgradedVersionNineteen, 0xaa, 0xabec, new(0x0040, 0x0080));
        AssertTrue(stockRightSonicTorizo.LowTable.SequenceEqual(
                       inheritedRightSonicTorizo.LowTable) &&
                   stockRightSonicTorizo.HighTable.SequenceEqual(
                       inheritedRightSonicTorizo.HighTable),
            "version-nineteen override inherits Golden Torizo right-sonic art");
        AssertTrue(DrawExtended(upgradedVersionNineteen, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-nineteen migration retains an edited Pirate frame");

        var versionTwenty = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreTorizoFallingLeft,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames, EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount,
            versionTwenty.Frames.Count,
            "version-twenty extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwenty, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionTwenty = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockFallingLeft = DrawExtendedForBank(stock, 0xaa, TorizoFallingLeftFrame, new(0x0040, 0x0080));
        OamBuffer inheritedFallingLeft = DrawExtendedForBank(upgradedVersionTwenty,
            0xaa, TorizoFallingLeftFrame, new(0x0040, 0x0080));
        AssertTrue(stockFallingLeft.ImageEquals(inheritedFallingLeft),
            "version-twenty override inherits the Torizo falling-left frame");
        AssertTrue(DrawExtended(upgradedVersionTwenty, editedPointer, new(0x0040, 0x0080)).LowTable.SequenceEqual(editedOam.LowTable),
            "version-twenty migration retains an edited Pirate frame");

        EnemyExtendedFrameDocument fallingLeftOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string fallingLeftName = "torizo_falling_left_B014";
        EnemyExtendedVisualComponent originalFallingLeftComponent =
            fallingLeftOverride.Frames[fallingLeftName][0];
        fallingLeftOverride.Frames[fallingLeftName][0] =
            originalFallingLeftComponent with
            {
                OffsetX = originalFallingLeftComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            fallingLeftOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedFallingLeft = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedFallingLeft = DrawExtendedForBank(editedFallingLeft,
            0xaa, TorizoFallingLeftFrame, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockFallingLeft.LowTable[0] + 1)),
            movedFallingLeft.LowTable[0],
            "editable Torizo falling-left component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                TorizoFallingLeftFrame,
                out var unchangedFallingLeftCollision) &&
                   unchangedFallingLeftCollision[0].Offset.X == -16 &&
                   unchangedFallingLeftCollision[0].HitboxList == 0x87c7,
            "Torizo falling-left cosmetic edit cannot move its physical component");

        var versionTwentyOne = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizoLeftFootOrb,
            Frames = HistoricalExtendedEntries(fallingLeftOverride.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount),
            DisplayFrames = HistoricalExtendedEntries((fallingLeftOverride.DisplayFrames ?? throw new InvalidOperationException("Verification requires fallingLeftOverride.DisplayFrames.")), EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount,
            versionTwentyOne.Frames.Count,
            "version-twenty-one extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwentyOne, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionTwentyOne = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockLeftFootOrb = DrawExtendedForBank(stock, 0xaa, 0xac06, new(0x0040, 0x0080));
        OamBuffer inheritedLeftFootOrb = DrawExtendedForBank(
            upgradedVersionTwentyOne, 0xaa, 0xac06, new(0x0040, 0x0080));
        AssertTrue(stockLeftFootOrb.ImageEquals(inheritedLeftFootOrb),
            "version-twenty-one override inherits Golden Torizo left-foot orb art");
        AssertTrue(DrawExtendedForBank(upgradedVersionTwentyOne, 0xaa, TorizoFallingLeftFrame,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(movedFallingLeft.LowTable),
            "version-twenty-one migration retains the edited falling-left frame");

        var versionTwentyTwo = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreTorizoJumpBackLeft,
            Frames = HistoricalExtendedEntries(fallingLeftOverride.Frames, EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount),
            DisplayFrames = HistoricalExtendedEntries(fallingLeftOverride.DisplayFrames, EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount,
            versionTwentyTwo.Frames.Count,
            "version-twenty-two extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwentyTwo, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionTwentyTwo = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockLeftJumpBack = DrawExtendedForBank(stock, 0xaa, 0xaffa, new(0x0040, 0x0080));
        OamBuffer inheritedLeftJumpBack = DrawExtendedForBank(
            upgradedVersionTwentyTwo, 0xaa, 0xaffa, new(0x0040, 0x0080));
        AssertTrue(stockLeftJumpBack.ImageEquals(inheritedLeftJumpBack),
            "version-twenty-two override inherits left-facing jump-back art");
        AssertTrue(DrawExtendedForBank(upgradedVersionTwentyTwo, 0xaa, TorizoFallingLeftFrame,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(movedFallingLeft.LowTable),
            "version-twenty-two migration retains the edited falling-left frame");

        var versionTwentyThree = new EnemyExtendedFrameDocument
        {
            Version = (int)EnemyExtendedFrameSchema.PreGoldenTorizoLeftOrb,
            Frames = HistoricalExtendedEntries(fallingLeftOverride.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount),
            DisplayFrames = HistoricalExtendedEntries(fallingLeftOverride.DisplayFrames, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount,
            versionTwentyThree.Frames.Count,
            "version-twenty-three extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwentyThree, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedVersionTwentyThree = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer stockLeftOrb = DrawExtendedForBank(stock, 0xaa, 0xa6ea, new(0x0040, 0x0080));
        OamBuffer inheritedLeftOrb = DrawExtendedForBank(
            upgradedVersionTwentyThree, 0xaa, 0xa6ea, new(0x0040, 0x0080));
        AssertTrue(stockLeftOrb.ImageEquals(inheritedLeftOrb),
            "version-twenty-three override inherits left-orb art");
        AssertTrue(DrawExtendedForBank(upgradedVersionTwentyThree, 0xaa, TorizoFallingLeftFrame,
                new(0x0040, 0x0080)).LowTable.SequenceEqual(movedFallingLeft.LowTable),
            "version-twenty-three migration retains the edited falling-left frame");

        EnemyExtendedFrameDocument leftFootOrbOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string leftFootOrbName = "golden_torizo_left_foot_orb_AC06";
        EnemyExtendedVisualComponent originalLeftFootOrbComponent =
            leftFootOrbOverride.Frames[leftFootOrbName][0];
        leftFootOrbOverride.Frames[leftFootOrbName][0] =
            originalLeftFootOrbComponent with
            {
                OffsetX = originalLeftFootOrbComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            leftFootOrbOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedLeftFootOrb = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedLeftFootOrb = DrawExtendedForBank(
            editedLeftFootOrb, 0xaa, 0xac06, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockLeftFootOrb.LowTable[0] + 1)),
            movedLeftFootOrb.LowTable[0],
            "editable Golden Torizo left-foot orb component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xac06, out var unchangedLeftFootOrbCollision) &&
                   unchangedLeftFootOrbCollision[0].Offset.X == 9 &&
                   unchangedLeftFootOrbCollision[0].HitboxList == 0x87c7,
            "Golden Torizo left-foot orb cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument rightSonicOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string rightSonicName = "golden_torizo_right_sonic_ABEC";
        EnemyExtendedVisualComponent originalRightSonicComponent =
            rightSonicOverride.Frames[rightSonicName][0];
        rightSonicOverride.Frames[rightSonicName][0] =
            originalRightSonicComponent with
            {
                OffsetX = originalRightSonicComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            rightSonicOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedRightSonic = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedRightSonic = DrawExtendedForBank(
            editedRightSonic, 0xaa, 0xabec, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockRightSonicTorizo.LowTable[0] + 1)),
            movedRightSonic.LowTable[0],
            "editable Golden Torizo right-sonic component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xabec, out var unchangedRightSonicCollision) &&
                   unchangedRightSonicCollision[0].Offset.X == 15 &&
                   unchangedRightSonicCollision[0].HitboxList == 0x87c7,
            "Golden Torizo right-sonic cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument rightOrbOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string rightOrbName = "golden_torizo_right_orb_AC88";
        EnemyExtendedVisualComponent originalRightOrbComponent =
            rightOrbOverride.Frames[rightOrbName][0];
        rightOrbOverride.Frames[rightOrbName][0] =
            originalRightOrbComponent with
            {
                OffsetX = originalRightOrbComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            rightOrbOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedRightOrb = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedRightOrb = DrawExtendedForBank(
            editedRightOrb, 0xaa, 0xac88, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockRightOrbTorizo.LowTable[0] + 1)),
            movedRightOrb.LowTable[0],
            "editable Golden Torizo right-orb component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xac88, out var unchangedRightOrbCollision) &&
                   unchangedRightOrbCollision[0].Offset.X == 15 &&
                   unchangedRightOrbCollision[0].HitboxList == 0x87c7,
            "Golden Torizo right-orb cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument jumpBackOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string jumpBackName = "torizo_jump_back_B048";
        EnemyExtendedVisualComponent originalJumpBackComponent =
            jumpBackOverride.Frames[jumpBackName][0];
        jumpBackOverride.Frames[jumpBackName][0] =
            originalJumpBackComponent with
            {
                OffsetX = originalJumpBackComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            jumpBackOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedJumpBack = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedJumpBack = DrawExtendedForBank(
            editedJumpBack, 0xaa, 0xb048, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockJumpBackTorizo.LowTable[0] + 1)),
            movedJumpBack.LowTable[0],
            "editable Torizo jump-back component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xb048, out var unchangedJumpBackCollision) &&
                   unchangedJumpBackCollision[0].Offset.X == 15 &&
                   unchangedJumpBackCollision[0].HitboxList == 0x87c7,
            "Torizo jump-back cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument goldenRightwardOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string goldenRightwardName = "golden_torizo_rightward_A4F0";
        EnemyExtendedVisualComponent originalRightwardComponent =
            goldenRightwardOverride.Frames[goldenRightwardName][0];
        goldenRightwardOverride.Frames[goldenRightwardName][0] =
            originalRightwardComponent with
            {
                OffsetX = originalRightwardComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            goldenRightwardOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedGoldenRightward = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedGoldenRightward = DrawExtendedForBank(
            editedGoldenRightward, 0xaa, 0xa4f0, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockRightwardGoldenTorizo.LowTable[0] + 1)),
            movedGoldenRightward.LowTable[0],
            "editable Golden Torizo turning-right component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xa4f0, out var unchangedRightwardCollision) &&
                   unchangedRightwardCollision[0].Offset.X == 0 &&
                   unchangedRightwardCollision[0].HitboxList == 0x87c7,
            "Golden Torizo rightward cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument goldenWalkingOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string goldenWalkingName = "golden_torizo_walk_left_A4FA";
        EnemyExtendedVisualComponent originalWalkingComponent =
            goldenWalkingOverride.Frames[goldenWalkingName][0];
        goldenWalkingOverride.Frames[goldenWalkingName][0] =
            originalWalkingComponent with
            {
                OffsetX = originalWalkingComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            goldenWalkingOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedGoldenWalking = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedGoldenWalking = DrawExtendedForBank(
            editedGoldenWalking, 0xaa, 0xa4fa, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockWalkingGoldenTorizo.LowTable[0] + 1)),
            movedGoldenWalking.LowTable[0],
            "editable Golden Torizo walking component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xa4fa, out var unchangedWalkingCollision) &&
                   unchangedWalkingCollision[0].Offset.X == -15 &&
                   unchangedWalkingCollision[0].HitboxList == 0x87c7,
            "Golden Torizo walking cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument goldenAwakeningOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string goldenAwakeningName = "golden_torizo_awake_AA5E";
        EnemyExtendedVisualComponent originalGoldenComponent =
            goldenAwakeningOverride.Frames[goldenAwakeningName][0];
        goldenAwakeningOverride.Frames[goldenAwakeningName][0] =
            originalGoldenComponent with
            {
                OffsetX = originalGoldenComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            goldenAwakeningOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedGoldenAwakening = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedGoldenAwakening = DrawExtendedForBank(
            editedGoldenAwakening, 0xaa, 0xaa5e, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockAwakeGoldenTorizo.LowTable[0] + 1)),
            movedGoldenAwakening.LowTable[0],
            "editable Golden Torizo awakening component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xaa5e, out var unchangedGoldenCollision) &&
                   unchangedGoldenCollision[0].Offset.X == -5 &&
                   unchangedGoldenCollision[0].HitboxList == 0x886a,
            "Golden Torizo cosmetic edit cannot move the engine-owned physical component");

        EnemyExtendedFrameDocument kraidArmOverride =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string kraidArmName = "kraid_arm_oam_90FD";
        EnemyExtendedVisualComponent originalArm = kraidArmOverride.Frames[kraidArmName][0];
        kraidArmOverride.Frames[kraidArmName][0] = originalArm with
        {
            OffsetX = originalArm.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            kraidArmOverride, CamelCaseJson));
        EnemyTileArtworkCatalog editedKraidArm = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer movedKraidArm = DrawExtendedForBank(editedKraidArm, 0xa7, 0x90fd, new(0x0040, 0x0080));
        AssertEqual(unchecked((byte)(stockKraidArm.LowTable[0] + 1)),
            movedKraidArm.LowTable[0],
            "editable Kraid arm component changes live OAM without ROM reads");

        EnemyExtendedFrameDocument sporeVisualRemap =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        OamBuffer closedSpore = DrawExtendedForBank(stock, 0xa5, 0xee65, new(0x0040, 0x0080));
        EnemyExtendedFrameDefinition[] visiblyDifferentSporeFrames =
            EnemyExtendedFrameDefinitions.Frames
                .Where(frame => frame.Name.StartsWith("spore_spawn_oam_",
                    StringComparison.Ordinal))
                .Where(frame =>
                {
                    OamBuffer candidate = DrawExtendedForBank(stock, frame.Bank, frame.Pointer, new(0x0040, 0x0080));
                    return !candidate.LowTable.SequenceEqual(closedSpore.LowTable) ||
                           !candidate.HighTable.SequenceEqual(closedSpore.HighTable);
                }).ToArray();
        AssertTrue(visiblyDifferentSporeFrames.Length > 0,
            "Spore Spawn visual remap fixture has a visibly different frame");
        EnemyExtendedFrameDefinition targetSporeFrame = visiblyDifferentSporeFrames[0];
        (sporeVisualRemap.DisplayFrames ?? throw new InvalidOperationException("Verification requires sporeVisualRemap.DisplayFrames."))["spore_spawn_oam_EE65"] =
            targetSporeFrame.Name;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            sporeVisualRemap, CamelCaseJson));
        EnemyTileArtworkCatalog swappedSpore = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer openSpore = DrawExtendedForBank(stock, targetSporeFrame.Bank, targetSporeFrame.Pointer, new(0x0040, 0x0080));
        OamBuffer swappedClosedSpore = DrawExtendedForBank(swappedSpore, 0xa5, 0xee65, new(0x0040, 0x0080));
        AssertTrue(swappedClosedSpore.ImageEquals(openSpore),
            "Spore Spawn display remap changes live OAM to the selected frame");
        EnemyTileArtworkCatalog reloadedSpore = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        OamBuffer reloadedClosedSpore = DrawExtendedForBank(reloadedSpore, 0xa5, 0xee65, new(0x0040, 0x0080));
        AssertTrue(reloadedClosedSpore.ImageEquals(openSpore),
            "Spore Spawn display remap survives asset reload");
        Suite(nameof(VerifySporeSpawnVisualRemapKeepsMechanics), () => VerifySporeSpawnVisualRemapKeepsMechanics(rom, stock, swappedSpore));

        EnemyExtendedFrameDocument remapped =
            RequiredJson.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string sourceName = "walking_pirate_walk_left_0";
        const string targetName = "walking_pirate_walk_left_1";
        (remapped.DisplayFrames ?? throw new InvalidOperationException("Verification requires remapped.DisplayFrames."))[sourceName] = targetName;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        EnemyTileArtworkCatalog swapped = StockWithExtendedFrames(stockDirectory, LoadExtendedFrameOverride());
        ushort sourcePointer = EnemyExtendedFrameDefinitions.Frames
            .Single(frame => frame.Name == sourceName).Pointer;
        ushort targetPointer = EnemyExtendedFrameDefinitions.Frames
            .Single(frame => frame.Name == targetName).Pointer;
        OamBuffer sourceOam = DrawExtended(stock, sourcePointer,
            new(0x0040, 0x0080));
        OamBuffer targetOam = DrawExtended(stock, targetPointer,
            new(0x0040, 0x0080));
        OamBuffer remappedOam = DrawExtended(swapped, sourcePointer,
            new(0x0040, 0x0080), slot =>
            {
                AssertEqual(sourcePointer, slot.SpritemapPointer,
                    "Pirate visual binding retains the native hitbox-frame pointer");
                AssertEqual((ushort)7, slot.InstructionTimer.RawValue,
                    "Pirate visual binding retains the instruction timer");
            });
        AssertTrue(!sourceOam.LowTable.SequenceEqual(targetOam.LowTable),
            "Pirate remap fixture uses visibly distinct native frames");
        AssertTrue(remappedOam.ImageEquals(targetOam),
            "Pirate display binding changes live OAM without reading the source frame");
        AssertEqual(GetTouchCallback(stock, sourcePointer),
            GetTouchCallback(swapped, sourcePointer),
            "Pirate display binding cannot move the compiled hitbox callback");
        AssertTrue(LoadExtendedFrameOverride().TryGetDisplay(EnemyExtendedFrameDefinitions.Bank,
                    sourcePointer, out _),
            "Pirate display binding survives reload");
        remapped.DisplayFrames[sourceName] = "wall_pirate_climb_left_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "cross-family Pirate display binding fails loudly");
        remapped.DisplayFrames[sourceName] = targetName;
        remapped.DisplayFrames["spore_spawn_oam_EE65"] = "draygon_oam_A2DF";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current Spore Spawn display binding cannot select Draygon art");
        remapped.DisplayFrames["spore_spawn_oam_EE65"] =
            "spore_spawn_oam_EE65";
        remapped.DisplayFrames[steamName] = "ridley_body_E983";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "Ceres steam display binding cannot select Ridley art");
        remapped.DisplayFrames[steamName] = steamName;
        remapped.DisplayFrames[oumName] = "ceres_steam_oam_F142";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "Oum display binding cannot select Ceres steam art");
        remapped.DisplayFrames[oumName] = oumName;
        remapped.DisplayFrames[crocomireName] = oumName;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "Crocomire tongue display binding cannot select Oum art");
        remapped.DisplayFrames[crocomireName] = crocomireName;
        remapped.DisplayFrames.Remove(sourceName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "missing Pirate display binding fails loudly");

        EnemyExtendedVisualComponent[] editedNinjaComponents = document.Frames[ninjaName];
        document.Frames.Remove(ninjaName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "v3 override missing a Ninja frame fails loudly");
        document.Frames.Add(ninjaName, editedNinjaComponents);

        EnemyExtendedVisualComponent[] editedWallComponents = document.Frames[wallName];
        document.Frames.Remove(wallName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "v3 override missing a wall-Pirate frame fails loudly");
        document.Frames.Add(wallName, editedWallComponents);

        EnemyExtendedVisualComponent[] steamComponents = document.Frames[steamName];
        document.Frames.Remove(steamName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current override missing a Ceres steam frame fails loudly");
        document.Frames.Add(steamName, steamComponents);

        EnemyExtendedVisualComponent[] oumComponents = document.Frames[oumName];
        document.Frames.Remove(oumName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current override missing an Oum frame fails loudly");
        document.Frames.Add(oumName, oumComponents);

        EnemyExtendedVisualComponent[] crocomireComponents =
            document.Frames[crocomireName];
        document.Frames.Remove(crocomireName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current override missing a Crocomire tongue frame fails loudly");
        document.Frames.Add(crocomireName, crocomireComponents);

        document.Frames.Remove(editedName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "missing extended frame fails loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "malformed extended composition override fails loudly");

        Console.WriteLine(
            "Extended enemy art: 131 Pirate, 11 Ridley, 48 Draygon, 12 Spore Spawn, 28 Ceres steam, 30 Oum, 9 Crocomire, 22 Kraid arm, 6 Golden Torizo awakening, 10 walking-left, 11 rightward, 3 shared right jump-back, 2 new left jump-back, 6 right-orb, 12 left-orb, 21 right-sonic, one falling-left and five left-foot orb OAM frames match native OAM " +
            "at three origins with visual ROM reads forbidden; Pirate and boss edits and " +
            "draw-only frame remaps preserve hitboxes/timers; v1-v23 override " +
            "migration, reload, stock hash and invalid-resource checks pass.");
    }

    /// <summary>Selects the historical schema's fixed frame-name prefix so later families cannot enter an old override.</summary>
    private static Dictionary<string, T> HistoricalExtendedEntries<T>(
        Dictionary<string, T> source, int count) =>
        EnemyExtendedFrameDefinitions.Frames.Take(count)
            .ToDictionary(frame => frame.Name, frame => source[frame.Name], StringComparer.Ordinal);
    /// <summary>Maps renamed Spore Spawn frames back to the identifiers used by older override schemas.</summary>
    private static string LegacyExtendedFrameName(string name) =>
        name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)
            ? "draygon_oam_" + name["spore_spawn_oam_".Length..]
            : name;

    /// <summary>Walks native extended spritemap components for an independent OAM reference, including clipping and packed sprite parts.</summary>
    /// <remarks>BG2 command streams are checked by their dedicated fixture.</remarks>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the reference extended-frame drawer")]
    internal static OamBuffer DrawReferenceExtendedFrame(
        ISnesAddressSpace bus, byte bank, ushort pointer, Position16<ScreenPixels> origin)
    {
        byte ReadByte(int address) => bus.ReadCartridgeByte(((int)new SnesAddress(bank, checked((ushort)(SnesAddressMath.OffsetInBank(address))))));
        ushort ReadWord(int address) => SnesLittleEndian.Word(ReadByte(address), ReadByte(address + 1));
        var oam = new OamBuffer();
        int count = ReadByte(pointer); // The high header byte is draw metadata.
        for (int index = 0; index < count; index++)
        {
            int component = pointer + 2 + index * 8;
            ushort sprite = ReadWord(component + 4);
            if (ReadWord(sprite) == 0xfffe)
                continue;
            ushort componentX = unchecked((ushort)(origin.X + ReadWord(component)));
            ushort componentY = unchecked((ushort)(origin.Y + ReadWord(component + 2)));
            if (((componentX + 128) & 0xfe00) != 0 ||
                ((componentY + 128) & 0xfe00) != 0)
                continue;
            DrawImportedEnemySpritemap(bus, oam, new EnemySpritemapDraw { Bank = bank, Pointer = sprite, Origin = new(componentX, componentY), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = (SnesLittleEndian.HighByte(componentY)) == 0 });
        }
        return oam;
    }

    /// <summary>Draws a bank-$B2 installed frame through the production enemy-layer path for OAM comparison.</summary>
    private static OamBuffer DrawExtended(EnemyTileArtworkCatalog art,
        ushort pointer, Position16<RoomPixels> position,
        Action<RoomEnemySlot>? inspect = null)
    {
        return DrawExtendedForBank(art, EnemyExtendedFrameDefinitions.Bank,
            pointer, position, inspect);
    }

    /// <summary>Installs a chosen extended frame on a fixture enemy and captures the production layer draw for its bank.</summary>
    internal static OamBuffer DrawExtendedForBank(EnemyTileArtworkCatalog? art,
        byte bank, ushort pointer, Position16<RoomPixels> position,
        Action<RoomEnemySlot>? inspect = null)
    {
        var enemies = new RoomEnemySystem { TileArtwork = art };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Drawing.Enqueue(0, 0);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        // A non-Ridley definition lets the focused OAM comparison isolate the
        // extended body from Ridley's separately drawn tail and wings.
        slot.Bind(bank == EnemyExtendedFrameDefinitions.Bank
            ? PirateDefinitionForFrame(pointer) : EnemyDefinitionId.Boyon, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
        {
            Bank = bank,
        });
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        slot.SpritemapPointer = pointer;
        slot.InstructionTimer = NativeCountdown.Start(7);
        slot.Position = slot.Position.WithWhole(position);
        var oam = new OamBuffer();
        enemies.Drawing.DrawLayers(oam, new(0, 0), 0, 0);
        inspect?.Invoke(slot);
        return oam;
    }

    /// <summary>Samples a stock Space Pirate hitbox through the production collision walker and returns its touch callback.</summary>
    private static ushort GetTouchCallback(EnemyTileArtworkCatalog art,
        ushort pointer)
    {
        var enemies = new RoomEnemySystem { TileArtwork = art };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(PirateDefinitionForFrame(pointer), slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
        {
            Bank = EnemyExtendedFrameDefinitions.Bank,
        });
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0040, 0x0080));
        SpacePirateCollisionComponent component =
            SpacePirateCollisionDefinitions.ComponentsAt(pointer)[0];
        SpacePirateCollisionHitbox hitbox =
            SpacePirateCollisionDefinitions.HitboxesAt(component.HitboxPointer)[0];
        ushort sampleX = unchecked((ushort)(slot.Position.X.Whole + component.Offset.X +
            (hitbox.Left + hitbox.Right) / 2));
        ushort sampleY = unchecked((ushort)(slot.Position.Y.Whole + component.Offset.Y +
            (hitbox.Top + hitbox.Bottom) / 2));
        object?[] arguments =
            [slot, new Position16<RoomPixels>(sampleX, sampleY), new Extent16<RoomPixels>((ushort)0, (ushort)0), false, (ushort)0];
        AssertTrue(ExtendedHitboxWalker.Find(enemies, arguments),
            "Space Pirate stock hitbox contains its sampled point");
        return (ushort)(arguments[4] ?? throw new InvalidOperationException("Verification requires arguments[4]."));
    }

    /// <summary>Selects walking, ninja, or wall Pirate ownership from a compiled extended-frame identity.</summary>
    private static EnemyDefinitionId PirateDefinitionForFrame(ushort pointer)
    {
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            if (frame.Pointer != pointer)
                continue;
            if (frame.Name.StartsWith("ninja_pirate_", StringComparison.Ordinal))
                return EnemyDefinitionId.PirateGreyNinja;
            if (frame.Name.StartsWith("wall_pirate_", StringComparison.Ordinal))
                return EnemyDefinitionId.PirateGreyWall;
            return EnemyDefinitionId.PirateGreyWalking;
        }
        if (pointer == EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap)
            return EnemyDefinitionId.PirateGreyWalking;
        throw new InvalidDataException(
            $"Extended Space Pirate frame $B2:{pointer:X4} is not installed.");
    }

    /// <summary>Creates a read guard whose returned callback adds native extended frames and their sprite parts to the forbidden set.</summary>
    internal static CartridgeReadGuard NewExtendedVisualReadGuard(
        ISnesAddressSpace source, out Action<byte, ushort> blockFrame)
    {
        var blocked = new HashSet<int>();
        blockFrame = (bank, pointer) => BlockExtendedVisualFrame(blocked, source, bank, pointer);
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed extended enemy draw read visual ROM byte ${address:X6}.");
    }

    /// <summary>Blocks a frame header, its OAM components, sprite records, and any bounded BG2 command stream from runtime ROM reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for extended visual frame layouts the guard blocks")]
    private static void BlockExtendedVisualFrame(
        HashSet<int> blocked, ISnesAddressSpace source, byte bank, ushort pointer)
    {
        int root = ((int)new SnesAddress(bank, pointer));
        int count = source.ReadCartridgeByte(root);
        Block(bank, pointer, 2 + count * 8);
        for (int index = 0; index < count; index++)
        {
            ushort component = unchecked((ushort)(pointer + 2 + index * 8));
            ushort sprite = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(component + 4)))));
            int spriteAddress = ((int)new SnesAddress(bank, sprite));
            int parts = SnesLittleEndian.Word(source.ReadCartridgeByte(spriteAddress), source.ReadCartridgeByte((int)new SnesAddress(bank, unchecked((ushort)(sprite + 1)))));
            if (parts == EnemyBg2FrameLayout.StreamMarker)
            {
                Block(bank, sprite, 2);
                ushort cursor = unchecked((ushort)(sprite + 2));
                bool terminated = false;
                for (int command = 0;
                     command < EnemyBg2FrameLayout.MaximumCommandsPerStream;
                     command++)
                {
                    ushort destination = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, cursor)));
                    if (destination == 0xffff)
                    {
                        Block(bank, cursor, 2);
                        terminated = true;
                        break;
                    }
                    ushort wordCount = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(cursor + 2)))));
                    int length = 4 + wordCount * 2;
                    Block(bank, cursor, length);
                    cursor = unchecked((ushort)(cursor + length));
                }
                if (!terminated)
                    throw new InvalidDataException(
                        $"Extended BG2 stream ${bank:X2}:{sprite:X4} has no terminator.");
                continue;
            }
            Block(bank, sprite, 2 + parts * 5);
        }

        void Block(byte blockBank, ushort blockPointer, int length)
        {
            for (int index = 0; index < length; index++)
                blocked.Add(((int)new SnesAddress(blockBank, unchecked((ushort)(blockPointer + index)))));
        }
    }
}
