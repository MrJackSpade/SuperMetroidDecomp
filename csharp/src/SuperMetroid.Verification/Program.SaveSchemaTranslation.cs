using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySaveSchemaTranslation()
    {
        // Isolated arrays only; no player save file is read or changed.
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var ram = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        var samus = new SamusState { ReserveMissiles = 27 };
        var system = new Bank80SystemState { LoadedItemCount = 65535 };
        ram.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(samus, system, 0, 0, japaneseText: true));
        ram.SetGameCompleted(true);
        ram.SelectSlot(0);
        string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(bus, RetailPresentationFixture()));
        AssertTrue(!json.Contains("preservedUntranslatedSram", StringComparison.Ordinal) &&
            json.Contains("\"gameCompleted\": true", StringComparison.Ordinal) &&
            json.Contains("\"reserveMissiles\": 27", StringComparison.Ordinal) &&
            json.Contains("\"loadedItemCount\": 65535", StringComparison.Ordinal) &&
            json.Contains("\"japaneseText\": true", StringComparison.Ordinal), "schema two contains named missing state and no SRAM blob");
        var target = SuperMetroidAddressSpace.CreateWithoutCartridge();
        target.SaveRam.Fill(0xa5);
        GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(json), target, RetailPresentationFixture());
        AssertTrue(target.SaveRam.SequenceEqual(bus.SaveRam), "named schema rebuilds canonical SRAM and clears stale target bytes");
        var loaded = new SuperMetroidSaveRam(target, RetailPresentationFixture()).ReadSlot(0)!;
        var restoredSamus = new SamusState();
        var restoredSystem = new Bank80SystemState();
        loaded.ApplyTo(restoredSamus, restoredSystem);
        AssertEqual(27, restoredSamus.ReserveMissiles, "reserve missiles reach the live Samus owner");
        AssertEqual(65535, restoredSystem.LoadedItemCount, "item load count reaches live progression owner");
        AssertTrue(loaded.JapaneseText && new SuperMetroidSaveRam(target, RetailPresentationFixture()).HasCompletedGame, "language and completion survive save load");
        restoredSystem.WritePersistentMirror(target);
        var mirror = new Bank80SystemState(); mirror.LoadPersistentMirror(target);
        AssertEqual(65535, mirror.LoadedItemCount, "native mirror retains translated item count");

        Suite(nameof(VerifyNamedSaveRuntime), () => VerifyNamedSaveRuntime(bus, ram));

        // Build a genuine old envelope independently: old named fields override its raw mirror.
        JsonObject legacy = JsonNode.Parse(json)!.AsObject();
        legacy["schemaVersion"] = 1;
        legacy.Remove("gameCompleted");
        JsonObject slot = legacy["slots"]![0]!.AsObject();
        slot.Remove("japaneseText"); slot.Remove("loadedItemCount");
        slot["resources"]!.AsObject().Remove("reserveMissiles");
        slot["resources"]!["health"] = 42; // Deliberately differs from native image health 99.
        byte[] native = bus.SaveRam.ToArray();
        native[0x1a00] = 0xa5; // Unused SRAM, outside all slots and metadata.
        var pages = new JsonArray();
        for (int offset = 0; offset < native.Length; offset += 256)
            pages.Add(new JsonObject { ["offset"] = $"0x{offset:X4}", ["bytes"] = Convert.ToHexString(native.AsSpan(offset, 256)) });
        legacy["preservedUntranslatedSram"] = pages;
        string oldJson = legacy.ToJsonString();
        var upgraded = GameSaveJsonCodec.Deserialize(oldJson);
        AssertEqual(2, upgraded.SchemaVersion, "schema-one import becomes schema two");
        GameSaveJsonCodec.Apply(upgraded, target, RetailPresentationFixture());
        var imported = new SuperMetroidSaveRam(target, RetailPresentationFixture()).ReadSlot(0)!;
        AssertEqual(42, imported.Health, "legacy named health overrides stale raw image");
        AssertEqual(27, imported.ReserveMissiles, "legacy importer decodes reserve missile word");
        AssertEqual(65535, imported.LoadedItemCount, "legacy importer decodes item load word");
        AssertTrue(imported.JapaneseText && new SuperMetroidSaveRam(target, RetailPresentationFixture()).HasCompletedGame, "legacy importer decodes language and completion");
        AssertEqual(0, target.SaveRam[0x1a00], "confirmed unused bytes are not perpetuated");
        AssertTrue(!GameSaveJsonCodec.Serialize(upgraded).Contains("preservedUntranslatedSram", StringComparison.Ordinal), "legacy import never re-emits its blob");
        var bad = JsonNode.Parse(oldJson)!.AsObject();
        bad["preservedUntranslatedSram"]![0]!["bytes"] = "not hex";
        AssertInvalidJsonSave(bad.ToJsonString(), "not hexadecimal");
        bad = JsonNode.Parse(oldJson)!.AsObject();
        bad["preservedUntranslatedSram"]!.AsArray().RemoveAt(0);
        AssertInvalidJsonSave(bad.ToJsonString(), "32 pages");
        AssertInvalidJsonSave("{\"schemaVersion\":\"two\"}", "schemaVersion");
        var illegalV2 = JsonNode.Parse(json)!.AsObject();
        illegalV2["preservedUntranslatedSram"] = new JsonArray();
        AssertInvalidJsonSave(illegalV2.ToJsonString(), "preservedUntranslatedSram");

        string fixtureDirectory = Path.Combine(Path.GetTempPath(), "save-schema-two-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixtureDirectory);
        try
        {
            string path = Path.Combine(fixtureDirectory, "fixture.save.json");
            File.WriteAllText(path, oldJson);
            GameSaveFileStore.LoadOrMigrate(target, path, Path.Combine(fixtureDirectory, "absent.srm"), RetailPresentationFixture());
            AssertEqual(oldJson, File.ReadAllText(path + ".bak"), "automatic upgrade backs up the exact original schema-one JSON");
            AssertTrue(!File.ReadAllText(path).Contains("preservedUntranslatedSram", StringComparison.Ordinal), "automatic upgrade replaces old format with named schema");
            AssertEqual(42, new SuperMetroidSaveRam(target, RetailPresentationFixture()).ReadSlot(0)!.Health, "automatic upgrade applies named values");
        }
        finally { Directory.Delete(fixtureDirectory, recursive: true); }
        Console.WriteLine("Schema two: named missing state, runtime restoration, legacy JSON conversion, precedence, padding removal, strict failures and backed-up upgrade pass.");
    }
    private static void VerifyNamedSaveRuntime(SuperMetroidAddressSpace bus, SuperMetroidSaveRam ram)
    {
        // Confirm the translated completion bit is consumed and produced by the frontend.
        var frontend = new SuperMetroidGame(bus);
        frontend.BindMapPresentation(RetailPresentationFixture());
        ram.SelectSlot(2);
        ram.SetGameCompleted(false);
        AssertEqual(3, frontend.AvailableDemoSetCount(), "incomplete save exposes three demo sets");
        ram.SetGameCompleted(true);
        AssertEqual(4, frontend.AvailableDemoSetCount(), "completed save exposes fourth demo set");
        AssertEqual(2, ram.ReadSelectedSlot(), "twelve-byte completion signature preserves selected slot");
        ram.SetGameCompleted(false);
        var native = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var text = SuperMetroid.Core.Assets.EndingTextPresentation.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.EndingTextExtractor.Extract(native), writable: false));
        var ending = new EndingCreditsState(bus, new SuperMetroid.Core.Audio.CartridgeAudioState(), 0, 0, endingText: text);
        ending.BindPaletteFxColors(SuperMetroid.Core.Assets.RoomPaletteFxPresentation.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.RoomPaletteFxPresentationExtractor.Extract(native), writable: false)));
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(EndingCreditsState).GetProperty("Phase")!.SetValue(ending, EndingCreditsPhase.ItemPercentageScrollDown);
        typeof(EndingCreditsState).GetField("phaseTimer", flags)!.SetValue(ending, 1);
        typeof(SuperMetroidGame).GetField("endingCredits", flags)!.SetValue(frontend, ending);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(frontend, SuperMetroidGameState.EndingAndCredits);
        int writes = 0;
        frontend.SaveRamChanged += () => writes++;
        frontend.Step(0);
        AssertTrue(ram.HasCompletedGame, "ending final-message transition persists completion");
        AssertEqual(1, writes, "ending requests durable save exactly on completion transition");
        frontend.Step(0);
        AssertEqual(1, writes, "final-message idle does not repeatedly save");

        var fixtureBus = new TestAddressSpace();
        SeedCollectibleRom(fixtureBus);
        // One representative for each native setup path; precollected items still count.
        var exposed = LoadCollectible(fixtureBus, 0xeed7, 0, precollected: false);
        var shot = LoadCollectible(fixtureBus, 0xef7f, 0, precollected: true);
        AssertEqual(1, exposed.System.LoadedItemCount, "exposed item setup increments saved counter");
        AssertEqual(1, shot.System.LoadedItemCount, "collected shot-block setup increments saved counter");
        shot.System.LoadedItemCount = ushort.MaxValue;
        shot.Plms.LoadRoomPopulation(fixtureBus, shot.Level, shot.Streamer, new SnesVram(),
            SuperMetroid.AssetExtraction.RoomPlmPopulationImporter.Read(fixtureBus, 0x9000),
            shot.System, areaIndex: AreaId.Crateria, getSamus: () => shot.Samus, isAreaTorizoDefeated: () => false);
        AssertEqual(0, shot.System.LoadedItemCount, "saved item-load counter wraps as a native word");
        ram.SelectSlot(0);
    }
}
