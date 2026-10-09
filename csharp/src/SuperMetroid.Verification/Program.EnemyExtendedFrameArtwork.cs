using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks installed enemy extended-frame compositions against native drawing while blocking their visual ROM ranges.</summary>
    private static void VerifyInstalledEnemyExtendedFrames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.ExtendedFrames is not null,
            "installed enemy catalog contains extended visual compositions");
        var guard = new ExtendedVisualReadGuard(rom);
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            guard.BlockFrame(frame.Bank, frame.Pointer);
            AssertTrue(stock.ExtendedFrames!.TryGet(frame.Bank, frame.Pointer, out _),
                $"installed extended frame {frame.Name} exists");
        }
        ushort emptyPointer = EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap;
        guard.BlockFrame(EnemyExtendedFrameDefinitions.Bank, emptyPointer);
        AssertTrue(stock.ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                emptyPointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> empty) &&
                   empty.IsEmpty,
            "walking Pirate initial empty frame is a compiled draw identity");
        OamBuffer nativeEmpty = DrawReferenceExtendedFrame(rom, EnemyExtendedFrameDefinitions.Bank, emptyPointer,
            0x0040, 0x0080);
        OamBuffer installedEmpty = DrawExtended(stock, guard, emptyPointer,
            0x0040, 0x0080);
        AssertTrue(nativeEmpty.LowTable.SequenceEqual(installedEmpty.LowTable) &&
                   nativeEmpty.HighTable.SequenceEqual(installedEmpty.HighTable) &&
                   nativeEmpty.NextByteOffset == installedEmpty.NextByteOffset,
            "walking Pirate common empty frame draws without ROM reads");
        Suite(nameof(VerifySharedEmptyExtendedFrames), () => VerifySharedEmptyExtendedFrames(rom, stock));
        Suite(nameof(VerifyInstalledCeresSteamInstructionFrames), () => VerifyInstalledCeresSteamInstructionFrames(stock));
        Suite(nameof(VerifyInstalledOumVisualSelectors), () => VerifyInstalledOumVisualSelectors(rom, stock));
        Suite(nameof(VerifyInstalledCrocomireTongueVisualSelectors), () => VerifyInstalledCrocomireTongueVisualSelectors(rom, stock));
        AssertEqual(EnemyExtendedFrameDefinitions.ExpectedFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Length,
            "walking/wall Pirate distinct extended-frame count");
        AssertEqual(EnemyExtendedFrameDefinitions.WallFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("wall_pirate_", StringComparison.Ordinal)),
            "all wall-Pirate visual frame identities are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.NinjaFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
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
                    frame.Bank, frame.Pointer, x, y);
                OamBuffer installed = DrawExtendedForBank(stock, guard,
                    frame.Bank, frame.Pointer, x, y);
                AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) &&
                           native.HighTable.SequenceEqual(installed.HighTable) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed extended {frame.Name} matches native OAM at {x:X4},{y:X4}");
            }
        }
        Console.WriteLine("  Extended enemy OAM: every installed composition matches native component offsets, clipping, and packed sprite bytes at all three fixture origins.");
        Suite(nameof(VerifyInstalledSporeSpawnSelectorPrograms), () => VerifyInstalledSporeSpawnSelectorPrograms(rom, stock));
        AssertEqual(EnemyExtendedFrameDefinitions.RidleyFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("ridley_body_", StringComparison.Ordinal)),
            "all Ceres/Lower Norfair Ridley body frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CeresSteamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("ceres_steam_oam_", StringComparison.Ordinal)),
            "all selected Ceres steam visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.OumFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("oum_oam_", StringComparison.Ordinal)),
            "all selected Oum visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CrocomireOamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("crocomire_oam_", StringComparison.Ordinal)),
            "all selected Crocomire tongue visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CrocomireBodyFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("crocomire_body_oam_", StringComparison.Ordinal)),
            "all selected Crocomire fight-body OAM frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.KraidArmFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("kraid_arm_oam_", StringComparison.Ordinal)),
            "all Kraid arm visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoAwakeningFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("golden_torizo_awake_", StringComparison.Ordinal)),
            "all Golden Torizo awakening visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoWalkingFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("golden_torizo_walk_left_", StringComparison.Ordinal)),
            "all Golden Torizo walking-left visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightwardFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("golden_torizo_rightward_", StringComparison.Ordinal)),
            "all Golden Torizo turning/walking-right visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoJumpBackFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("torizo_jump_back_", StringComparison.Ordinal) &&
                    !frame.Name.StartsWith("torizo_jump_back_left_", StringComparison.Ordinal)),
            "all shared Torizo jump-back visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoJumpBackLeftNewFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("torizo_jump_back_left_", StringComparison.Ordinal)),
            "both new left-facing Torizo jump-back visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("golden_torizo_right_orb_", StringComparison.Ordinal)),
            "all Golden Torizo right-orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoLeftOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("golden_torizo_left_orb_", StringComparison.Ordinal)),
            "all Golden Torizo left-orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightSonicFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("golden_torizo_right_sonic_", StringComparison.Ordinal)),
            "all Golden Torizo right-sonic visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoFallingLeftFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("torizo_falling_left_", StringComparison.Ordinal)),
            "shared Torizo falling-left visual frame is installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoLeftFootOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("golden_torizo_left_foot_orb_", StringComparison.Ordinal)),
            "all new Golden Torizo left-foot orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.DraygonOamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal)),
            "all selected ordinary-OAM Draygon extended frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.SporeSpawnOamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.ToArray().Count(
                frame => frame.Name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)),
            "all selected Spore Spawn extended frames are installed");
        var draygonOamPointers = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Where(frame => frame.Name.StartsWith("draygon_oam_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        var draygonBg2Pointers = new HashSet<ushort>();
        for (int index = 0; index <
             DraygonInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort operand = DraygonInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            ushort pointer = (ushort)(rom.ReadByte(0xa50000 | operand) |
                rom.ReadByte(0xa50000 | unchecked((ushort)(operand + 1))) << 8);
            if (draygonOamPointers.Contains(pointer))
                continue;
            draygonBg2Pointers.Add(pointer);
            int count = rom.ReadByte(0xa50000 | pointer);
            AssertTrue(Enumerable.Range(0, count).Any(component =>
            {
                ushort record = unchecked((ushort)(pointer + 2 + component * 8 + 4));
                ushort sprite = (ushort)(rom.ReadByte(0xa50000 | record) |
                    rom.ReadByte(0xa50000 | unchecked((ushort)(record + 1))) << 8);
                return (rom.ReadByte(0xa50000 | sprite) |
                    rom.ReadByte(0xa50000 | unchecked((ushort)(sprite + 1))) << 8)
                    == 0xfffe;
            }), $"unextracted Draygon frame $A5:{pointer:X4} is a BG2 command");
        }
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
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
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
            JsonSerializer.SerializeToUtf8Bytes(document, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedRidley = EnemyTileArtworkFiles.Load(
            stockDirectory, ridleyOverrideDirectory);
        OamBuffer stockRidley = DrawExtendedForBank(stock, guard,
            0xa6, 0xe983, 0x0040, 0x0080);
        OamBuffer movedRidley = DrawExtendedForBank(editedRidley, guard,
            0xa6, 0xe983, 0x0040, 0x0080);
        OamBuffer stockDraygonOam = DrawExtendedForBank(stock, guard,
            0xa5, 0xa2df, 0x0040, 0x0080);
        OamBuffer movedDraygonOam = DrawExtendedForBank(editedRidley, guard,
            0xa5, 0xa2df, 0x0040, 0x0080);
        OamBuffer stockSteam = DrawExtendedForBank(stock, guard,
            0xa6, 0xf142, 0x0040, 0x0080);
        OamBuffer movedSteam = DrawExtendedForBank(editedRidley, guard,
            0xa6, 0xf142, 0x0040, 0x0080);
        OamBuffer stockOum = DrawExtendedForBank(stock, guard,
            0xa2, 0xcb87, 0x0040, 0x0080);
        OamBuffer movedOum = DrawExtendedForBank(editedRidley, guard,
            0xa2, 0xcb87, 0x0040, 0x0080);
        OamBuffer stockCrocomire = DrawExtendedForBank(stock, guard,
            0xa4, 0xc65e, 0x0040, 0x0080);
        OamBuffer movedCrocomire = DrawExtendedForBank(editedRidley, guard,
            0xa4, 0xc65e, 0x0040, 0x0080);
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
            return EnemyExtendedFrameCatalog.Load(json, stock.ExtendedFrames!);
        }
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort editedPointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == editedName).Pointer;
        OamBuffer stockOam = DrawExtended(stock, guard,
            editedPointer, 0x0040, 0x0080);
        OamBuffer editedOam = DrawExtended(edited, guard,
            editedPointer, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0],
            "editable walking Pirate component moves live OAM by one pixel");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "walking Pirate X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, guard, editedPointer),
            GetTouchCallback(edited, guard, editedPointer),
            "editable walking Pirate component does not move the compiled hitbox");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                    editedPointer, out _),
            "extended composition override survives catalog reload");

        // Existing v1 overrides were authored before wall-Pirate frames were
        // extracted. Preserve their validated walking edits and fill only the
        // new identities from the hash-checked current stock catalog.
        var legacyDocument = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.FirstVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.WalkingFrameCount),
        };
        byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(legacyDocument,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(legacyJson, writable: false)),
            "legacy extended override needs complete verified stock to merge");
        File.WriteAllBytes(overridePath, legacyJson);
        EnemyTileArtworkCatalog legacy = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer legacyWalking = DrawExtended(legacy, guard,
            editedPointer, 0x0040, 0x0080);
        AssertTrue(legacyWalking.LowTable.SequenceEqual(editedOam.LowTable) &&
                   legacyWalking.HighTable.SequenceEqual(editedOam.HighTable),
            "v1 walking-Pirate artwork edit survives the v2 catalog update");

        const string wallName = "wall_pirate_climb_left_0";
        ushort wallPointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == wallName).Pointer;
        OamBuffer stockWall = DrawExtended(stock, guard,
            wallPointer, 0x0040, 0x0080);
        OamBuffer legacyWall = DrawExtended(legacy, guard,
            wallPointer, 0x0040, 0x0080);
        AssertTrue(stockWall.LowTable.SequenceEqual(legacyWall.LowTable) &&
                   stockWall.HighTable.SequenceEqual(legacyWall.HighTable),
            "v1 override inherits stock wall-Pirate frames");

        EnemyExtendedVisualComponent wallFirst = document.Frames[wallName][0];
        document.Frames[wallName][0] = wallFirst with
        {
            OffsetX = wallFirst.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        EnemyTileArtworkCatalog editedWall = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedWall = DrawExtended(editedWall, guard,
            wallPointer, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockWall.LowTable[0] + 1)),
            movedWall.LowTable[0],
            "editable wall-Pirate component moves live OAM by one pixel");
        AssertEqual(stockWall.LowTable[1], movedWall.LowTable[1],
            "wall-Pirate X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, guard, wallPointer),
            GetTouchCallback(editedWall, guard, wallPointer),
            "editable wall-Pirate component does not move its compiled hitbox");
        AssertTrue(LoadExtendedFrameOverride().TryGet(EnemyExtendedFrameDefinitions.Bank,
                    wallPointer, out _),
            "edited wall-Pirate composition survives catalog reload");

        // The second published schema includes edited walking and wall frames,
        // but no Ninja identities. It must inherit only those new frames from
        // current stock without dropping either family's prior edit.
        var versionTwoDocument = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreviousVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.WalkingFrameCount + EnemyExtendedFrameDefinitions.WallFrameCount),
        };
        byte[] versionTwoJson = JsonSerializer.SerializeToUtf8Bytes(versionTwoDocument,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(versionTwoJson, writable: false)),
            "v2 extended override requires complete verified v3 stock");
        File.WriteAllBytes(overridePath, versionTwoJson);
        EnemyTileArtworkCatalog versionTwo = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer versionTwoWalking = DrawExtended(versionTwo, guard,
            editedPointer, 0x0040, 0x0080);
        OamBuffer versionTwoWall = DrawExtended(versionTwo, guard,
            wallPointer, 0x0040, 0x0080);
        AssertTrue(versionTwoWalking.LowTable.SequenceEqual(editedOam.LowTable) &&
                   versionTwoWall.LowTable.SequenceEqual(movedWall.LowTable),
            "v2 override preserves both walking and wall-Pirate art edits");

        EnemyExtendedFrameDefinition ninjaFrame = EnemyExtendedFrameDefinitions
            .Frames.ToArray().First(frame => frame.Name.StartsWith(
                "ninja_pirate_", StringComparison.Ordinal));
        string ninjaName = ninjaFrame.Name;
        ushort ninjaPointer = ninjaFrame.Pointer;
        OamBuffer stockNinja = DrawExtended(stock, guard,
            ninjaPointer, 0x0040, 0x0080);
        OamBuffer versionTwoNinja = DrawExtended(versionTwo, guard,
            ninjaPointer, 0x0040, 0x0080);
        OamBuffer versionOneNinja = DrawExtended(legacy, guard,
            ninjaPointer, 0x0040, 0x0080);
        AssertTrue(stockNinja.LowTable.SequenceEqual(versionTwoNinja.LowTable) &&
                   stockNinja.HighTable.SequenceEqual(versionTwoNinja.HighTable) &&
                   stockNinja.LowTable.SequenceEqual(versionOneNinja.LowTable) &&
                   stockNinja.HighTable.SequenceEqual(versionOneNinja.HighTable),
            "v1 and v2 overrides inherit verified stock Ninja frames");

        EnemyExtendedVisualComponent ninjaFirst = document.Frames[ninjaName][0];
        document.Frames[ninjaName][0] = ninjaFirst with
        {
            OffsetX = ninjaFirst.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        EnemyTileArtworkCatalog editedNinja = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedNinja = DrawExtended(editedNinja, guard,
            ninjaPointer, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockNinja.LowTable[0] + 1)),
            movedNinja.LowTable[0],
            "editable Ninja component moves live OAM by one pixel");
        AssertEqual(stockNinja.LowTable[1], movedNinja.LowTable[1],
            "Ninja X edit does not move visual Y");
        AssertEqual(GetTouchCallback(stock, guard, ninjaPointer),
            GetTouchCallback(editedNinja, guard, ninjaPointer),
            "editable Ninja component does not move its compiled hitbox");
        AssertTrue(LoadExtendedFrameOverride().TryGet(EnemyExtendedFrameDefinitions.Bank,
                    ninjaPointer, out _),
            "edited Ninja composition survives catalog reload");

        var preBindings = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreDisplayBindingsVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PirateFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            preBindings, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedBindings = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        AssertTrue(movedNinja.LowTable.SequenceEqual(DrawExtended(
                upgradedBindings, guard, ninjaPointer, 0x0040, 0x0080).LowTable),
            "version-three extended override retains edited art and stock display bindings");

        // The previous current schema had all Pirate art and editable display
        // bindings but no Ridley body. Preserve both types of player edit when
        // filling its new Ridley identities from verified stock.
        var versionFour = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PirateDisplayBindingsVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PirateFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PirateFrameCount),
        };
        const string sourceNameForLegacyBinding = "walking_pirate_walk_left_0";
        const string targetNameForLegacyBinding = "walking_pirate_walk_left_1";
        versionFour.DisplayFrames[sourceNameForLegacyBinding] = targetNameForLegacyBinding;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFour, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionFour = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        ushort legacySourcePointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == sourceNameForLegacyBinding).Pointer;
        ushort legacyTargetPointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == targetNameForLegacyBinding).Pointer;
        AssertTrue(DrawExtended(upgradedVersionFour, guard, legacySourcePointer,
                0x0040, 0x0080).LowTable.SequenceEqual(DrawExtended(stock, guard,
                legacyTargetPointer, 0x0040, 0x0080).LowTable),
            "version-four visual binding survives Ridley-frame migration");
        AssertTrue(upgradedVersionFour.ExtendedFrames!.TryGet(0xa6, 0xe983, out _),
            "version-four override inherits stock Ridley body art");

        // The immediately preceding schema includes Ridley but not Draygon.
        // Keep edited Pirate/Ridley compositions and display bindings while
        // filling the new boss's OAM-only frames from hash-checked stock.
        var versionFive = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreDraygonVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreDraygonFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreDraygonFrameCount),
        };
        versionFive.DisplayFrames[sourceNameForLegacyBinding] = targetNameForLegacyBinding;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFive, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionFive = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        AssertTrue(DrawExtended(upgradedVersionFive, guard, legacySourcePointer,
                0x0040, 0x0080).LowTable.SequenceEqual(DrawExtended(stock, guard,
                legacyTargetPointer, 0x0040, 0x0080).LowTable),
            "version-five override retains its authored Pirate display binding");
        AssertTrue(upgradedVersionFive.ExtendedFrames!.TryGet(0xa5, 0xa2df, out _),
            "version-five override inherits stock Draygon OAM art");

        // V6 published the Spore Spawn roots with Draygon-prefixed author keys.
        // Migrate the keys by unchanged bank/pointer identity so old artwork
        // and display bindings remain effective after the family correction.
        EnemyExtendedFrameDocument beforeSporeIdentity =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string currentSporeName = "spore_spawn_oam_EE6F";
        const string legacySporeName = "draygon_oam_EE6F";
        var versionSixFrames = HistoricalExtendedEntries(beforeSporeIdentity.Frames, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount).ToDictionary(entry => LegacyExtendedFrameName(entry.Key),
                entry => entry.Value, StringComparer.Ordinal);
        var versionSixBindings = HistoricalExtendedEntries(beforeSporeIdentity.DisplayFrames!, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount).ToDictionary(entry => LegacyExtendedFrameName(entry.Key),
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
            Version = EnemyExtendedFrameDefinitions.PreSporeIdentityVersion,
            Frames = versionSixFrames,
            DisplayFrames = versionSixBindings,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSix, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionSix = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockSpore = DrawExtendedForBank(stock, guard,
            0xa5, 0xee6f, 0x0040, 0x0080);
        OamBuffer editedSpore = DrawExtendedForBank(upgradedVersionSix, guard,
            0xa5, 0xee6f, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockSpore.LowTable[0] + 1)),
            editedSpore.LowTable[0],
            "version-six Draygon-named Spore Spawn art edit survives migration");
        OamBuffer reboundSpore = DrawExtendedForBank(upgradedVersionSix, guard,
            0xa5, 0xee65, 0x0040, 0x0080);
        AssertTrue(reboundSpore.LowTable.SequenceEqual(editedSpore.LowTable) &&
                   reboundSpore.HighTable.SequenceEqual(editedSpore.HighTable),
            "version-six Spore Spawn display binding survives key migration");
        AssertTrue(beforeSporeIdentity.Frames.ContainsKey(currentSporeName) &&
                   !beforeSporeIdentity.Frames.ContainsKey(legacySporeName),
            "current stock exposes the corrected Spore Spawn author key");

        var versionSeven = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreCeresSteamVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSeven, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionSeven = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer inheritedSteam = DrawExtendedForBank(upgradedVersionSeven, guard,
            0xa6, 0xf142, 0x0040, 0x0080);
        AssertTrue(inheritedSteam.LowTable.SequenceEqual(stockSteam.LowTable) &&
                   inheritedSteam.HighTable.SequenceEqual(stockSteam.HighTable),
            "version-seven override inherits verified stock Ceres steam art");
        AssertTrue(DrawExtended(upgradedVersionSeven, guard, editedPointer,
                0x0040, 0x0080).LowTable.SequenceEqual(editedOam.LowTable),
            "version-seven Pirate edit survives the Ceres steam schema update");

        var versionEight = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreOumVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreOumFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreOumFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionEight, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionEight = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer inheritedOum = DrawExtendedForBank(upgradedVersionEight, guard,
            0xa2, 0xcb87, 0x0040, 0x0080);
        AssertTrue(inheritedOum.LowTable.SequenceEqual(stockOum.LowTable) &&
                   inheritedOum.HighTable.SequenceEqual(stockOum.HighTable),
            "version-eight override inherits verified stock Oum art");
        AssertTrue(DrawExtendedForBank(upgradedVersionEight, guard, 0xa6, 0xf142,
                0x0040, 0x0080).LowTable.SequenceEqual(movedSteam.LowTable),
            "version-eight Ceres steam edit survives the Oum schema update");

        var versionNine = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreCrocomireVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreCrocomireFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreCrocomireFrameCount),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionNine, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionNine = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer inheritedCrocomire = DrawExtendedForBank(upgradedVersionNine, guard,
            0xa4, 0xc65e, 0x0040, 0x0080);
        AssertTrue(inheritedCrocomire.LowTable.SequenceEqual(stockCrocomire.LowTable) &&
                   inheritedCrocomire.HighTable.SequenceEqual(stockCrocomire.HighTable),
            "version-nine override inherits verified stock Crocomire tongue art");
        AssertTrue(DrawExtendedForBank(upgradedVersionNine, guard, 0xa2, 0xcb87,
                0x0040, 0x0080).LowTable.SequenceEqual(movedOum.LowTable),
            "version-nine Oum edit survives the Crocomire schema update");

        var versionTen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreCrocomireBodyVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreCrocomireBodyFrameCount,
            versionTen.Frames.Count, "version-ten extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyExtendedFrameCatalog upgradedVersionTen = LoadExtendedFrameOverride();
        AssertTrue(upgradedVersionTen.TryGetDisplay(0xa4, 0xc2ec, out _),
            "version-ten override inherits Crocomire's mixed fight-body OAM");

        var versionEleven = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreBombTorizoVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreBombTorizoFrameCount,
            versionEleven.Frames.Count, "version-eleven extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionEleven, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionEleven = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockDormantTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0x87d0, 0x0040, 0x0080);
        OamBuffer inheritedDormantTorizo = DrawExtendedForBank(
            upgradedVersionEleven, guard, 0xaa, 0x87d0, 0x0040, 0x0080);
        AssertTrue(stockDormantTorizo.LowTable.SequenceEqual(
                       inheritedDormantTorizo.LowTable) &&
                   stockDormantTorizo.HighTable.SequenceEqual(
                       inheritedDormantTorizo.HighTable),
            "version-eleven override inherits verified stock dormant Torizo art");
        AssertTrue(DrawExtended(upgradedVersionEleven, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-eleven migration retains an edited Pirate frame");

        var versionTwelve = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoFrameCount,
            versionTwelve.Frames.Count, "version-twelve extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwelve, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionTwelve = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockInitialGoldenTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0xaa30, 0x0040, 0x0080);
        OamBuffer inheritedInitialGoldenTorizo = DrawExtendedForBank(
            upgradedVersionTwelve, guard, 0xaa, 0xaa30, 0x0040, 0x0080);
        AssertTrue(stockInitialGoldenTorizo.LowTable.SequenceEqual(
                       inheritedInitialGoldenTorizo.LowTable) &&
                   stockInitialGoldenTorizo.HighTable.SequenceEqual(
                       inheritedInitialGoldenTorizo.HighTable),
            "version-twelve override inherits verified stock initial Golden Torizo art");
        AssertTrue(DrawExtended(upgradedVersionTwelve, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-twelve migration retains an edited Pirate frame");

        var versionThirteen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreKraidArmVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreKraidArmFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreKraidArmFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreKraidArmFrameCount,
            versionThirteen.Frames.Count, "version-thirteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionThirteen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionThirteen = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockKraidArm = DrawExtendedForBank(stock, guard,
            0xa7, 0x90fd, 0x0040, 0x0080);
        OamBuffer inheritedKraidArm = DrawExtendedForBank(
            upgradedVersionThirteen, guard, 0xa7, 0x90fd, 0x0040, 0x0080);
        AssertTrue(stockKraidArm.LowTable.SequenceEqual(inheritedKraidArm.LowTable) &&
                   stockKraidArm.HighTable.SequenceEqual(inheritedKraidArm.HighTable),
            "version-thirteen override inherits verified stock Kraid arm art");
        AssertTrue(DrawExtended(upgradedVersionThirteen, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-thirteen migration retains an edited Pirate frame");

        var versionFourteen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoAwakeningFrameCount,
            versionFourteen.Frames.Count,
            "version-fourteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFourteen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionFourteen = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockAwakeGoldenTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0xaa5e, 0x0040, 0x0080);
        OamBuffer inheritedAwakeGoldenTorizo = DrawExtendedForBank(
            upgradedVersionFourteen, guard, 0xaa, 0xaa5e, 0x0040, 0x0080);
        AssertTrue(stockAwakeGoldenTorizo.LowTable.SequenceEqual(
                       inheritedAwakeGoldenTorizo.LowTable) &&
                   stockAwakeGoldenTorizo.HighTable.SequenceEqual(
                       inheritedAwakeGoldenTorizo.HighTable),
            "version-fourteen override inherits verified Golden Torizo awakening art");
        AssertTrue(DrawExtended(upgradedVersionFourteen, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-fourteen migration retains an edited Pirate frame");

        var versionFifteen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoWalkingFrameCount,
            versionFifteen.Frames.Count,
            "version-fifteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFifteen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionFifteen = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockWalkingGoldenTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0xa4fa, 0x0040, 0x0080);
        OamBuffer inheritedWalkingGoldenTorizo = DrawExtendedForBank(
            upgradedVersionFifteen, guard, 0xaa, 0xa4fa, 0x0040, 0x0080);
        AssertTrue(stockWalkingGoldenTorizo.LowTable.SequenceEqual(
                       inheritedWalkingGoldenTorizo.LowTable) &&
                   stockWalkingGoldenTorizo.HighTable.SequenceEqual(
                       inheritedWalkingGoldenTorizo.HighTable),
            "version-fifteen override inherits Golden Torizo walking art");
        AssertTrue(DrawExtended(upgradedVersionFifteen, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-fifteen migration retains an edited Pirate frame");

        var versionSixteen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoRightwardFrameCount,
            versionSixteen.Frames.Count,
            "version-sixteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSixteen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionSixteen = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockRightwardGoldenTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0xa4f0, 0x0040, 0x0080);
        OamBuffer inheritedRightwardGoldenTorizo = DrawExtendedForBank(
            upgradedVersionSixteen, guard, 0xaa, 0xa4f0, 0x0040, 0x0080);
        AssertTrue(stockRightwardGoldenTorizo.LowTable.SequenceEqual(
                       inheritedRightwardGoldenTorizo.LowTable) &&
                   stockRightwardGoldenTorizo.HighTable.SequenceEqual(
                       inheritedRightwardGoldenTorizo.HighTable),
            "version-sixteen override inherits Golden Torizo turning-right art");
        AssertTrue(DrawExtended(upgradedVersionSixteen, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-sixteen migration retains an edited Pirate frame");

        var versionSeventeen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreTorizoJumpBackVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreTorizoJumpBackFrameCount,
            versionSeventeen.Frames.Count,
            "version-seventeen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSeventeen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionSeventeen = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockJumpBackTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0xb048, 0x0040, 0x0080);
        OamBuffer inheritedJumpBackTorizo = DrawExtendedForBank(
            upgradedVersionSeventeen, guard, 0xaa, 0xb048, 0x0040, 0x0080);
        AssertTrue(stockJumpBackTorizo.LowTable.SequenceEqual(
                       inheritedJumpBackTorizo.LowTable) &&
                   stockJumpBackTorizo.HighTable.SequenceEqual(
                       inheritedJumpBackTorizo.HighTable),
            "version-seventeen override inherits Torizo jump-back art");
        AssertTrue(DrawExtended(upgradedVersionSeventeen, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-seventeen migration retains an edited Pirate frame");

        var versionEighteen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoRightOrbFrameCount,
            versionEighteen.Frames.Count,
            "version-eighteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionEighteen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionEighteen = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockRightOrbTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0xac88, 0x0040, 0x0080);
        OamBuffer inheritedRightOrbTorizo = DrawExtendedForBank(
            upgradedVersionEighteen, guard, 0xaa, 0xac88, 0x0040, 0x0080);
        AssertTrue(stockRightOrbTorizo.LowTable.SequenceEqual(
                       inheritedRightOrbTorizo.LowTable) &&
                   stockRightOrbTorizo.HighTable.SequenceEqual(
                       inheritedRightOrbTorizo.HighTable),
            "version-eighteen override inherits Golden Torizo right-orb art");
        AssertTrue(DrawExtended(upgradedVersionEighteen, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-eighteen migration retains an edited Pirate frame");

        var versionNineteen = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoRightSonicFrameCount,
            versionNineteen.Frames.Count,
            "version-nineteen extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionNineteen, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionNineteen = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockRightSonicTorizo = DrawExtendedForBank(stock, guard,
            0xaa, 0xabec, 0x0040, 0x0080);
        OamBuffer inheritedRightSonicTorizo = DrawExtendedForBank(
            upgradedVersionNineteen, guard, 0xaa, 0xabec, 0x0040, 0x0080);
        AssertTrue(stockRightSonicTorizo.LowTable.SequenceEqual(
                       inheritedRightSonicTorizo.LowTable) &&
                   stockRightSonicTorizo.HighTable.SequenceEqual(
                       inheritedRightSonicTorizo.HighTable),
            "version-nineteen override inherits Golden Torizo right-sonic art");
        AssertTrue(DrawExtended(upgradedVersionNineteen, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(
                editedOam.LowTable),
            "version-nineteen migration retains an edited Pirate frame");

        var versionTwenty = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreTorizoFallingLeftVersion,
            Frames = HistoricalExtendedEntries(document.Frames, EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount),
            DisplayFrames = HistoricalExtendedEntries(document.DisplayFrames!, EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreTorizoFallingLeftFrameCount,
            versionTwenty.Frames.Count,
            "version-twenty extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwenty, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionTwenty = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockFallingLeft = DrawExtendedForBank(stock, guard,
            0xaa, TorizoFallingLeftInstructionProgramDefinitions.FallingFrame, 0x0040, 0x0080);
        OamBuffer inheritedFallingLeft = DrawExtendedForBank(upgradedVersionTwenty,
            guard, 0xaa, TorizoFallingLeftInstructionProgramDefinitions.FallingFrame, 0x0040, 0x0080);
        AssertTrue(stockFallingLeft.LowTable.SequenceEqual(inheritedFallingLeft.LowTable) &&
                   stockFallingLeft.HighTable.SequenceEqual(inheritedFallingLeft.HighTable),
            "version-twenty override inherits the Torizo falling-left frame");
        AssertTrue(DrawExtended(upgradedVersionTwenty, guard,
                editedPointer, 0x0040, 0x0080).LowTable.SequenceEqual(editedOam.LowTable),
            "version-twenty migration retains an edited Pirate frame");

        EnemyExtendedFrameDocument fallingLeftOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string fallingLeftName = "torizo_falling_left_B014";
        EnemyExtendedVisualComponent originalFallingLeftComponent =
            fallingLeftOverride.Frames[fallingLeftName][0];
        fallingLeftOverride.Frames[fallingLeftName][0] =
            originalFallingLeftComponent with
            {
                OffsetX = originalFallingLeftComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            fallingLeftOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedFallingLeft = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedFallingLeft = DrawExtendedForBank(editedFallingLeft,
            guard, 0xaa, TorizoFallingLeftInstructionProgramDefinitions.FallingFrame, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockFallingLeft.LowTable[0] + 1)),
            movedFallingLeft.LowTable[0],
            "editable Torizo falling-left component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                TorizoFallingLeftInstructionProgramDefinitions.FallingFrame,
                out var unchangedFallingLeftCollision) &&
                   unchangedFallingLeftCollision[0].X == -16 &&
                   unchangedFallingLeftCollision[0].HitboxList == 0x87c7,
            "Torizo falling-left cosmetic edit cannot move its physical component");

        var versionTwentyOne = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbVersion,
            Frames = HistoricalExtendedEntries(fallingLeftOverride.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount),
            DisplayFrames = HistoricalExtendedEntries(fallingLeftOverride.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftFootOrbFrameCount,
            versionTwentyOne.Frames.Count,
            "version-twenty-one extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwentyOne, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionTwentyOne = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockLeftFootOrb = DrawExtendedForBank(stock, guard,
            0xaa, 0xac06, 0x0040, 0x0080);
        OamBuffer inheritedLeftFootOrb = DrawExtendedForBank(
            upgradedVersionTwentyOne, guard, 0xaa, 0xac06, 0x0040, 0x0080);
        AssertTrue(stockLeftFootOrb.LowTable.SequenceEqual(inheritedLeftFootOrb.LowTable) &&
                   stockLeftFootOrb.HighTable.SequenceEqual(inheritedLeftFootOrb.HighTable),
            "version-twenty-one override inherits Golden Torizo left-foot orb art");
        AssertTrue(DrawExtendedForBank(upgradedVersionTwentyOne, guard,
                0xaa, TorizoFallingLeftInstructionProgramDefinitions.FallingFrame,
                0x0040, 0x0080).LowTable.SequenceEqual(movedFallingLeft.LowTable),
            "version-twenty-one migration retains the edited falling-left frame");

        var versionTwentyTwo = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftVersion,
            Frames = HistoricalExtendedEntries(fallingLeftOverride.Frames, EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount),
            DisplayFrames = HistoricalExtendedEntries(fallingLeftOverride.DisplayFrames!, EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreTorizoJumpBackLeftFrameCount,
            versionTwentyTwo.Frames.Count,
            "version-twenty-two extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwentyTwo, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionTwentyTwo = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockLeftJumpBack = DrawExtendedForBank(stock, guard,
            0xaa, 0xaffa, 0x0040, 0x0080);
        OamBuffer inheritedLeftJumpBack = DrawExtendedForBank(
            upgradedVersionTwentyTwo, guard, 0xaa, 0xaffa, 0x0040, 0x0080);
        AssertTrue(stockLeftJumpBack.LowTable.SequenceEqual(inheritedLeftJumpBack.LowTable) &&
                   stockLeftJumpBack.HighTable.SequenceEqual(inheritedLeftJumpBack.HighTable),
            "version-twenty-two override inherits left-facing jump-back art");
        AssertTrue(DrawExtendedForBank(upgradedVersionTwentyTwo, guard,
                0xaa, TorizoFallingLeftInstructionProgramDefinitions.FallingFrame,
                0x0040, 0x0080).LowTable.SequenceEqual(movedFallingLeft.LowTable),
            "version-twenty-two migration retains the edited falling-left frame");

        var versionTwentyThree = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbVersion,
            Frames = HistoricalExtendedEntries(fallingLeftOverride.Frames, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount),
            DisplayFrames = HistoricalExtendedEntries(fallingLeftOverride.DisplayFrames!, EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount),
        };
        AssertEqual(EnemyExtendedFrameDefinitions.PreGoldenTorizoLeftOrbFrameCount,
            versionTwentyThree.Frames.Count,
            "version-twenty-three extended-frame schema count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionTwentyThree, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionTwentyThree = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer stockLeftOrb = DrawExtendedForBank(stock, guard,
            0xaa, 0xa6ea, 0x0040, 0x0080);
        OamBuffer inheritedLeftOrb = DrawExtendedForBank(
            upgradedVersionTwentyThree, guard, 0xaa, 0xa6ea, 0x0040, 0x0080);
        AssertTrue(stockLeftOrb.LowTable.SequenceEqual(inheritedLeftOrb.LowTable) &&
                   stockLeftOrb.HighTable.SequenceEqual(inheritedLeftOrb.HighTable),
            "version-twenty-three override inherits left-orb art");
        AssertTrue(DrawExtendedForBank(upgradedVersionTwentyThree, guard,
                0xaa, TorizoFallingLeftInstructionProgramDefinitions.FallingFrame,
                0x0040, 0x0080).LowTable.SequenceEqual(movedFallingLeft.LowTable),
            "version-twenty-three migration retains the edited falling-left frame");

        EnemyExtendedFrameDocument leftFootOrbOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string leftFootOrbName = "golden_torizo_left_foot_orb_AC06";
        EnemyExtendedVisualComponent originalLeftFootOrbComponent =
            leftFootOrbOverride.Frames[leftFootOrbName][0];
        leftFootOrbOverride.Frames[leftFootOrbName][0] =
            originalLeftFootOrbComponent with
            {
                OffsetX = originalLeftFootOrbComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            leftFootOrbOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedLeftFootOrb = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedLeftFootOrb = DrawExtendedForBank(
            editedLeftFootOrb, guard, 0xaa, 0xac06, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockLeftFootOrb.LowTable[0] + 1)),
            movedLeftFootOrb.LowTable[0],
            "editable Golden Torizo left-foot orb component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xac06, out var unchangedLeftFootOrbCollision) &&
                   unchangedLeftFootOrbCollision[0].X == 9 &&
                   unchangedLeftFootOrbCollision[0].HitboxList == 0x87c7,
            "Golden Torizo left-foot orb cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument rightSonicOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string rightSonicName = "golden_torizo_right_sonic_ABEC";
        EnemyExtendedVisualComponent originalRightSonicComponent =
            rightSonicOverride.Frames[rightSonicName][0];
        rightSonicOverride.Frames[rightSonicName][0] =
            originalRightSonicComponent with
            {
                OffsetX = originalRightSonicComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            rightSonicOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedRightSonic = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedRightSonic = DrawExtendedForBank(
            editedRightSonic, guard, 0xaa, 0xabec, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockRightSonicTorizo.LowTable[0] + 1)),
            movedRightSonic.LowTable[0],
            "editable Golden Torizo right-sonic component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xabec, out var unchangedRightSonicCollision) &&
                   unchangedRightSonicCollision[0].X == 15 &&
                   unchangedRightSonicCollision[0].HitboxList == 0x87c7,
            "Golden Torizo right-sonic cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument rightOrbOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string rightOrbName = "golden_torizo_right_orb_AC88";
        EnemyExtendedVisualComponent originalRightOrbComponent =
            rightOrbOverride.Frames[rightOrbName][0];
        rightOrbOverride.Frames[rightOrbName][0] =
            originalRightOrbComponent with
            {
                OffsetX = originalRightOrbComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            rightOrbOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedRightOrb = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedRightOrb = DrawExtendedForBank(
            editedRightOrb, guard, 0xaa, 0xac88, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockRightOrbTorizo.LowTable[0] + 1)),
            movedRightOrb.LowTable[0],
            "editable Golden Torizo right-orb component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xac88, out var unchangedRightOrbCollision) &&
                   unchangedRightOrbCollision[0].X == 15 &&
                   unchangedRightOrbCollision[0].HitboxList == 0x87c7,
            "Golden Torizo right-orb cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument jumpBackOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string jumpBackName = "torizo_jump_back_B048";
        EnemyExtendedVisualComponent originalJumpBackComponent =
            jumpBackOverride.Frames[jumpBackName][0];
        jumpBackOverride.Frames[jumpBackName][0] =
            originalJumpBackComponent with
            {
                OffsetX = originalJumpBackComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            jumpBackOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedJumpBack = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedJumpBack = DrawExtendedForBank(
            editedJumpBack, guard, 0xaa, 0xb048, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockJumpBackTorizo.LowTable[0] + 1)),
            movedJumpBack.LowTable[0],
            "editable Torizo jump-back component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xb048, out var unchangedJumpBackCollision) &&
                   unchangedJumpBackCollision[0].X == 15 &&
                   unchangedJumpBackCollision[0].HitboxList == 0x87c7,
            "Torizo jump-back cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument goldenRightwardOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string goldenRightwardName = "golden_torizo_rightward_A4F0";
        EnemyExtendedVisualComponent originalRightwardComponent =
            goldenRightwardOverride.Frames[goldenRightwardName][0];
        goldenRightwardOverride.Frames[goldenRightwardName][0] =
            originalRightwardComponent with
            {
                OffsetX = originalRightwardComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            goldenRightwardOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedGoldenRightward = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedGoldenRightward = DrawExtendedForBank(
            editedGoldenRightward, guard, 0xaa, 0xa4f0, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockRightwardGoldenTorizo.LowTable[0] + 1)),
            movedGoldenRightward.LowTable[0],
            "editable Golden Torizo turning-right component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xa4f0, out var unchangedRightwardCollision) &&
                   unchangedRightwardCollision[0].X == 0 &&
                   unchangedRightwardCollision[0].HitboxList == 0x87c7,
            "Golden Torizo rightward cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument goldenWalkingOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string goldenWalkingName = "golden_torizo_walk_left_A4FA";
        EnemyExtendedVisualComponent originalWalkingComponent =
            goldenWalkingOverride.Frames[goldenWalkingName][0];
        goldenWalkingOverride.Frames[goldenWalkingName][0] =
            originalWalkingComponent with
            {
                OffsetX = originalWalkingComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            goldenWalkingOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedGoldenWalking = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedGoldenWalking = DrawExtendedForBank(
            editedGoldenWalking, guard, 0xaa, 0xa4fa, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockWalkingGoldenTorizo.LowTable[0] + 1)),
            movedGoldenWalking.LowTable[0],
            "editable Golden Torizo walking component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xa4fa, out var unchangedWalkingCollision) &&
                   unchangedWalkingCollision[0].X == -15 &&
                   unchangedWalkingCollision[0].HitboxList == 0x87c7,
            "Golden Torizo walking cosmetic edit cannot move its physical component");

        EnemyExtendedFrameDocument goldenAwakeningOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string goldenAwakeningName = "golden_torizo_awake_AA5E";
        EnemyExtendedVisualComponent originalGoldenComponent =
            goldenAwakeningOverride.Frames[goldenAwakeningName][0];
        goldenAwakeningOverride.Frames[goldenAwakeningName][0] =
            originalGoldenComponent with
            {
                OffsetX = originalGoldenComponent.OffsetX + 1,
            };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            goldenAwakeningOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedGoldenAwakening = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedGoldenAwakening = DrawExtendedForBank(
            editedGoldenAwakening, guard, 0xaa, 0xaa5e, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockAwakeGoldenTorizo.LowTable[0] + 1)),
            movedGoldenAwakening.LowTable[0],
            "editable Golden Torizo awakening component changes live OAM");
        AssertTrue(TorizoCollisionDefinitions.TryGetComponents(
                0xaa5e, out var unchangedGoldenCollision) &&
                   unchangedGoldenCollision[0].X == -5 &&
                   unchangedGoldenCollision[0].HitboxList == 0x886a,
            "Golden Torizo cosmetic edit cannot move the engine-owned physical component");

        EnemyExtendedFrameDocument kraidArmOverride =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string kraidArmName = "kraid_arm_oam_90FD";
        EnemyExtendedVisualComponent originalArm = kraidArmOverride.Frames[kraidArmName][0];
        kraidArmOverride.Frames[kraidArmName][0] = originalArm with
        {
            OffsetX = originalArm.OffsetX + 1,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            kraidArmOverride, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog editedKraidArm = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer movedKraidArm = DrawExtendedForBank(editedKraidArm, guard,
            0xa7, 0x90fd, 0x0040, 0x0080);
        AssertEqual(unchecked((byte)(stockKraidArm.LowTable[0] + 1)),
            movedKraidArm.LowTable[0],
            "editable Kraid arm component changes live OAM without ROM reads");

        EnemyExtendedFrameDocument sporeVisualRemap =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        OamBuffer closedSpore = DrawExtendedForBank(stock, guard,
            0xa5, 0xee65, 0x0040, 0x0080);
        EnemyExtendedFrameDefinition[] visiblyDifferentSporeFrames =
            EnemyExtendedFrameDefinitions.Frames.ToArray()
                .Where(frame => frame.Name.StartsWith("spore_spawn_oam_",
                    StringComparison.Ordinal))
                .Where(frame =>
                {
                    OamBuffer candidate = DrawExtendedForBank(stock, guard,
                        frame.Bank, frame.Pointer, 0x0040, 0x0080);
                    return !candidate.LowTable.SequenceEqual(closedSpore.LowTable) ||
                           !candidate.HighTable.SequenceEqual(closedSpore.HighTable);
                }).ToArray();
        AssertTrue(visiblyDifferentSporeFrames.Length > 0,
            "Spore Spawn visual remap fixture has a visibly different frame");
        EnemyExtendedFrameDefinition targetSporeFrame = visiblyDifferentSporeFrames[0];
        sporeVisualRemap.DisplayFrames!["spore_spawn_oam_EE65"] =
            targetSporeFrame.Name;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            sporeVisualRemap, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog swappedSpore = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer openSpore = DrawExtendedForBank(stock, guard,
            targetSporeFrame.Bank, targetSporeFrame.Pointer, 0x0040, 0x0080);
        OamBuffer swappedClosedSpore = DrawExtendedForBank(swappedSpore, guard,
            0xa5, 0xee65, 0x0040, 0x0080);
        AssertTrue(swappedClosedSpore.LowTable.SequenceEqual(openSpore.LowTable) &&
                   swappedClosedSpore.HighTable.SequenceEqual(openSpore.HighTable),
            "Spore Spawn display remap changes live OAM to the selected frame");
        EnemyTileArtworkCatalog reloadedSpore = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        OamBuffer reloadedClosedSpore = DrawExtendedForBank(reloadedSpore, guard,
            0xa5, 0xee65, 0x0040, 0x0080);
        AssertTrue(reloadedClosedSpore.LowTable.SequenceEqual(openSpore.LowTable) &&
                   reloadedClosedSpore.HighTable.SequenceEqual(openSpore.HighTable),
            "Spore Spawn display remap survives asset reload");
        Suite(nameof(VerifySporeSpawnVisualRemapKeepsMechanics), () => VerifySporeSpawnVisualRemapKeepsMechanics(rom, stock, swappedSpore));

        EnemyExtendedFrameDocument remapped =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string sourceName = "walking_pirate_walk_left_0";
        const string targetName = "walking_pirate_walk_left_1";
        remapped.DisplayFrames![sourceName] = targetName;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog swapped = stock.WithExtendedFrames(LoadExtendedFrameOverride());
        ushort sourcePointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == sourceName).Pointer;
        ushort targetPointer = EnemyExtendedFrameDefinitions.Frames.ToArray()
            .Single(frame => frame.Name == targetName).Pointer;
        OamBuffer sourceOam = DrawExtended(stock, guard, sourcePointer,
            0x0040, 0x0080);
        OamBuffer targetOam = DrawExtended(stock, guard, targetPointer,
            0x0040, 0x0080);
        OamBuffer remappedOam = DrawExtended(swapped, guard, sourcePointer,
            0x0040, 0x0080, slot =>
            {
                AssertEqual(sourcePointer, slot.SpritemapPointer,
                    "Pirate visual binding retains the native hitbox-frame pointer");
                AssertEqual((ushort)7, slot.InstructionTimer,
                    "Pirate visual binding retains the instruction timer");
            });
        AssertTrue(!sourceOam.LowTable.SequenceEqual(targetOam.LowTable),
            "Pirate remap fixture uses visibly distinct native frames");
        AssertTrue(remappedOam.LowTable.SequenceEqual(targetOam.LowTable) &&
                   remappedOam.HighTable.SequenceEqual(targetOam.HighTable),
            "Pirate display binding changes live OAM without reading the source frame");
        AssertEqual(GetTouchCallback(stock, guard, sourcePointer),
            GetTouchCallback(swapped, guard, sourcePointer),
            "Pirate display binding cannot move the compiled hitbox callback");
        AssertTrue(LoadExtendedFrameOverride().TryGetDisplay(EnemyExtendedFrameDefinitions.Bank,
                    sourcePointer, out _),
            "Pirate display binding survives reload");
        remapped.DisplayFrames[sourceName] = "wall_pirate_climb_left_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "cross-family Pirate display binding fails loudly");
        remapped.DisplayFrames[sourceName] = targetName;
        remapped.DisplayFrames["spore_spawn_oam_EE65"] = "draygon_oam_A2DF";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current Spore Spawn display binding cannot select Draygon art");
        remapped.DisplayFrames["spore_spawn_oam_EE65"] =
            "spore_spawn_oam_EE65";
        remapped.DisplayFrames[steamName] = "ridley_body_E983";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "Ceres steam display binding cannot select Ridley art");
        remapped.DisplayFrames[steamName] = steamName;
        remapped.DisplayFrames[oumName] = "ceres_steam_oam_F142";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "Oum display binding cannot select Ceres steam art");
        remapped.DisplayFrames[oumName] = oumName;
        remapped.DisplayFrames[crocomireName] = oumName;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "Crocomire tongue display binding cannot select Oum art");
        remapped.DisplayFrames[crocomireName] = crocomireName;
        remapped.DisplayFrames.Remove(sourceName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "missing Pirate display binding fails loudly");

        EnemyExtendedVisualComponent[] editedNinjaComponents = document.Frames[ninjaName];
        document.Frames.Remove(ninjaName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "v3 override missing a Ninja frame fails loudly");
        document.Frames.Add(ninjaName, editedNinjaComponents);

        EnemyExtendedVisualComponent[] editedWallComponents = document.Frames[wallName];
        document.Frames.Remove(wallName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "v3 override missing a wall-Pirate frame fails loudly");
        document.Frames.Add(wallName, editedWallComponents);

        EnemyExtendedVisualComponent[] steamComponents = document.Frames[steamName];
        document.Frames.Remove(steamName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current override missing a Ceres steam frame fails loudly");
        document.Frames.Add(steamName, steamComponents);

        EnemyExtendedVisualComponent[] oumComponents = document.Frames[oumName];
        document.Frames.Remove(oumName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current override missing an Oum frame fails loudly");
        document.Frames.Add(oumName, oumComponents);

        EnemyExtendedVisualComponent[] crocomireComponents =
            document.Frames[crocomireName];
        document.Frames.Remove(crocomireName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => LoadExtendedFrameOverride(),
            "current override missing a Crocomire tongue frame fails loudly");
        document.Frames.Add(crocomireName, crocomireComponents);

        document.Frames.Remove(editedName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
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

    // Historical schemas own a fixed prefix of the append-only identity catalog.
    // Select authored values by those names so later families cannot leak into
    // old overrides; retain their edited compositions and display bindings.
    /// <summary>Selects the named prefix owned by an older schema from the append-only frame identity catalog.</summary>
    private static Dictionary<string, T> HistoricalExtendedEntries<T>(
        Dictionary<string, T> source, int count) =>
        EnemyExtendedFrameDefinitions.Frames.ToArray().Take(count)
            .ToDictionary(frame => frame.Name, frame => source[frame.Name], StringComparer.Ordinal);
    /// <summary>Maps the former Spore Spawn frame prefix to its legacy Draygon identity during override migration.</summary>
    private static string LegacyExtendedFrameName(string name) =>
        name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)
            ? "draygon_oam_" + name["spore_spawn_oam_".Length..]
            : name;

    // Independent native extended-spritemap OAM walker ($A0: drawing path).
    // BG2 streams are covered by their dedicated fixture; this comparison owns
    // only component offsets, clipping, and the packed sprite compositions.
    /// <summary>Walks a native extended spritemap and produces its clipped OAM composition for comparison.</summary>
    private static OamBuffer DrawReferenceExtendedFrame(
        ISnesAddressSpace bus, byte bank, ushort pointer, ushort x, ushort y)
    {
        byte ReadByte(int address) => bus.ReadByte((bank << 16) | (address & 0xffff));
        ushort ReadWord(int address) => (ushort)(ReadByte(address) | ReadByte(address + 1) << 8);
        var oam = new OamBuffer();
        int count = ReadByte(pointer); // The high header byte is draw metadata.
        for (int index = 0; index < count; index++)
        {
            int component = pointer + 2 + index * 8;
            ushort sprite = ReadWord(component + 4);
            if (ReadWord(sprite) == 0xfffe)
                continue;
            ushort componentX = unchecked((ushort)(x + ReadWord(component)));
            ushort componentY = unchecked((ushort)(y + ReadWord(component + 2)));
            if (((componentX + 128) & 0xfe00) != 0 ||
                ((componentY + 128) & 0xfe00) != 0)
                continue;
            DrawImportedEnemySpritemap(bus, oam, bank, sprite, componentX,
                componentY, 0, 0, clipVerticalWrap: true,
                originYIsOnScreen: (componentY >> 8) == 0);
        }
        return oam;
    }

    /// <summary>Draws one installed extended frame through the game renderer using the catalog's configured bank.</summary>
    private static OamBuffer DrawExtended(EnemyTileArtworkCatalog art,
        ISnesAddressSpace bus, ushort pointer, ushort x, ushort y,
        Action<RoomEnemySlot>? inspect = null)
    {
        return DrawExtendedForBank(art, bus, EnemyExtendedFrameDefinitions.Bank,
            pointer, x, y, inspect);
    }

    /// <summary>Draws a selected bank and frame through the runtime renderer with a synthetic enemy slot.</summary>
    private static OamBuffer DrawExtendedForBank(EnemyTileArtworkCatalog? art,
        ISnesAddressSpace bus, byte bank, ushort pointer, ushort x, ushort y,
        Action<RoomEnemySlot>? inspect = null)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        var queues = (List<ushort>[])typeof(RoomEnemySystem)
            .GetField("_drawQueues", flags)!.GetValue(enemies)!;
        queues[0].Add(0);
        RoomEnemySlot slot = enemies.Slots[0];
        // A non-Ridley definition lets the focused OAM comparison isolate the
        // extended body from Ridley's separately drawn tail and wings.
        slot.EnemyDefinitionPointer = bank == EnemyExtendedFrameDefinitions.Bank
            ? PirateDefinitionForFrame(pointer) : RoomEnemySystem.BoyonDefinition;
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = bank,
        };
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        slot.SpritemapPointer = pointer;
        slot.InstructionTimer = 7;
        slot.XPosition = x;
        slot.YPosition = y;
        var oam = new OamBuffer();
        enemies.DrawLayers(oam, 0, 0, 0, 0);
        inspect?.Invoke(slot);
        return oam;
    }

    /// <summary>Queries the runtime's extended-hitbox path and returns the callback selected at a known interior point.</summary>
    private static ushort GetTouchCallback(EnemyTileArtworkCatalog art,
        ISnesAddressSpace bus, ushort pointer)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = PirateDefinitionForFrame(pointer);
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = EnemyExtendedFrameDefinitions.Bank,
        };
        slot.SpritemapPointer = pointer;
        slot.XPosition = 0x0040;
        slot.YPosition = 0x0080;
        SpacePirateCollisionComponent component =
            SpacePirateCollisionDefinitions.ComponentsAt(pointer)[0];
        SpacePirateCollisionHitbox hitbox =
            SpacePirateCollisionDefinitions.HitboxesAt(component.HitboxPointer)[0];
        ushort sampleX = unchecked((ushort)(slot.XPosition + component.X +
            (hitbox.Left + hitbox.Right) / 2));
        ushort sampleY = unchecked((ushort)(slot.YPosition + component.Y +
            (hitbox.Top + hitbox.Bottom) / 2));
        MethodInfo collision = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        object?[] arguments =
            [slot, sampleX, sampleY, (ushort)0, (ushort)0,
                false, (ushort)0];
        AssertTrue((bool)collision.Invoke(enemies, arguments)!,
            "Space Pirate stock hitbox contains its sampled point");
        return (ushort)arguments[6]!;
    }

    /// <summary>Chooses the matching walking, wall, or ninja Pirate definition for an installed frame pointer.</summary>
    private static ushort PirateDefinitionForFrame(ushort pointer)
    {
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            if (frame.Pointer != pointer)
                continue;
            if (frame.Name.StartsWith("ninja_pirate_", StringComparison.Ordinal))
                return RoomEnemySystem.GreyNinjaSpacePirateDefinition;
            if (frame.Name.StartsWith("wall_pirate_", StringComparison.Ordinal))
                return RoomEnemySystem.GreyWallSpacePirateDefinition;
            return RoomEnemySystem.GreyWalkingSpacePirateDefinition;
        }
        if (pointer == EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap)
            return RoomEnemySystem.GreyWalkingSpacePirateDefinition;
        throw new InvalidDataException(
            $"Extended Space Pirate frame $B2:{pointer:X4} is not installed.");
    }

    /// <summary>Forwards cartridge access while throwing if drawing reads any visual bytes enumerated for a frame.</summary>
    /// <param name="source">Underlying address space used to enumerate frame data and service accesses outside blocked ranges.</param>
    private sealed class ExtendedVisualReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Cartridge byte addresses belonging to visual frame data that must already be installed.</summary>
        private readonly HashSet<int> blocked = [];

        /// <summary>Blocks the frame header, component list, and referenced sprite or BG2 stream bytes for a banked frame.</summary>
        internal void BlockFrame(byte bank, ushort pointer)
        {
            int root = (bank << 16) | pointer;
            int count = source.ReadByte(root);
            Block(bank, pointer, 2 + count * 8);
            for (int index = 0; index < count; index++)
            {
                ushort component = unchecked((ushort)(pointer + 2 + index * 8));
                ushort sprite = (ushort)(source.ReadByte((bank << 16) |
                        unchecked((ushort)(component + 4))) |
                    source.ReadByte((bank << 16) |
                        unchecked((ushort)(component + 5))) << 8);
                int spriteAddress = (bank << 16) | sprite;
                int parts = source.ReadByte(spriteAddress) |
                    source.ReadByte((bank << 16) |
                        unchecked((ushort)(sprite + 1))) << 8;
                if (parts == EnemyBg2FrameLayout.StreamMarker)
                {
                    Block(bank, sprite, 2);
                    ushort cursor = unchecked((ushort)(sprite + 2));
                    bool terminated = false;
                    for (int command = 0;
                         command < EnemyBg2FrameLayout.MaximumCommandsPerStream;
                         command++)
                    {
                        ushort destination = ReadSourceWord(cursor);
                        if (destination == 0xffff)
                        {
                            Block(bank, cursor, 2);
                            terminated = true;
                            break;
                        }
                        ushort wordCount = ReadSourceWord(unchecked((ushort)(cursor + 2)));
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

            ushort ReadSourceWord(ushort address) => unchecked((ushort)(
                source.ReadByte((bank << 16) | address) |
                source.ReadByte((bank << 16) | unchecked((ushort)(address + 1))) << 8));
        }

        /// <summary>Adds a contiguous bank-local byte range to the set rejected by subsequent reads.</summary>
        private void Block(byte bank, ushort pointer, int length)
        {
            for (int index = 0; index < length; index++)
                blocked.Add((bank << 16) | unchecked((ushort)(pointer + index)));
        }

        /// <summary>Routes the importer-facing read through the same visual-range guard as runtime reads.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from blocked frame bytes and forwards all other reads to the underlying address space.</summary>
        public byte ReadByte(int address)
        {
            if (blocked.Contains(address))
                throw new InvalidOperationException(
                    $"Installed extended enemy draw read visual ROM byte ${address:X6}.");
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged because this guard restricts reads of installed visual data only.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
