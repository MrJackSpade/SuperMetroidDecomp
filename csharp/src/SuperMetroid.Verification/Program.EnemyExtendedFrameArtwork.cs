using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
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
        OamBuffer nativeEmpty = DrawExtended(null, rom, emptyPointer,
            0x0040, 0x0080);
        OamBuffer installedEmpty = DrawExtended(stock, guard, emptyPointer,
            0x0040, 0x0080);
        AssertTrue(nativeEmpty.LowTable.SequenceEqual(installedEmpty.LowTable) &&
                   nativeEmpty.HighTable.SequenceEqual(installedEmpty.HighTable) &&
                   nativeEmpty.NextByteOffset == installedEmpty.NextByteOffset,
            "walking Pirate common empty frame draws without ROM reads");
        VerifySharedEmptyExtendedFrames(rom, stock);
        VerifyInstalledSporeSpawnSelectorPrograms(rom, stock);
        VerifyInstalledCeresSteamInstructionFrames(stock);
        VerifyInstalledOumVisualSelectors(rom, stock);
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
                OamBuffer native = DrawExtendedForBank(null, rom,
                    frame.Bank, frame.Pointer, x, y);
                OamBuffer installed = DrawExtendedForBank(stock, guard,
                    frame.Bank, frame.Pointer, x, y);
                AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) &&
                           native.HighTable.SequenceEqual(installed.HighTable) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed extended {frame.Name} matches native OAM at {x:X4},{y:X4}");
            }
        }
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
             DraygonInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = DraygonInstructionProgramDefinitions
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
            Frames = document.Frames.Where(entry => entry.Key.StartsWith(
                "walking_pirate_", StringComparison.Ordinal)).ToDictionary(
                    entry => entry.Key, entry => entry.Value,
                    StringComparer.Ordinal),
        };
        byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(legacyDocument,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(legacyJson, writable: false)),
            "legacy extended override needs complete verified stock to merge");
        File.WriteAllBytes(overridePath, legacyJson);
        EnemyTileArtworkCatalog legacy = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
        EnemyTileArtworkCatalog editedWall = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                    wallPointer, out _),
            "edited wall-Pirate composition survives catalog reload");

        // The second published schema includes edited walking and wall frames,
        // but no Ninja identities. It must inherit only those new frames from
        // current stock without dropping either family's prior edit.
        var versionTwoDocument = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreviousVersion,
            Frames = document.Frames.Where(entry =>
                !entry.Key.StartsWith("ninja_pirate_", StringComparison.Ordinal) &&
                !entry.Key.StartsWith("ridley_body_", StringComparison.Ordinal) &&
                !IsBankA5ExtendedFrameName(entry.Key) &&
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value,
                    StringComparer.Ordinal),
        };
        byte[] versionTwoJson = JsonSerializer.SerializeToUtf8Bytes(versionTwoDocument,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyExtendedFrameCatalog.Load(
                new MemoryStream(versionTwoJson, writable: false)),
            "v2 extended override requires complete verified v3 stock");
        File.WriteAllBytes(overridePath, versionTwoJson);
        EnemyTileArtworkCatalog versionTwo = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
        EnemyTileArtworkCatalog editedNinja = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGet(EnemyExtendedFrameDefinitions.Bank,
                    ninjaPointer, out _),
            "edited Ninja composition survives catalog reload");

        var preBindings = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PreDisplayBindingsVersion,
            Frames = document.Frames.Where(entry =>
                !entry.Key.StartsWith("ridley_body_", StringComparison.Ordinal) &&
                !IsBankA5ExtendedFrameName(entry.Key) &&
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            preBindings, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedBindings = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(movedNinja.LowTable.SequenceEqual(DrawExtended(
                upgradedBindings, guard, ninjaPointer, 0x0040, 0x0080).LowTable),
            "version-three extended override retains edited art and stock display bindings");

        // The previous current schema had all Pirate art and editable display
        // bindings but no Ridley body. Preserve both types of player edit when
        // filling its new Ridley identities from verified stock.
        var versionFour = new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.PirateDisplayBindingsVersion,
            Frames = document.Frames.Where(entry =>
                !entry.Key.StartsWith("ridley_body_", StringComparison.Ordinal) &&
                !IsBankA5ExtendedFrameName(entry.Key) &&
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
            DisplayFrames = document.DisplayFrames!.Where(entry =>
                !entry.Key.StartsWith("ridley_body_", StringComparison.Ordinal) &&
                !IsBankA5ExtendedFrameName(entry.Key) &&
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key))
                .ToDictionary(entry => entry.Key, entry => entry.Value,
                    StringComparer.Ordinal),
        };
        const string sourceNameForLegacyBinding = "walking_pirate_walk_left_0";
        const string targetNameForLegacyBinding = "walking_pirate_walk_left_1";
        versionFour.DisplayFrames[sourceNameForLegacyBinding] = targetNameForLegacyBinding;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFour, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionFour = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
            Frames = document.Frames.Where(entry =>
                !IsBankA5ExtendedFrameName(entry.Key) &&
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
            DisplayFrames = document.DisplayFrames!.Where(entry =>
                !IsBankA5ExtendedFrameName(entry.Key) &&
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key))
                .ToDictionary(entry => entry.Key, entry => entry.Value,
                    StringComparer.Ordinal),
        };
        versionFive.DisplayFrames[sourceNameForLegacyBinding] = targetNameForLegacyBinding;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionFive, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionFive = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
        var versionSixFrames = beforeSporeIdentity.Frames
            .Where(entry => !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key))
            .ToDictionary(entry => LegacyExtendedFrameName(entry.Key),
                entry => entry.Value, StringComparer.Ordinal);
        var versionSixBindings = beforeSporeIdentity.DisplayFrames!
            .Where(entry => !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key))
            .ToDictionary(entry => LegacyExtendedFrameName(entry.Key),
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
        EnemyTileArtworkCatalog upgradedVersionSix = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
            Frames = document.Frames.Where(entry =>
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
            DisplayFrames = document.DisplayFrames!.Where(entry =>
                !IsCeresSteamExtendedFrameName(entry.Key) &&
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionSeven, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionSeven = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
            Frames = document.Frames.Where(entry =>
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
            DisplayFrames = document.DisplayFrames!.Where(entry =>
                !IsOumExtendedFrameName(entry.Key)).ToDictionary(
                    entry => entry.Key, entry => entry.Value, StringComparer.Ordinal),
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            versionEight, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgradedVersionEight = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer inheritedOum = DrawExtendedForBank(upgradedVersionEight, guard,
            0xa2, 0xcb87, 0x0040, 0x0080);
        AssertTrue(inheritedOum.LowTable.SequenceEqual(stockOum.LowTable) &&
                   inheritedOum.HighTable.SequenceEqual(stockOum.HighTable),
            "version-eight override inherits verified stock Oum art");
        AssertTrue(DrawExtendedForBank(upgradedVersionEight, guard, 0xa6, 0xf142,
                0x0040, 0x0080).LowTable.SequenceEqual(movedSteam.LowTable),
            "version-eight Ceres steam edit survives the Oum schema update");

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
        EnemyTileArtworkCatalog swappedSpore = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer openSpore = DrawExtendedForBank(stock, guard,
            targetSporeFrame.Bank, targetSporeFrame.Pointer, 0x0040, 0x0080);
        OamBuffer swappedClosedSpore = DrawExtendedForBank(swappedSpore, guard,
            0xa5, 0xee65, 0x0040, 0x0080);
        AssertTrue(swappedClosedSpore.LowTable.SequenceEqual(openSpore.LowTable) &&
                   swappedClosedSpore.HighTable.SequenceEqual(openSpore.HighTable),
            "Spore Spawn display remap changes live OAM to the selected frame");
        EnemyTileArtworkCatalog reloadedSpore = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer reloadedClosedSpore = DrawExtendedForBank(reloadedSpore, guard,
            0xa5, 0xee65, 0x0040, 0x0080);
        AssertTrue(reloadedClosedSpore.LowTable.SequenceEqual(openSpore.LowTable) &&
                   reloadedClosedSpore.HighTable.SequenceEqual(openSpore.HighTable),
            "Spore Spawn display remap survives asset reload");
        VerifySporeSpawnVisualRemapKeepsMechanics(rom, stock, swappedSpore);

        EnemyExtendedFrameDocument remapped =
            JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string sourceName = "walking_pirate_walk_left_0";
        const string targetName = "walking_pirate_walk_left_1";
        remapped.DisplayFrames![sourceName] = targetName;
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog swapped = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
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
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .ExtendedFrames!.TryGetDisplay(EnemyExtendedFrameDefinitions.Bank,
                    sourcePointer, out _),
            "Pirate display binding survives reload");
        remapped.DisplayFrames[sourceName] = "wall_pirate_climb_left_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "cross-family Pirate display binding fails loudly");
        remapped.DisplayFrames[sourceName] = targetName;
        remapped.DisplayFrames["spore_spawn_oam_EE65"] = "draygon_oam_A2DF";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "current Spore Spawn display binding cannot select Draygon art");
        remapped.DisplayFrames["spore_spawn_oam_EE65"] =
            "spore_spawn_oam_EE65";
        remapped.DisplayFrames[steamName] = "ridley_body_E983";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "Ceres steam display binding cannot select Ridley art");
        remapped.DisplayFrames[steamName] = steamName;
        remapped.DisplayFrames[oumName] = "ceres_steam_oam_F142";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "Oum display binding cannot select Ceres steam art");
        remapped.DisplayFrames[oumName] = oumName;
        remapped.DisplayFrames.Remove(sourceName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "missing Pirate display binding fails loudly");

        EnemyExtendedVisualComponent[] editedNinjaComponents = document.Frames[ninjaName];
        document.Frames.Remove(ninjaName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
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
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "v3 override missing a wall-Pirate frame fails loudly");
        document.Frames.Add(wallName, editedWallComponents);

        EnemyExtendedVisualComponent[] steamComponents = document.Frames[steamName];
        document.Frames.Remove(steamName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "current override missing a Ceres steam frame fails loudly");
        document.Frames.Add(steamName, steamComponents);

        EnemyExtendedVisualComponent[] oumComponents = document.Frames[oumName];
        document.Frames.Remove(oumName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "current override missing an Oum frame fails loudly");
        document.Frames.Add(oumName, oumComponents);

        document.Frames.Remove(editedName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "missing extended frame fails loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "malformed extended composition override fails loudly");

        Console.WriteLine(
            "Extended enemy art: 131 Pirate, 11 Ridley, 48 Draygon, 12 Spore Spawn, 28 Ceres steam and 30 Oum OAM frames match native OAM " +
            "at three origins with visual ROM reads forbidden; Pirate and boss edits and " +
            "draw-only frame remaps preserve hitboxes/timers; v1-v8 override " +
            "migration, reload, stock hash and invalid-resource checks pass.");
    }

    private static bool IsBankA5ExtendedFrameName(string name) =>
        name.StartsWith("draygon_oam_", StringComparison.Ordinal) ||
        name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal);

    private static bool IsCeresSteamExtendedFrameName(string name) =>
        name.StartsWith("ceres_steam_oam_", StringComparison.Ordinal);

    private static bool IsOumExtendedFrameName(string name) =>
        name.StartsWith("oum_oam_", StringComparison.Ordinal);

    private static string LegacyExtendedFrameName(string name) =>
        name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)
            ? "draygon_oam_" + name["spore_spawn_oam_".Length..]
            : name;

    private static OamBuffer DrawExtended(EnemyTileArtworkCatalog? art,
        ISnesAddressSpace bus, ushort pointer, ushort x, ushort y,
        Action<RoomEnemySlot>? inspect = null)
    {
        return DrawExtendedForBank(art, bus, EnemyExtendedFrameDefinitions.Bank,
            pointer, x, y, inspect);
    }

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
            ? PirateDefinitionForFrame(pointer) : (ushort)0;
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

    private sealed class ExtendedVisualReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        private readonly HashSet<int> blocked = [];

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
                Block(bank, sprite, 2 + parts * 5);
            }
        }

        private void Block(byte bank, ushort pointer, int length)
        {
            for (int index = 0; index < length; index++)
                blocked.Add((bank << 16) | unchecked((ushort)(pointer + index)));
        }

        public byte ReadByte(int address)
        {
            if (blocked.Contains(address))
                throw new InvalidOperationException(
                    $"Installed extended enemy draw read visual ROM byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
