using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.CollectiblesVerification;
using static SuperMetroid.Verification.Saves.GameSaveJsonVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Saves;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks named save-schema translation, legacy precedence, rejection paths, and backup-preserving upgrade.</summary>
internal static class SaveSchemaTranslationVerification
{
    /// <summary>Checks named schema-two save state, legacy import precedence, strict failures, and backed-up file upgrade.</summary>
    internal static void VerifySaveSchemaTranslation()
    {
        // Isolated arrays only; no player save file is read or changed.
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var ram = new SaveSlotStore(RetailPresentationFixture());
        var samus = new SamusState { ReserveMissiles = 27 };
        var system = new Bank80SystemState { LoadedItemCount = 65535 };
        ram.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(samus, system, new SaveSnapshotContext { Area = 0, SaveStation = 0, JapaneseText = true }));
        ram.SetGameCompleted(true);
        ram.SelectSlot(0);
        string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(ram));
        AssertTrue(!json.Contains("preservedUntranslatedSram", StringComparison.Ordinal) &&
            json.Contains("\"gameCompleted\": true", StringComparison.Ordinal) &&
            json.Contains("\"reserveMissiles\": 27", StringComparison.Ordinal) &&
            json.Contains("\"loadedItemCount\": 65535", StringComparison.Ordinal) &&
            json.Contains("\"japaneseText\": true", StringComparison.Ordinal), "schema two contains named missing state and no SRAM blob");
        var target = SuperMetroidAddressSpace.CreateWithoutCartridge();
        SaveSlotStore targetSaves = GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(json), RetailPresentationFixture());
        AssertSameBytes(SaveRamCodec.Encode(targetSaves, RetailPresentationFixture()), SaveRamCodec.Encode(ram, RetailPresentationFixture()),
            "named schema rebuilds the canonical SRAM image");
        var loaded = targetSaves.ReadSlot(0);
        var restoredSamus = new SamusState();
        var restoredSystem = new Bank80SystemState();
        (loaded ?? throw new InvalidOperationException("Verification requires loaded.")).ApplyTo(restoredSamus, restoredSystem);
        AssertEqual(27, restoredSamus.ReserveMissiles, "reserve missiles reach the live Samus owner");
        AssertEqual(65535, restoredSystem.LoadedItemCount, "item load count reaches live progression owner");
        AssertTrue(loaded.JapaneseText && targetSaves.HasCompletedGame, "language and completion survive save load");

        Suite(nameof(VerifyNamedSaveRuntime), () => VerifyNamedSaveRuntime(ram));

        // Build a genuine old envelope independently: old named fields override its raw mirror.
        JsonObject legacy = RequiredJson.Parse(json).AsObject();
        legacy["schemaVersion"] = 1;
        legacy.Remove("gameCompleted");
        JsonObject slot = legacy.Required("slots").Required(0).AsObject();
        slot.Remove("japaneseText"); slot.Remove("loadedItemCount");
        slot.Required("resources").AsObject().Remove("reserveMissiles");
        slot.Required("resources")["health"] = 42; // Deliberately differs from native image health 99.
        byte[] native = SaveRamCodec.Encode(ram, RetailPresentationFixture());
        native[0x1a00] = 0xa5; // Unused SRAM, outside all slots and metadata.
        var pages = new JsonArray();
        for (int offset = 0; offset < native.Length; offset += 256)
            pages.Add(new JsonObject { ["offset"] = $"0x{offset:X4}", ["bytes"] = Convert.ToHexString(native.AsSpan(offset, 256)) });
        legacy["preservedUntranslatedSram"] = pages;
        string oldJson = legacy.ToJsonString();
        var upgraded = GameSaveJsonCodec.Deserialize(oldJson);
        AssertEqual(2, upgraded.SchemaVersion, "schema-one import becomes schema two");
        SaveSlotStore importedSaves = GameSaveJsonCodec.ToStore(upgraded, RetailPresentationFixture());
        var imported = importedSaves.ReadSlot(0);
        AssertEqual(42, (imported ?? throw new InvalidOperationException("Verification requires imported.")).Health, "legacy named health overrides stale raw image");
        AssertEqual(27, imported.ReserveMissiles, "legacy importer decodes reserve missile word");
        AssertEqual(65535, imported.LoadedItemCount, "legacy importer decodes item load word");
        AssertTrue(imported.JapaneseText && importedSaves.HasCompletedGame, "legacy importer decodes language and completion");
        AssertEqual(0, SaveRamCodec.Encode(importedSaves, RetailPresentationFixture())[0x1a00], "confirmed unused bytes are not perpetuated");
        AssertTrue(!GameSaveJsonCodec.Serialize(upgraded).Contains("preservedUntranslatedSram", StringComparison.Ordinal), "legacy import never re-emits its blob");
        var bad = RequiredJson.Parse(oldJson).AsObject();
        bad.Required("preservedUntranslatedSram").Required(0)["bytes"] = "not hex";
        AssertInvalidJsonSave(bad.ToJsonString(), "not hexadecimal");
        bad = RequiredJson.Parse(oldJson).AsObject();
        bad.Required("preservedUntranslatedSram").AsArray().RemoveAt(0);
        AssertInvalidJsonSave(bad.ToJsonString(), "32 pages");
        AssertInvalidJsonSave("{\"schemaVersion\":\"two\"}", "schemaVersion");
        var illegalV2 = RequiredJson.Parse(json).AsObject();
        illegalV2["preservedUntranslatedSram"] = new JsonArray();
        AssertInvalidJsonSave(illegalV2.ToJsonString(), "preservedUntranslatedSram");

        using var fixtureDirectoryScratch = new TestScratchDirectory("save-schema-two");
        string fixtureDirectory = fixtureDirectoryScratch.Root;
        Directory.CreateDirectory(fixtureDirectory);
        try
        {
            string path = Path.Combine(fixtureDirectory, "fixture.save.json");
            File.WriteAllText(path, oldJson);
            GameSaveLoadResult upgradedLoad = GameSaveFileStore.LoadOrMigrate(path, Path.Combine(fixtureDirectory, "absent.srm"), RetailPresentationFixture());
            AssertEqual(oldJson, File.ReadAllText(path + ".bak"), "automatic upgrade backs up the exact original schema-one JSON");
            AssertTrue(!File.ReadAllText(path).Contains("preservedUntranslatedSram", StringComparison.Ordinal), "automatic upgrade replaces old format with named schema");
            AssertEqual(42, (upgradedLoad.Saves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires the upgraded slot 0.")).Health, "automatic upgrade applies named values");
        }
        finally { Directory.Delete(fixtureDirectory, recursive: true); }
        Console.WriteLine("Schema two: named missing state, runtime restoration, legacy JSON conversion, precedence, padding removal, strict failures and backed-up upgrade pass.");
    }
    /// <summary>Confirms translated completion and item-load fields are consumed by the live frontend.</summary>
    private static void VerifyNamedSaveRuntime(SaveSlotStore ram)
    {
        // Confirm the translated completion bit is consumed and produced by the frontend.
        var frontend = new SuperMetroidGame(ram);
        frontend.BindArtwork(frontend.Artwork with
        {
            MapPresentation = RetailPresentationFixture(),
        });
        // $80:8261 fixes the demo-set count at boot, so each marker state boots a session.
        ram.SelectSlot(2);
        ram.SetGameCompleted(false);
        AssertEqual(3, new SuperMetroidGame(ram).Session.AvailableDemoSetCount(), "incomplete save exposes three demo sets");
        ram.SetGameCompleted(true);
        AssertEqual(4, new SuperMetroidGame(ram).Session.AvailableDemoSetCount(), "completed save exposes fourth demo set");
        AssertEqual(2, ram.ReadSelectedSlot(), "twelve-byte completion signature preserves selected slot");
        ram.SetGameCompleted(false);
        var native = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var text = SuperMetroid.Core.Frontend.Ending.Artwork.EndingTextPresentation.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Frontend.Ending.EndingTextExtractor.Extract(native), writable: false));
        var ending = new EndingCreditsState(new SuperMetroid.Core.Audio.CartridgeAudioState(), new EndingCreditsSetup { GameTimeHours = 0, GameTimeMinutes = 0, EndingText = text });
        ending.InstalledArtwork.BindPaletteFxColors(SuperMetroid.Core.Fx.Artwork.RoomPaletteFxPresentation.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Fx.RoomPaletteFxPresentationExtractor.Extract(native), writable: false)));
        // The game binds the installed sheets with the ending, which draws its stars from them.
        ending.InstalledArtwork.BindSpriteSheets(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SpriteSheets);
        ending.Stage.Phase = EndingCreditsPhase.ItemPercentageScrollDown;
        ending.Stage.PhaseTimer = 1;
        frontend.Session.Show(ending);
        frontend.GameState = SuperMetroidGameState.EndingAndCredits;
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
        shot.Plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(shot.Level, BgArtworkFixtures.For(new SnesVram()), shot.System, AreaId.Crateria), SuperMetroid.TestSupport.VerificationImporters.RoomPlmPopulationImporter.Read(fixtureBus, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => shot.Samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertEqual(0, shot.System.LoadedItemCount, "saved item-load counter wraps as a native word");
        ram.SelectSlot(0);
    }
}
