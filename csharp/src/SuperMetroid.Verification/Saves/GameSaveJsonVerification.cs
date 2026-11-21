using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Diagnostics.Saves;
using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Saves.SaveSchemaTranslationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Saves;
using SuperMetroid.Verification.Harness;

/// <summary>Checks named JSON save persistence against canonical SRAM and legacy migration.</summary>
internal static class GameSaveJsonVerification
{
    /// <summary>
    /// Exercises the named JSON schema, canonical native encoding, strict parsing,
    /// atomic replacement, and one-time legacy migration against the retail map packer.
    /// </summary>
    internal static void VerifyGameSaveJsonPersistence()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
            throw new FileNotFoundException("JSON save verification requires the private retail ROM.", romPath);
        var saveRam = new SaveSlotStore(RetailPresentationFixture());
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Health = 94,
            MaxHealth = 199,
            Missiles = 11,
            MaxMissiles = 15,
            SuperMissiles = 3,
            MaxSuperMissiles = 5,
            PowerBombs = 2,
            MaxPowerBombs = 5,
            ReserveEnergy = 37,
            MaxReserveEnergy = 100,
            ReserveTankMode = ReserveTankMode.Auto,
            SelectedHudItem = HudItem.SuperMissile,
            CollectedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.VariaSuit),
            EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.VariaSuit),
            CollectedBeams = (SamusBeamFlags.Charge | SamusBeamFlags.Wave),
            EquippedBeams = (SamusBeamFlags.Charge | SamusBeamFlags.Wave),
        };
        var system = new SharedGameState(new SharedRandom());
        system.SetEvent(StoryEvent.ZebesAwake);
        system.RecordBossesDefeated(AreaId.Brinstar, DefeatedBosses.AreaBoss | DefeatedBosses.AreaMiniBoss);
        system.SetCollectedItemBit(23);
        system.SetOpenedDoorBit(31);
        system.MarkSaveStationUsed(LoadStationDefinitions.CrocomireSave);
        system.SetAreaMapAcquired(AreaId.Brinstar);
        system.MarkExploredMapTile(AreaId.Brinstar, new(9, 1));
        system.MarkExploredMapTile(AreaId.Brinstar, new(23, 2));
        var time = new GameClockReading(frames: 41, seconds: 32, minutes: 21, hours: 7);
        ControllerBindings bindings = ControllerBindings.Default.AssignAndSwap(
            action: ControllerAction.Shoot,
            physicalButton: ControllerButton.Y);
        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(samus, system, SaveSnapshotFixtures.At(SuperMetroid.Core.Progression.LoadStationDefinitions.RedBrinstarSave) with { GameTime = time, ControllerBindings = bindings, MoonwalkEnabled = true, IconCancelEnabled = true });
        saveRam.SaveSlot(SaveSlot.B, snapshot);

        snapshot = snapshot with { JapaneseText = true };
        saveRam.SaveSlot(SaveSlot.B, snapshot);
        saveRam.CompleteGame();
        saveRam.SelectSlot(SaveSlot.B);
        byte[] expectedSram = SaveRamCodec.Encode(saveRam, RetailPresentationFixture());

        GameSaveJsonDocument document = GameSaveJsonCodec.Capture(saveRam);
        string json = GameSaveJsonCodec.Serialize(document);
        AssertTrue(json.Contains("\"checkpoint\"", StringComparison.Ordinal) &&
                   json.Contains("\"zebesAwake\"", StringComparison.OrdinalIgnoreCase) &&
                   json.Contains("\"exploredAreas\"", StringComparison.Ordinal),
            "JSON save exposes named checkpoint, event, and explored-map domains");
        AssertTrue(json.Contains("\"reserveMode\": \"auto\"", StringComparison.Ordinal) &&
                   json.Contains("\"hudItem\": \"superMissile\"", StringComparison.Ordinal),
            "JSON save names the reserve-tank mode and the HUD selection");
        AssertTrue(!json.Contains("cartridgeDebugFlag", StringComparison.Ordinal) &&
                   !json.Contains("newFileMarker", StringComparison.Ordinal),
            "JSON save keeps no setting the game never reads");
        AssertEqual(json, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Deserialize(json)),
            "JSON save formatting and property order are deterministic");

        SaveSlotStore restored = GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(json), RetailPresentationFixture());
        AssertTrue(expectedSram.SequenceEqual(SaveRamCodec.Encode(restored, RetailPresentationFixture())),
            "JSON save round trip rebuilds canonical SRAM from named state");
        SuperMetroidSaveSlot restoredSlot = restored.ReadSlot(SaveSlot.B) ??
            throw new InvalidOperationException("JSON round trip lost slot 1.");
        AssertTrue(ReferenceEquals(SuperMetroid.Core.Progression.LoadStationDefinitions.RedBrinstarSave, restoredSlot.Checkpoint), "JSON checkpoint station");
        AssertEqual(94, restoredSlot.Inventory.Health, "JSON health");
        AssertEqual(15, restoredSlot.Inventory.MaxMissiles, "JSON missile capacity");
        AssertTrue(restoredSlot.Inventory.CollectedItems.HasAll(
            SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.VariaSuit),
            "JSON collected equipment flags");
        SuperMetroidSaveSlot savedSlot = saveRam.ReadSlot(SaveSlot.B) ?? throw new InvalidOperationException("The saved slot 1 failed.");
        AssertTrue(restoredSlot.Progression.SameAs(savedSlot.Progression),
            "JSON events, bosses, items, doors, stations, maps and explored cells");
        AssertTrue(restoredSlot.Progression.Events.Members.SequenceEqual([StoryEvent.ZebesAwake]),
            "JSON keeps the story events by name");
        AssertEqual(bindings, restoredSlot.ControllerBindings, "JSON controller bindings");
        AssertEqual(ReserveTankMode.Auto, restoredSlot.Inventory.ReserveMode, "JSON reserve-tank mode");
        AssertEqual(HudItem.SuperMissile, restoredSlot.HudItem, "JSON HUD selection");
        AssertEqual(time, restoredSlot.GameTime, "JSON game time");

        AssertInvalidJsonSave(
            json.Replace($"\"schemaVersion\": {GameSaveJsonFormat.SchemaVersion}", "\"schemaVersion\": 99", StringComparison.Ordinal),
            "schemaVersion");
        AssertInvalidJsonSave(
            json.Replace("\"selectedSlot\": \"B\",", "\"selectedSlot\": \"B\",\n  \"unknownField\": true,",
                StringComparison.Ordinal),
            "unknownField");
        AssertInvalidJsonSave(
            json.Replace("\"health\": 94", "\"health\": 999", StringComparison.Ordinal),
            "health");

        using var temporaryDirectoryScratch = new TestScratchDirectory("super-metroid-json-save");
        string temporaryDirectory = temporaryDirectoryScratch.Root;
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            string legacyPath = Path.Combine(temporaryDirectory, "game.srm");
            string jsonPath = Path.Combine(temporaryDirectory, "game.save.json");
            File.WriteAllBytes(legacyPath, expectedSram);
            GameSaveLoadResult firstLoad = GameSaveFileStore.LoadOrMigrate(
                jsonPath,
                legacyPath, RetailPresentationFixture());
            AssertTrue(firstLoad.MigratedLegacySram && File.Exists(jsonPath),
                "legacy SRAM migrates to JSON on first load");
            AssertTrue(expectedSram.SequenceEqual(File.ReadAllBytes(legacyPath)),
                "legacy SRAM remains unchanged after migration");
            AssertTrue(expectedSram.SequenceEqual(SaveRamCodec.Encode(firstLoad.Saves, RetailPresentationFixture())),
                "legacy migration decodes the exact canonical SRAM image");

            GameSaveLoadResult secondLoad = GameSaveFileStore.LoadOrMigrate(
                jsonPath,
                legacyPath, RetailPresentationFixture());
            AssertTrue(!secondLoad.MigratedLegacySram &&
                       expectedSram.SequenceEqual(SaveRamCodec.Encode(secondLoad.Saves, RetailPresentationFixture())),
                "subsequent load prefers JSON and restores identical state");
            GameSaveFileStore.WriteAtomic(secondLoad.Saves, jsonPath);
            AssertTrue(File.Exists(jsonPath + ".bak"),
                "atomic JSON replacement retains the previous complete save");
            AssertEqual(0, Directory.GetFiles(temporaryDirectory, "*.tmp").Length,
                "atomic JSON replacement leaves no partial file");

            File.Delete(jsonPath);
            File.WriteAllBytes(legacyPath, new byte[17]);
            try
            {
                _ = GameSaveFileStore.LoadOrMigrate(jsonPath, legacyPath, RetailPresentationFixture());
                throw new InvalidOperationException("Wrong-sized legacy SRAM was accepted.");
            }
            catch (InvalidDataException exception)
            {
                AssertTrue(exception.Message.Contains("17 bytes", StringComparison.Ordinal),
                    "legacy migration failure explains the actual byte length");
            }
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }

        Console.WriteLine(
            "  JSON saves: named schema, all slot domains, canonical SRAM, strict errors, atomic writes, and legacy migration agree.");
    }

    /// <summary>
    /// Imported SRAM can hold a selected-slot pair the port never writes. The cartridge's
    /// title-to-file-select initialiser (<c>$81:9ED8-9EF0</c>) and copy-completion reload
    /// (<c>$81:9B0C-9B24</c>) keep the stored slot when it is signed 0-2 and
    /// <c>slot &amp; complement == 0</c>; anything else preselects slot A. The boot read,
    /// file-select initialisation, and JSON capture must all decode the same slot.
    /// </summary>
    internal static void VerifySelectedSaveSlotDecode()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
            throw new FileNotFoundException("Selected-slot decode verification requires the private retail ROM.", romPath);
        InstalledScreenContent presentation = RetailPresentationFixture();
        const ushort AllBits = ushort.MaxValue;
        const ushort SignBit = 1 << 15;
        GameSaveJsonCase[] cases =
        [
            new GameSaveJsonCase(0, AllBits, 0), // canonical slot A
            new GameSaveJsonCase(1, AllBits ^ 1, 1), // canonical slot B
            new GameSaveJsonCase(2, AllBits ^ 2, 2), // canonical slot C
            new GameSaveJsonCase(1, 0, 1), // issue #1340: a zero complement is accepted
            new GameSaveJsonCase(2, 0, 2),
            new GameSaveJsonCase(1, 2, 1), // no bits shared with the slot
            new GameSaveJsonCase(2, SignBit | 1, 2),
            new GameSaveJsonCase(0, 0, 0),
            new GameSaveJsonCase(1, 1, 0), // a shared bit rejects
            new GameSaveJsonCase(2, AllBits, 0),
            new GameSaveJsonCase(3, 0, 0), // CMP #$0003 / BPL rejects
            new GameSaveJsonCase(SignBit - 1, 0, 0),
            new GameSaveJsonCase(SignBit | 1, 0, 0), // CMP #$0000 / BMI rejects negative words
            new GameSaveJsonCase(AllBits, 0, 0),
        ];
        // Every consumer only reads the selected-slot pair, so one imported image is
        // rewritten per case.
        var image = new byte[SaveRamLayout.ImageByteCount];
        foreach ((ushort slot, ushort complement, int expected) in cases)
        {
            string label = $"selected slot ${slot:X4} complement ${complement:X4}";
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(SaveRamLayout.SelectedSlotOffset), slot);
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(SaveRamLayout.SelectedSlotComplementOffset), complement);
            SaveSlotStore decoded = SaveRamCodec.Decode(image, presentation);

            AssertEqual(SaveSlots.All[expected], decoded.ReadSelectedSlot(), $"{label}: SRAM decode matches the cartridge");
            AssertEqual(FileSelectItemRows.ForSaveSlot(SaveSlots.All[expected]), new FileSelectMenuState(decoded, AudioQueues.Standalone(), screenContent: presentation, spriteSheets: MenuSheetFixtures.For(presentation)).SelectedItem,
                $"{label}: file-select initial cursor matches $81:9ED8");
            AssertEqual(GameSaveJsonFormat.SlotKey(SaveSlots.All[expected]), GameSaveJsonCodec.Capture(decoded).SelectedSlot,
                $"{label}: JSON capture keeps the cartridge-selected slot");
        }

        Console.WriteLine("  Selected save slot: imported noncanonical SRAM decodes with the cartridge's range and complement rule.");
    }

    /// <summary>
    /// The JSON numbers the slot document now holds as <see cref="int"/> keep the range the schema always had: the
    /// named checkpoint, the startup route and the named HUD selection and reserve-tank mode
    /// survive store, document, JSON, document and store again with the same SRAM image, and a number outside its
    /// field's range, a numbered selection or mode and fields the game never reads, the new-file word and the item set-up
    /// count, are rejected at parse instead of
    /// reaching the store.
    /// </summary>
    internal static void VerifyWidenedSlotFieldsJsonRoundTrip()
    {
        InstalledScreenContent maps = RetailPresentationFixture();
        var store = new SaveSlotStore(maps);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 99, MaxHealth = 99 };
        var system = new SharedGameState(new SharedRandom());
        SuperMetroidSaveSnapshot captured = SuperMetroidSaveSnapshot.Capture(samus, system,
            SaveSnapshotFixtures.At(SuperMetroid.Core.Progression.LoadStationDefinitions.DraygonSave));
        SuperMetroidSaveSnapshot snapshot = captured with
        {
            StartupRoute = SaveStartupRoute.CeresDestruction,
            HudItem = HudItem.XRay,
            Inventory = captured.Inventory with { ReserveMode = ReserveTankMode.Manual },
        };
        store.SaveSlot(SaveSlot.A, snapshot);
        byte[] sram = SaveRamCodec.Encode(store, maps);

        GameSaveJsonDocument document = GameSaveJsonCodec.Capture(store);
        GameSaveSlotJsonDocument slot = document.Slots[SlotA];
        AssertEqual(HudItem.XRay, slot.HudItem, "JSON carries the last selection of the HUD ring");
        AssertEqual(ReserveTankMode.Manual, slot.ReserveMode, "JSON carries the manual reserve-tank mode");
        AssertEqual(SaveStartupRoute.CeresDestruction, slot.StartupRoute, "JSON names the save's startup route");
        AssertEqual(nameof(LoadStationDefinitions.DraygonSave), slot.Checkpoint, "JSON names the checkpoint's load station");

        string json = GameSaveJsonCodec.Serialize(document);
        GameSaveJsonDocument reparsed = GameSaveJsonCodec.Deserialize(json);
        AssertEqual(json, GameSaveJsonCodec.Serialize(reparsed), "document -> JSON -> document is stable");
        AssertTrue(sram.SequenceEqual(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(reparsed, maps), maps)),
            "store -> document -> store rebuilds the same SRAM image byte for byte");

        string Replaced(string from, string to) => json.Contains(from, StringComparison.Ordinal)
            ? json.Replace(from, to, StringComparison.Ordinal)
            : throw new InvalidOperationException($"The fixture JSON lacks {from}.");
        AssertInvalidJsonSave(Replaced("\"hudItem\": \"xRay\"", "\"hudItem\": 5"), "hudItem");
        AssertInvalidJsonSave(Replaced("\"checkpoint\": \"DraygonSave\"", "\"checkpoint\": \"Draygon\""), "load station");
        AssertInvalidJsonSave(Replaced("\"reserveMode\": \"manual\"", "\"reserveMode\": \"overflow\""), "reserveMode");
        AssertInvalidJsonSave(Replaced("\"hudItem\": \"xRay\"", "\"hudItem\": \"xRay\",\n      \"newFileMarker\": 1"), "newFileMarker");
        AssertInvalidJsonSave(Replaced("\"hudItem\": \"xRay\"", "\"hudItem\": \"xRay\",\n      \"loadedItemCount\": 0"), "loadedItemCount");
        // Counts are signed words, which the cartridge drains below zero; only values outside the word are invalid.
        AssertInvalidJsonSave(Replaced("\"health\": 99", "\"health\": -32769"), "health");
        AssertInvalidJsonSave(Replaced("\"maxHealth\": 99", "\"maxHealth\": 32768"), "maxHealth");
        // The current schema always writes the startup route, so a slot without one is malformed, not a main-game save.
        JsonObject withoutRoute = RequiredJson.Parse(json).AsObject();
        withoutRoute.Required("slots").Required(SlotA).AsObject().Remove("startupRoute");
        AssertInvalidJsonSave(withoutRoute.ToJsonString(), "startupRoute");
        Console.WriteLine("  JSON save numbers: full-range slot fields round-trip exactly; out-of-range numbers and a checkpoint naming no load station are rejected at parse.");
    }

    /// <summary>
    /// #1623: a used-station number that names no station Samus uses, which the SpaceTime beam's overwrite of the record
    /// set while saves kept every number's bit, is dropped by the schema-eight upgrade (#1723) as the save-RAM table decoder
    /// drops its bit, so such a save loads with the same used stations from either; the current schema, which names the
    /// stations, rejects a name that is no station Samus uses; and every usable station round-trips, alone, through the
    /// JSON save and the save-RAM table.
    /// </summary>
    internal static void VerifyUsedStationNumbersWithoutStation()
    {
        InstalledScreenContent maps = RetailPresentationFixture();
        string SavedJson(LoadStation used)
        {
            var store = new SaveSlotStore(maps);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 99, MaxHealth = 99 };
            var system = new SharedGameState(new SharedRandom());
            system.MarkSaveStationUsed(used);
            store.SaveSlot(SaveSlot.A, SuperMetroidSaveSnapshot.Capture(samus, system,
                SaveSnapshotFixtures.At(LoadStationDefinitions.DraygonSave)));
            return GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(store));
        }
        PersistentProgress Loaded(string json) =>
            (GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(json), maps).ReadSlot(SaveSlot.A)
                ?? throw new InvalidOperationException("The JSON save lost slot 0.")).Progression;

        byte[] table = new byte[SuperMetroid.AssetExtraction.Progression.ProgressionTableCodec.UsedStationByteCount];
        foreach (LoadStation station in LoadStationDefinitions.Usable)
        {
            AssertTrue(Loaded(SavedJson(station)).UsedStations.Members.SequenceEqual([station]),
                $"the JSON save keeps {station} used, and nothing else");
            var written = new PersistentProgress();
            written.UsedStations.Add(station);
            SuperMetroid.AssetExtraction.Progression.ProgressionTableCodec.WriteUsedStations(written, table);
            var read = new PersistentProgress();
            SuperMetroid.AssetExtraction.Progression.ProgressionTableCodec.ReadUsedStations(table, read);
            AssertTrue(read.UsedStations.Members.SequenceEqual([station]), $"the save-RAM table keeps {station} used, and nothing else");
        }

        // The current schema names the stations; a name that is no station Samus uses is rejected.
        JsonObject named = RequiredJson.Parse(SavedJson(LoadStationDefinitions.CrocomireSave)).AsObject();
        JsonObject namedProgression = named.Required("slots").Required(SlotA).Required("progression").AsObject();
        AssertTrue(namedProgression.Required("usedStations").AsArray().Select(name => name?.GetValue<string>())
                .SequenceEqual([nameof(LoadStationDefinitions.CrocomireSave)]),
            "the fixture save names the Crocomire save station");
        namedProgression["usedStations"] = new JsonArray(nameof(LoadStationDefinitions.ZebesArrival));
        AssertInvalidJsonSave(named.ToJsonString(), "usedStations");

        // Schema eight listed used stations by their numbers (#1723); Norfair's list numbers stations 0 through 5 and 8
        // through 10, so 6 and 15 name none, and the upgrade drops them.
        JsonObject overwritten = RequiredJson.Parse(SavedJson(LoadStationDefinitions.CrocomireSave)).AsObject();
        overwritten["schemaVersion"] = 8;
        ToSchemaFifteenSlots(overwritten);
        ToSchemaEightSlots(overwritten);
        JsonObject norfair = overwritten.Required("slots").Required(0).Required("progression").Required("usedStations").AsArray()
            .Select(entry => entry?.AsObject() ?? throw new InvalidOperationException("A used-station entry is null."))
            .Single(entry => entry.Required("area").GetValue<string>() == "norfair");
        AssertTrue(norfair.Required("stations").AsArray().Select(number => number?.GetValue<int>()).SequenceEqual([3]),
            "the schema-eight fixture lists the Crocomire save station by its number");
        norfair["stations"] = new JsonArray(3, 6, 15);
        PersistentProgress fromJson = (GameSaveJsonCodec.ToStore(LegacyGameSaveJson.Deserialize(overwritten.ToJsonString(), "game save", out _), maps)
            .ReadSlot(SaveSlot.A) ?? throw new InvalidOperationException("The upgraded JSON save lost slot 0.")).Progression;

        Array.Clear(table);
        table[SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Norfair) * sizeof(ushort)] = 1 << 3 | 1 << 6;
        table[SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Norfair) * sizeof(ushort) + 1] = 1 << 7;
        var fromTable = new PersistentProgress();
        SuperMetroid.AssetExtraction.Progression.ProgressionTableCodec.ReadUsedStations(table, fromTable);
        AssertTrue(fromJson.UsedStations.Members.SequenceEqual([LoadStationDefinitions.CrocomireSave]),
            "a schema-eight save listing numbers that name no station upgrades with only the station it names");
        AssertTrue(fromTable.UsedStations.Members.SequenceEqual(fromJson.UsedStations.Members),
            "the save-RAM table's bits for those numbers decode to the same used stations");
        Console.WriteLine($"  Used stations: all {LoadStationDefinitions.Usable.Count} usable stations round-trip alone; numbers naming no station are dropped by the upgrade and the table decoder.");
    }

    /// <summary>
    /// #2140: the save file's history keeps a power-on that found no valid save. A store whose boot found no valid save,
    /// then saved a slot, writes that history to JSON and reads it back, so the next power-on still adds the rumble's frame
    /// as the cartridge's kept marker does; a save holding both a completed game and that power-on is rejected; and a
    /// schema-eleven save, which kept no such history, upgrades to none.
    /// </summary>
    internal static void VerifyPowerOnHistoryJsonRoundTrip()
    {
        InstalledScreenContent maps = RetailPresentationFixture();
        var store = new SaveSlotStore(maps);
        AssertTrue(!store.DetermineCompletionDemoSetUnlocked(), "a boot with no valid save leaves the completion demo set out");
        AssertEqual(SaveFileHistory.PowerOnFoundNoValidSave, store.History, "a boot with no valid save keeps it in the history");
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 99, MaxHealth = 99 };
        store.SaveSlot(SaveSlot.A, SuperMetroidSaveSnapshot.Capture(samus, new SharedGameState(new SharedRandom()),
            SaveSnapshotFixtures.At(LoadStationDefinitions.DraygonSave)));
        AssertEqual(SaveFileHistory.PowerOnFoundNoValidSave, store.History, "saving a slot leaves the history");

        string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(store));
        AssertTrue(json.Contains("\"gameCompleted\": false", StringComparison.Ordinal) &&
            json.Contains("\"powerOnFoundNoValidSave\": true", StringComparison.Ordinal), "the JSON save names the history");
        SaveSlotStore loaded = GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(json), maps);
        AssertEqual(SaveFileHistory.PowerOnFoundNoValidSave, loaded.History, "the history survives the JSON save");
        AssertEqual(1, loaded.EarthquakeSoundDelayAtPowerOn().ExtraFrames,
            "a valid save after a power-on that found none adds the rumble's frame at the next power-on");
        AssertInvalidJsonSave(json.Replace("\"gameCompleted\": false", "\"gameCompleted\": true", StringComparison.Ordinal),
            "powerOnFoundNoValidSave");

        JsonObject schemaEleven = RequiredJson.Parse(json).AsObject();
        schemaEleven["schemaVersion"] = 11;
        ToSchemaFifteenSlots(schemaEleven);
        schemaEleven.Remove("powerOnFoundNoValidSave");
        GameSaveJsonDocument upgraded = LegacyGameSaveJson.Deserialize(schemaEleven.ToJsonString(), "game save", out bool migrated);
        AssertTrue(migrated, "a schema-eleven save is upgraded");
        AssertTrue(!upgraded.PowerOnFoundNoValidSave && !upgraded.GameCompleted, "a schema-eleven save upgrades to no history");
        Console.WriteLine("  Save-file history: a power-on that found no valid save survives the JSON save; schema eleven upgrades to none.");
    }

    /// <summary>The store the earlier save holds: several story events, items, doors, stations, maps and explored cells.</summary>
    private static SaveSlotStore EarlierSaveStore(InstalledScreenContent maps)
    {
        var store = new SaveSlotStore(maps);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 99, MaxHealth = 99 };
        var system = new SharedGameState(new SharedRandom());
        system.SetEvent(StoryEvent.ZebesAwake);
        system.SetEvent(StoryEvent.PhantoonStatueGrey);
        system.SetEvent(StoryEvent.TourianUnlocked);
        system.SetEvent(StoryEvent.CrittersEscaped);
        system.SetCollectedItemBit(5);
        system.SetCollectedItemBit(23);
        system.SetCollectedItemBit(64);
        system.SetOpenedDoorBit(3);
        system.SetOpenedDoorBit(31);
        system.MarkSaveStationUsed(LoadStationDefinitions.CrocomireSave);
        system.MarkSaveStationUsed(LoadStationDefinitions.DraygonSave);
        system.SetAreaMapAcquired(AreaId.Brinstar);
        system.SetAreaMapAcquired(AreaId.Maridia);
        system.MarkExploredMapTile(AreaId.Brinstar, new(9, 1));
        system.MarkExploredMapTile(AreaId.Maridia, new(4, 7));
        store.SaveSlot(SaveSlot.A, SuperMetroidSaveSnapshot.Capture(samus, system, SaveSnapshotFixtures.At(LoadStationDefinitions.DraygonSave)
            with { GameTime = new GameClockReading(frames: 7, seconds: 59, minutes: 3, hours: 12) }));
        return store;
    }

    /// <summary>
    /// #2543: a save the game wrote before saves read their story events, items, doors and stations in any order still
    /// loads and is written again byte for byte, and the same save with each of those lists reversed loads the same progress
    /// and is written in the same order; a name listed twice is rejected.
    /// </summary>
    internal static void VerifyEarlierSaveLoadsInAnyListOrder()
    {
        InstalledScreenContent maps = RetailPresentationFixture();
        // The serializer ends its lines with the platform's line ending, so the fixture is compared with the same endings.
        string earlier = EarlierSaveJson.ReplaceLineEndings(Environment.NewLine) + Environment.NewLine;
        SaveSlotStore store = EarlierSaveStore(maps);
        AssertEqual(earlier, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(store)),
            "the game writes the earlier save's progress byte for byte as it did before");
        AssertEqual(earlier, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(earlier), maps))),
            "the earlier save loads and is written again byte for byte");
        byte[] sram = SaveRamCodec.Encode(store, maps);
        AssertTrue(sram.SequenceEqual(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(earlier), maps), maps)),
            "the earlier save loads the progress it was written from");

        JsonObject reversed = RequiredJson.Parse(earlier).AsObject();
        JsonObject progression = reversed.Required("slots").Required(SlotA).Required("progression").AsObject();
        foreach (string list in (string[])["events", "collectedItems", "openedDoors", "usedStations"])
        {
            string[] names = [.. progression.Required(list).AsArray().Select(name => name?.GetValue<string>()
                ?? throw new InvalidOperationException($"The fixture's {list} lists a null."))];
            AssertTrue(names.Length > 1, $"the fixture's {list} lists more than one name, so reversing reorders it");
            progression[list] = new JsonArray([.. names.Reverse().Select(name => (JsonNode)JsonValue.Create(name))]);
        }
        GameSaveJsonDocument reordered = GameSaveJsonCodec.Deserialize(reversed.ToJsonString());
        SaveSlotStore reorderedStore = GameSaveJsonCodec.ToStore(reordered, maps);
        SuperMetroidSaveSlot loaded = reorderedStore.ReadSlot(SaveSlot.A)
            ?? throw new InvalidOperationException("The reordered save lost slot 0.");
        SuperMetroidSaveSlot saved = store.ReadSlot(SaveSlot.A) ?? throw new InvalidOperationException("The earlier save lost slot 0.");
        AssertTrue(loaded.Progression.SameAs(saved.Progression),
            "a save listing its events, items, doors and stations in reverse loads the same progress");
        AssertEqual(earlier, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(reorderedStore)),
            "the reordered save's progress is written again in the game's order");

        foreach ((string list, string name) in (ValueTuple<string, string>[])[("events", "PhantoonStatueGrey"), ("collectedItems", "BigPinkChozoChargeBeam"),
                     ("openedDoors", "LandingSiteGreyDoorFacingLeftAt142x70"), ("usedStations", "DraygonSave")])
        {
            JsonObject twice = RequiredJson.Parse(earlier).AsObject();
            twice.Required("slots").Required(SlotA).Required("progression").Required(list).AsArray().Add(name);
            AssertInvalidJsonSave(twice.ToJsonString(), list);
        }
        Console.WriteLine("  Earlier save: loads and is written again byte for byte; reversed lists load the same progress; a name listed twice is rejected.");
    }

    /// <summary>
    /// The save <see cref="EarlierSaveStore"/> holds, as the game wrote it before saves read their lists in any order
    /// (#2543, captured at 371cc2aaf5), its slots keyed by name as schema sixteen keys them (#2565).
    /// </summary>
    private const string EarlierSaveJson = """
        {
          "schemaVersion": 16,
          "selectedSlot": "A",
          "slots": {
            "A": {
              "checkpoint": "DraygonSave",
              "resources": {
                "health": 99,
                "maxHealth": 99,
                "missiles": 0,
                "maxMissiles": 0,
                "superMissiles": 0,
                "maxSuperMissiles": 0,
                "powerBombs": 0,
                "maxPowerBombs": 0,
                "reserveEnergy": 0,
                "maxReserveEnergy": 0
              },
              "inventory": {
                "collectedItems": "none",
                "equippedItems": "none",
                "collectedBeams": "none",
                "equippedBeams": "none"
              },
              "gameTime": {
                "hours": 12,
                "minutes": 3,
                "seconds": 59,
                "frames": 7
              },
              "controller": {
                "shoot": "x",
                "jump": "a",
                "dash": "b",
                "itemSelect": "select",
                "itemCancel": "y",
                "aimUp": "r",
                "aimDown": "l"
              },
              "progression": {
                "events": [
                  "ZebesAwake",
                  "PhantoonStatueGrey",
                  "TourianUnlocked",
                  "CrittersEscaped"
                ],
                "zebetitesDestroyed": 0,
                "defeatedBosses": [
                  {
                    "area": "crateria",
                    "bosses": "none"
                  },
                  {
                    "area": "brinstar",
                    "bosses": "none"
                  },
                  {
                    "area": "norfair",
                    "bosses": "none"
                  },
                  {
                    "area": "wreckedShip",
                    "bosses": "none"
                  },
                  {
                    "area": "maridia",
                    "bosses": "none"
                  },
                  {
                    "area": "tourian",
                    "bosses": "none"
                  },
                  {
                    "area": "ceres",
                    "bosses": "none"
                  }
                ],
                "collectedItems": [
                  "GauntletEnergyTankExposedEnergyTank",
                  "BigPinkChozoChargeBeam",
                  "BubbleMountainExposedMissileTank"
                ],
                "openedDoors": [
                  "LandingSiteGreyDoorFacingLeftAt142x70",
                  "GreenBrinstarMainShaftRedDoorFacingRightAt1x86"
                ],
                "usedStations": [
                  "CrocomireSave",
                  "DraygonSave"
                ],
                "acquiredMaps": [
                  "brinstar",
                  "maridia"
                ],
                "exploredAreas": [
                  {
                    "area": "crateria",
                    "tiles": []
                  },
                  {
                    "area": "brinstar",
                    "tiles": [
                      {
                        "x": 9,
                        "y": 1
                      }
                    ]
                  },
                  {
                    "area": "norfair",
                    "tiles": []
                  },
                  {
                    "area": "wreckedShip",
                    "tiles": []
                  },
                  {
                    "area": "maridia",
                    "tiles": []
                  },
                  {
                    "area": "tourian",
                    "tiles": []
                  },
                  {
                    "area": "ceres",
                    "tiles": []
                  }
                ]
              },
              "reserveMode": "none",
              "hudItem": "beam",
              "moonwalkEnabled": false,
              "iconCancelEnabled": false,
              "japaneseText": false,
              "startupRoute": "mainGame"
            }
          },
          "gameCompleted": false,
          "powerOnFoundNoValidSave": false
        }
        """;

    /// <summary>Confirms malformed save JSON is rejected with a diagnostic naming the invalid field or value.</summary>
    internal static void AssertInvalidJsonSave(string json, string expectedMessagePart)
    {
        try
        {
            _ = GameSaveJsonCodec.Deserialize(json, "invalid JSON save fixture");
            throw new InvalidOperationException(
                $"Invalid JSON save containing '{expectedMessagePart}' was accepted.");
        }
        catch (InvalidDataException exception)
        {
            AssertTrue(exception.Message.Contains(expectedMessagePart, StringComparison.OrdinalIgnoreCase),
                $"invalid JSON save identifies '{expectedMessagePart}'");
        }
    }

    /// <summary>Runs JSON persistence and schema-translation verification.</summary>
    [VerificationCommand("--game-save-json")]
    internal static int GameSaveJsonCommand()
    {
        Suite(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence());
        Suite(nameof(VerifySaveSchemaTranslation), () => VerifySaveSchemaTranslation());
        Suite(nameof(VerifySelectedSaveSlotDecode), () => VerifySelectedSaveSlotDecode());
        Suite(nameof(VerifyWidenedSlotFieldsJsonRoundTrip), () => VerifyWidenedSlotFieldsJsonRoundTrip());
        Suite(nameof(VerifyUsedStationNumbersWithoutStation), () => VerifyUsedStationNumbersWithoutStation());
        Suite(nameof(VerifyPowerOnHistoryJsonRoundTrip), () => VerifyPowerOnHistoryJsonRoundTrip());
        Suite(nameof(VerifyEarlierSaveLoadsInAnyListOrder), () => VerifyEarlierSaveLoadsInAnyListOrder());
        return 0;
    }

    /// <summary>Runs the selected-save-slot decode suite.</summary>
    [VerificationCommand("--selected-save-slot-decode")]
    internal static int SelectedSaveSlotDecodeCommand()
    {
        Suite(nameof(VerifySelectedSaveSlotDecode), () => VerifySelectedSaveSlotDecode());
        return 0;
    }

    /// <summary>Runs the focused JSON save round-trip and migration verification command.</summary>
    [VerificationCommand("--save-json-persistence")]
    internal static int SaveJsonPersistenceCommand()
    {
        Suite(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence());
        return 0;
    }
}
