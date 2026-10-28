using SuperMetroid.Diagnostics.Saves;
using System.Buffers.Binary;
using static SuperMetroid.Verification.Saves.SaveSchemaTranslationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
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
        var samus = new SamusState
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
        var system = new SharedGameState();
        system.SetEvent(EventNumber.ZebesAwake);
        system.SetEventRaw(63);
        system.SetBossBits(AreaId.Brinstar, BossBits.AreaBoss | BossBits.AreaMiniBoss);
        system.SetRoomChozoBit(17);
        system.SetCollectedItemBit(23);
        system.SetOpenedDoorBit(31);
        system.MarkSaveStationUsed(AreaId.Norfair, 3);
        system.SetAreaMapAcquired(AreaId.Brinstar);
        system.MarkExploredMapTile(AreaId.Brinstar, new(9, 1));
        system.MarkExploredMapTile(AreaId.Brinstar, new(23, 2));
        var time = new GameTimeState();
        time.Load(frames: 41, seconds: 32, minutes: 21, hours: 7);
        ControllerBindings bindings = ControllerBindings.Default.AssignAndSwap(
            action: 0,
            physicalButton: ControllerButton.Y);
        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(samus, system, new SaveSnapshotContext { Area = AreaId.Brinstar, SaveStation = 4, GameTime = time, ControllerBindings = bindings, MoonwalkEnabled = true, IconCancelEnabled = true });
        saveRam.SaveSlot(1, snapshot);

        // The former preservation-only word is the native reserve-missile accumulator.
        snapshot = snapshot with { ReserveMissiles = 165, JapaneseText = true, LoadedItemCount = 321 };
        saveRam.SaveSlot(1, snapshot);
        saveRam.SetGameCompleted(true);
        saveRam.SelectSlot(1);
        byte[] expectedSram = SaveRamCodec.Encode(saveRam, RetailPresentationFixture());

        GameSaveJsonDocument document = GameSaveJsonCodec.Capture(saveRam);
        string json = GameSaveJsonCodec.Serialize(document);
        AssertTrue(json.Contains("\"checkpoint\"", StringComparison.Ordinal) &&
                   json.Contains("\"zebesAwake\"", StringComparison.OrdinalIgnoreCase) &&
                   json.Contains("\"exploredAreas\"", StringComparison.Ordinal),
            "JSON save exposes named checkpoint, event, and explored-map domains");
        AssertEqual(json, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Deserialize(json)),
            "JSON save formatting and property order are deterministic");

        SaveSlotStore restored = GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(json), RetailPresentationFixture());
        AssertTrue(expectedSram.SequenceEqual(SaveRamCodec.Encode(restored, RetailPresentationFixture())),
            "JSON save round trip rebuilds canonical SRAM from named state");
        SuperMetroidSaveSlot restoredSlot = restored.ReadSlot(1) ??
            throw new InvalidOperationException("JSON round trip lost slot 1.");
        AssertEqual((ushort)AreaId.Brinstar, restoredSlot.Area, "JSON checkpoint area");
        AssertEqual(4, restoredSlot.SaveStation, "JSON checkpoint station");
        AssertEqual(94, restoredSlot.Health, "JSON health");
        AssertEqual(15, restoredSlot.MaxMissiles, "JSON missile capacity");
        AssertTrue(restoredSlot.CollectedItems.HasAll(
            SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.VariaSuit),
            "JSON collected equipment flags");
        AssertTrue(restoredSlot.BossBytes.Select(value => (BossBits)value).SequenceEqual(snapshot.BossFlags), "JSON boss flags");
        AssertTrue(restoredSlot.EventBytes.SequenceEqual(snapshot.EventBytes), "JSON event flags");
        AssertTrue(restoredSlot.ExploredMapBytes.SequenceEqual(snapshot.ExploredMapBytes),
            "JSON explored-map coordinates");
        AssertEqual(bindings, restoredSlot.ControllerBindings, "JSON controller bindings");
        AssertEqual(7, restoredSlot.GameTimeHours, "JSON game-time hours");

        AssertInvalidJsonSave(
            json.Replace("\"schemaVersion\": 2", "\"schemaVersion\": 99", StringComparison.Ordinal),
            "schemaVersion");
        AssertInvalidJsonSave(
            json.Replace("\"selectedSlot\": 1,", "\"selectedSlot\": 1,\n  \"unknownField\": true,",
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
        AreaMapPresentation presentation = RetailPresentationFixture();
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

            AssertEqual(expected, decoded.ReadSelectedSlot(), $"{label}: SRAM decode matches the cartridge");
            AssertEqual(FileSelectItemRows.ForSaveSlot(expected), new FileSelectMenuState(decoded, mapPresentation: presentation, spriteSheets: MenuSheetFixtures.For(presentation)).SelectedItem,
                $"{label}: file-select initial cursor matches $81:9ED8");
            AssertEqual(expected, GameSaveJsonCodec.Capture(decoded).SelectedSlot,
                $"{label}: JSON capture keeps the cartridge-selected slot");
        }

        Console.WriteLine("  Selected save slot: imported noncanonical SRAM decodes with the cartridge's range and complement rule.");
    }

    /// <summary>
    /// The JSON numbers the slot document now holds as <see cref="int"/> keep the range the schema always had: the
    /// largest item-load count, debug value, dispatcher route and load station survive store, document, JSON, document
    /// and store again with the same SRAM image, and a number outside its field's range is rejected at parse instead of
    /// wrapping into the store.
    /// </summary>
    internal static void VerifyWidenedSlotFieldsJsonRoundTrip()
    {
        AreaMapPresentation maps = RetailPresentationFixture();
        var store = new SaveSlotStore(maps);
        var samus = new SamusState { Health = 99, MaxHealth = 99, ReserveMissiles = 7 };
        var system = new SharedGameState { LoadedItemCount = ushort.MaxValue };
        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(samus, system,
            new SaveSnapshotContext { Area = AreaId.Maridia, SaveStation = SaveFieldValue.MaximumValue }) with
        {
            DebugFlag = SaveFieldValue.MaximumValue,
            NewFileMarker = 0,
            LoadingGameState = SaveFieldValue.MaximumValue,
        };
        store.SaveSlot(0, snapshot);
        byte[] sram = SaveRamCodec.Encode(store, maps);

        GameSaveJsonDocument document = GameSaveJsonCodec.Capture(store);
        GameSaveSlotJsonDocument slot = document.Slots[0] ?? throw new InvalidOperationException("JSON capture lost slot 0.");
        AssertEqual(SaveFieldValue.MaximumValue, slot.LoadedItemCount, "JSON carries the largest item-load count");
        AssertEqual(SaveFieldValue.MaximumValue, slot.DebugMode, "JSON carries the largest debug value");
        AssertEqual(0, slot.NewFileMarker, "JSON carries a zero new-file marker");
        AssertEqual(SaveFieldValue.MaximumValue, slot.LoadingGameState, "JSON carries an unnamed dispatcher route unchanged");
        AssertEqual(SaveFieldValue.MaximumValue, slot.Checkpoint.SaveStation, "JSON carries the largest load station");
        AssertEqual(7, slot.Resources.ReserveMissiles, "JSON carries the reserve-missile count");

        string json = GameSaveJsonCodec.Serialize(document);
        GameSaveJsonDocument reparsed = GameSaveJsonCodec.Deserialize(json);
        AssertEqual(json, GameSaveJsonCodec.Serialize(reparsed), "document -> JSON -> document is stable");
        AssertTrue(sram.SequenceEqual(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(reparsed, maps), maps)),
            "store -> document -> store rebuilds the same SRAM image byte for byte");

        string Replaced(string from, string to) => json.Contains(from, StringComparison.Ordinal)
            ? json.Replace(from, to, StringComparison.Ordinal)
            : throw new InvalidOperationException($"The fixture JSON lacks {from}.");
        AssertInvalidJsonSave(Replaced("\"loadedItemCount\": 65535", "\"loadedItemCount\": 65536"), "loadedItemCount");
        AssertInvalidJsonSave(Replaced("\"newFileMarker\": 0", "\"newFileMarker\": -1"), "newFileMarker");
        // Counts are signed words, which the cartridge drains below zero; only values outside the word are invalid.
        AssertInvalidJsonSave(Replaced("\"reserveMissiles\": 7", "\"reserveMissiles\": -32769"), "reserveMissiles");
        AssertInvalidJsonSave(Replaced("\"reserveMissiles\": 7", "\"reserveMissiles\": 32768"), "reserveMissiles");
        Console.WriteLine("  JSON save numbers: full-range slot fields round-trip exactly; out-of-range numbers are rejected at parse.");
    }

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
