using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectiblesVerification;
using static SuperMetroid.Verification.Saves.GameSaveJsonVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Saves;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Fx;

/// <summary>Checks named save-schema translation, legacy precedence, rejection paths, and backup-preserving upgrade.</summary>
internal static class SaveSchemaTranslationVerification
{
    /// <summary>Checks named schema-two save state, legacy import precedence, strict failures, and backed-up file upgrade.</summary>
    internal static void VerifySaveSchemaTranslation()
    {
        // Isolated arrays only; no player save file is read or changed.
        var ram = new SaveSlotStore(RetailPresentationFixture());
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            ReserveMissiles = 27,
            // A selection and mode other than the first of each, which schema six kept as numbers (#1605).
            SelectedHudItem = HudItem.PowerBomb,
            ReserveTankMode = ReserveTankMode.Manual,
        };
        var system = new SharedGameState(new SharedRandom());
        // Progress in every set schema two listed by table position, so its upgrade is exercised (#1475).
        system.RecordBossesDefeated(AreaId.Brinstar, DefeatedBosses.AreaBoss | DefeatedBosses.AreaTorizo);
        system.MarkSaveStationUsed(LoadStationDefinitions.CrocomireSave);
        system.Progress.UsedStations.Add(LoadStationDefinitions.MaridiaElevator);
        system.SetAreaMapAcquired(AreaId.WreckedShip);
        system.SetCollectedItemBit(23);
        system.SetOpenedDoorBit(31);
        // A Zebetite count and the Shitroid-approach event, which schema four listed as counter events and by number (#1527).
        system.Progress.ZebetitesDestroyed = 3;
        system.SetEvent(StoryEvent.ShitroidEncounterMusicSkipped);
        ram.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(samus, system, new SaveSnapshotContext { Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.Gunship, JapaneseText = true }));
        ram.SetGameCompleted(true);
        ram.SelectSlot(0);
        string json = GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Capture(ram));
        AssertTrue(!json.Contains("preservedUntranslatedSram", StringComparison.Ordinal) &&
            json.Contains("\"gameCompleted\": true", StringComparison.Ordinal) &&
            json.Contains("\"reserveMissiles\": 27", StringComparison.Ordinal) &&
            !json.Contains("loadedItemCount", StringComparison.Ordinal) &&
            json.Contains("\"japaneseText\": true", StringComparison.Ordinal), "schema two contains named missing state and no SRAM blob");
        SaveSlotStore targetSaves = GameSaveJsonCodec.ToStore(GameSaveJsonCodec.Deserialize(json), RetailPresentationFixture());
        AssertSameBytes(SaveRamCodec.Encode(targetSaves, RetailPresentationFixture()), SaveRamCodec.Encode(ram, RetailPresentationFixture()),
            "named schema rebuilds the canonical SRAM image");
        var loaded = targetSaves.ReadSlot(0);
        var restoredSamus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        var restoredSystem = new SharedGameState(new SharedRandom());
        (loaded ?? throw new InvalidOperationException("Verification requires loaded.")).ApplyTo(restoredSamus, restoredSystem);
        AssertEqual(27, restoredSamus.ReserveMissiles, "reserve missiles reach the live Samus owner");
        AssertTrue(loaded.JapaneseText && targetSaves.HasCompletedGame, "language and completion survive save load");

        Suite(nameof(VerifyNamedSaveRuntime), () => VerifyNamedSaveRuntime(ram));

        // A schema-two save lists bosses by the eight area slots, stations and maps by table bit; it upgrades to the same store.
        JsonObject schemaTwo = RequiredJson.Parse(json).AsObject();
        schemaTwo["schemaVersion"] = 2;
        ToSchemaFourProgression(schemaTwo);
        ToSchemaTwoProgression(schemaTwo);
        GameSaveJsonDocument upgradedTwo = LegacyGameSaveJson.Deserialize(schemaTwo.ToJsonString(), "game save", out bool migratedTwo);
        AssertTrue(migratedTwo, "a schema-two save is upgraded");
        AssertEqual(GameSaveJsonFormat.SchemaVersion, upgradedTwo.SchemaVersion, "schema two becomes the current schema");
        AssertSameBytes(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(upgradedTwo, RetailPresentationFixture()), RetailPresentationFixture()),
            SaveRamCodec.Encode(ram, RetailPresentationFixture()), "a schema-two save upgrades to the same saves");

        // A schema-three save keeps the cartridge's loading-state word; it upgrades to the startup route that word selects
        // (#1523), and a word the cartridge does not name resumes the main game.
        UpgradesSchemaThree(0x0022, SaveLoadingGameState.CeresDestruction);
        UpgradesSchemaThree(0x0880, SaveLoadingGameState.MainGame);
        void UpgradesSchemaThree(int word, SaveLoadingGameState route)
        {
            JsonObject schemaThree = RequiredJson.Parse(json).AsObject();
            schemaThree["schemaVersion"] = 3;
            ToSchemaFourProgression(schemaThree);
            JsonObject threeSlot = schemaThree.Required("slots").Required(0).AsObject();
            threeSlot.Remove("startupRoute");
            threeSlot["loadingGameState"] = word;
            GameSaveJsonDocument upgradedThree = LegacyGameSaveJson.Deserialize(schemaThree.ToJsonString(), "game save", out bool migratedThree);
            AssertTrue(migratedThree, "a schema-three save is upgraded");
            AssertEqual(route, (upgradedThree.Slots[0] ?? throw new InvalidOperationException("The upgraded save lost slot 0.")).StartupRoute,
                $"schema three's loading-state word ${word:X4} becomes its startup route");
        }

        // A schema-eight save names its checkpoint and used stations by their numbers in each area's station list (#1723);
        // the current schema's strict reader rejects the numbers, and the save upgrades to the stations' names: the same
        // save as the current schema writes, and the same saves. A checkpoint number naming no load station is rejected.
        AssertTrue(json.Contains($"\"checkpoint\": \"{nameof(LoadStationDefinitions.Gunship)}\"", StringComparison.Ordinal) &&
            json.Contains($"\"{nameof(LoadStationDefinitions.CrocomireSave)}\"", StringComparison.Ordinal) &&
            json.Contains($"\"{nameof(LoadStationDefinitions.MaridiaElevator)}\"", StringComparison.Ordinal),
            "the current schema names the checkpoint and used stations by their load stations");
        JsonObject schemaEight = RequiredJson.Parse(json).AsObject();
        schemaEight["schemaVersion"] = 8;
        ToSchemaEightSlots(schemaEight);
        AssertEqual(0, schemaEight.Required("slots").Required(0).Required("checkpoint").Required("saveStation").GetValue<int>(),
            "the schema-eight fixture numbers the gunship as Crateria's station list does");
        JsonObject eightAsCurrent = RequiredJson.Parse(schemaEight.ToJsonString()).AsObject();
        eightAsCurrent["schemaVersion"] = GameSaveJsonFormat.SchemaVersion;
        AssertInvalidJsonSave(eightAsCurrent.ToJsonString(), "checkpoint");
        GameSaveJsonDocument upgradedEight = LegacyGameSaveJson.Deserialize(schemaEight.ToJsonString(), "game save", out bool migratedEight);
        AssertTrue(migratedEight, "a schema-eight save is upgraded");
        AssertEqual(json, GameSaveJsonCodec.Serialize(upgradedEight), "a schema-eight save upgrades to the save the current schema writes");
        AssertSameBytes(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(upgradedEight, RetailPresentationFixture()), RetailPresentationFixture()),
            SaveRamCodec.Encode(ram, RetailPresentationFixture()), "a schema-eight save upgrades to the same saves");
        JsonObject badEight = RequiredJson.Parse(schemaEight.ToJsonString()).AsObject();
        badEight.Required("slots").Required(0).Required("checkpoint")["saveStation"] = 7;
        AssertInvalidLegacyJsonSave(badEight.ToJsonString(), "which is no load station");

        // A schema-nine save could list the equipment bit no item records, which Golden Torizo's item code set and nothing
        // reads (#1778); the save upgrades without it, and a list it alone made up becomes no item.
        JsonObject schemaNine = RequiredJson.Parse(json).AsObject();
        schemaNine["schemaVersion"] = 9;
        JsonObject nineInventory = schemaNine.Required("slots").Required(0).Required("inventory").AsObject();
        nineInventory["collectedItems"] = "springBall, unassignedItem";
        nineInventory["equippedItems"] = "unassignedItem";
        GameSaveJsonDocument upgradedNine = LegacyGameSaveJson.Deserialize(schemaNine.ToJsonString(), "game save", out bool migratedNine);
        AssertTrue(migratedNine, "a schema-nine save is upgraded");
        SaveInventoryJsonDocument nineItems = (upgradedNine.Slots[0] ?? throw new InvalidOperationException("The upgraded save lost slot 0.")).Inventory;
        AssertEqual(SamusEquipmentFlags.SpringBall, nineItems.CollectedItems, "schema nine's unused item bit is not carried among collected items");
        AssertEqual(SamusEquipmentFlags.None, nineItems.EquippedItems, "an equipped list of only the unused item bit becomes no item");

        // A schema-seven save carries the count of item set-ups, which nothing in the game reads (#1691); the current
        // schema's strict reader rejects it, and the save upgrades to the same saves without it.
        JsonObject schemaSeven = RequiredJson.Parse(json).AsObject();
        schemaSeven["schemaVersion"] = 7;
        ToSchemaSevenSlots(schemaSeven);
        JsonObject sevenAsCurrent = RequiredJson.Parse(json).AsObject();
        sevenAsCurrent.Required("slots").Required(0)["loadedItemCount"] = 65535;
        AssertInvalidJsonSave(sevenAsCurrent.ToJsonString(), "loadedItemCount");
        GameSaveJsonDocument upgradedSeven = LegacyGameSaveJson.Deserialize(schemaSeven.ToJsonString(), "game save", out bool migratedSeven);
        AssertTrue(migratedSeven, "a schema-seven save is upgraded");
        AssertTrue(!GameSaveJsonCodec.Serialize(upgradedSeven).Contains("loadedItemCount", StringComparison.Ordinal),
            "a schema-seven save's item set-up count is not carried");
        AssertSameBytes(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(upgradedSeven, RetailPresentationFixture()), RetailPresentationFixture()),
            SaveRamCodec.Encode(ram, RetailPresentationFixture()), "a schema-seven save upgrades to the same saves");

        // A schema-six save keeps the HUD selection and reserve-tank mode as numbers and carries the debug build's
        // placement setting and the never-read new-file word (#1605); it upgrades to the names and the same saves,
        // whatever the two unread words held, and a number naming no selection or mode is rejected.
        JsonObject schemaSix = RequiredJson.Parse(json).AsObject();
        schemaSix["schemaVersion"] = 6;
        ToSchemaSixSlots(schemaSix);
        JsonObject sixSlot = schemaSix.Required("slots").Required(0).AsObject();
        sixSlot["cartridgeDebugFlag"] = 0;
        sixSlot["newFileMarker"] = 65535;
        // The current schema's strict reader never takes schema six's numbers in place of the names.
        JsonObject sixAsCurrent = RequiredJson.Parse(json).AsObject();
        sixAsCurrent.Required("slots").Required(0)["reserveMode"] = sixSlot.Required("reserveMode").DeepClone();
        AssertInvalidJsonSave(sixAsCurrent.ToJsonString(), "reserveMode");
        GameSaveJsonDocument upgradedSix = LegacyGameSaveJson.Deserialize(schemaSix.ToJsonString(), "game save", out bool migratedSix);
        AssertTrue(migratedSix, "a schema-six save is upgraded");
        GameSaveSlotJsonDocument upgradedSixSlot = upgradedSix.Slots[0] ?? throw new InvalidOperationException("The upgraded save lost slot 0.");
        AssertEqual(HudItem.PowerBomb, upgradedSixSlot.HudItem, "schema six's HUD selection number becomes the selection");
        AssertEqual(ReserveTankMode.Manual, upgradedSixSlot.ReserveMode, "schema six's reserve-mode number becomes the mode");
        AssertSameBytes(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(upgradedSix, RetailPresentationFixture()), RetailPresentationFixture()),
            SaveRamCodec.Encode(ram, RetailPresentationFixture()),
            "a schema-six save upgrades to the same saves, without the words the game never reads");
        JsonObject badSix = RequiredJson.Parse(schemaSix.ToJsonString()).AsObject();
        badSix.Required("slots").Required(0)["hudItem"] = 6;
        AssertInvalidLegacyJsonSave(badSix.ToJsonString(), "HUD selection number 6");
        badSix = RequiredJson.Parse(schemaSix.ToJsonString()).AsObject();
        badSix.Required("slots").Required(0)["reserveMode"] = 3;
        AssertInvalidLegacyJsonSave(badSix.ToJsonString(), "Reserve-tank mode number 3");

        // A schema-five save lists event bits no story event uses by number, broken Chozo orbs, and item locations and
        // doors by room-data number, including numbers no retail placement has (#1553); it upgrades to the same saves,
        // without the bits nothing reads.
        JsonObject schemaFive = RequiredJson.Parse(json).AsObject();
        schemaFive["schemaVersion"] = 5;
        ToSchemaFiveProgression(schemaFive);
        JsonObject fiveProgression = schemaFive.Required("slots").Required(0).Required("progression").AsObject();
        JsonArray fiveEvents = fiveProgression.Required("events").AsArray();
        fiveEvents.Add("0x3F");
        fiveProgression["brokenChozoOrbs"] = new JsonArray(17, 91);
        JsonArray fiveItems = fiveProgression.Required("collectedItems").AsArray();
        fiveItems.Add(511);
        JsonArray fiveDoors = fiveProgression.Required("openedDoors").AsArray();
        fiveDoors.Add(511);
        GameSaveJsonDocument upgradedFive = LegacyGameSaveJson.Deserialize(schemaFive.ToJsonString(), "game save", out bool migratedFive);
        AssertTrue(migratedFive, "a schema-five save is upgraded");
        AssertSameBytes(SaveRamCodec.Encode(GameSaveJsonCodec.ToStore(upgradedFive, RetailPresentationFixture()), RetailPresentationFixture()),
            SaveRamCodec.Encode(ram, RetailPresentationFixture()),
            "a schema-five save upgrades to the same saves, keeping every named event, item location and door");

        // A schema-four save lists the Zebetite count as the three counter events and names the Shitroid-approach event
        // after its number (#1527); it upgrades to the count, the event's name and the same saves.
        JsonObject schemaFour = RequiredJson.Parse(json).AsObject();
        schemaFour["schemaVersion"] = 4;
        ToSchemaFourProgression(schemaFour);
        GameSaveJsonDocument upgradedFour = LegacyGameSaveJson.Deserialize(schemaFour.ToJsonString(), "game save", out bool migratedFour);
        AssertTrue(migratedFour, "a schema-four save is upgraded");
        SaveSlotStore upgradedFourSaves = GameSaveJsonCodec.ToStore(upgradedFour, RetailPresentationFixture());
        SuperMetroidSaveSlot upgradedFourSlot = upgradedFourSaves.ReadSlot(0) ??
            throw new InvalidOperationException("The upgraded schema-four save lost slot 0.");
        AssertEqual(3, upgradedFourSlot.Progression.ZebetitesDestroyed, "schema four's counter events become the Zebetite count");
        AssertTrue(upgradedFourSlot.Progression.Events.Members.SequenceEqual([StoryEvent.ShitroidEncounterMusicSkipped]),
            "schema four's Unused14 becomes ShitroidEncounterMusicSkipped and its counter events leave the story events");
        AssertSameBytes(SaveRamCodec.Encode(upgradedFourSaves, RetailPresentationFixture()), SaveRamCodec.Encode(ram, RetailPresentationFixture()),
            "a schema-four save upgrades to the same saves");
        // SpaceTime can leave the counter past four; the Zebetites treat every such value as all four destroyed.
        JsonObject overfullFour = RequiredJson.Parse(schemaFour.ToJsonString()).AsObject();
        overfullFour.Required("slots").Required(0).Required("progression")["events"] =
            new JsonArray("ZebetiteDestroyedBit0", "ZebetiteDestroyedBit1", "ZebetiteDestroyedBit2");
        GameSaveJsonDocument upgradedOverfull = LegacyGameSaveJson.Deserialize(overfullFour.ToJsonString(), "game save", out _);
        AssertEqual(PersistentProgress.ZebetiteCount,
            (GameSaveJsonCodec.ToStore(upgradedOverfull, RetailPresentationFixture()).ReadSlot(0) ??
                throw new InvalidOperationException("The upgraded overfull save lost slot 0.")).Progression.ZebetitesDestroyed,
            "a schema-four counter of seven becomes all four Zebetites destroyed");

        // Build a genuine old envelope independently: old named fields override its raw mirror.
        JsonObject legacy = RequiredJson.Parse(json).AsObject();
        legacy["schemaVersion"] = 1;
        ToSchemaFourProgression(legacy);
        ToSchemaTwoProgression(legacy);
        legacy.Remove("gameCompleted");
        JsonObject slot = legacy.Required("slots").Required(0).AsObject();
        slot.Remove("japaneseText"); slot.Remove("loadedItemCount");
        slot.Required("resources").AsObject().Remove("reserveMissiles");
        slot.Required("resources")["health"] = 42; // Deliberately differs from native image health 99.
        byte[] native = SaveRamCodec.Encode(ram, RetailPresentationFixture());
        native[0x1a00] = 0xa5; // Unused SRAM, outside all slots and metadata.
        // The image's item set-up count, which schema one read from the image, is not carried (#1691).
        byte[] countedPayload = SaveRamCodec.EncodePayload(SaveRamCodec.ToSnapshot(ram.ReadSlot(0) ??
            throw new InvalidOperationException("Verification requires slot 0.")), RetailPresentationFixture());
        countedPayload[SaveRamLayout.LoadedItemCountOffset] = 0x21;
        countedPayload[SaveRamLayout.LoadedItemCountOffset + 1] = 0x43;
        SaveRamCodec.WriteSlot(native, 0, countedPayload);
        var pages = new JsonArray();
        for (int offset = 0; offset < native.Length; offset += 256)
            pages.Add(new JsonObject { ["offset"] = $"0x{offset:X4}", ["bytes"] = Convert.ToHexString(native.AsSpan(offset, 256)) });
        legacy["preservedUntranslatedSram"] = pages;
        string oldJson = legacy.ToJsonString();
        var upgraded = LegacyGameSaveJson.Deserialize(oldJson, "game save", out _);
        AssertEqual(GameSaveJsonFormat.SchemaVersion, upgraded.SchemaVersion, "schema-one import becomes the current schema");
        SaveSlotStore importedSaves = GameSaveJsonCodec.ToStore(upgraded, RetailPresentationFixture());
        var imported = importedSaves.ReadSlot(0);
        AssertEqual(42, (imported ?? throw new InvalidOperationException("Verification requires imported.")).Health, "legacy named health overrides stale raw image");
        AssertEqual(27, imported.ReserveMissiles, "legacy importer decodes reserve missile word");
        AssertEqual(SaveRamLayout.LoadedItemCountSavedWord,
            SaveRamCodec.ReadSlotWord(SaveRamCodec.Encode(importedSaves, RetailPresentationFixture()), 0, SaveRamLayout.LoadedItemCountOffset),
            "legacy importer does not carry the image's item set-up count");
        AssertTrue(imported.JapaneseText && importedSaves.HasCompletedGame, "legacy importer decodes language and completion");
        AssertEqual(0, SaveRamCodec.Encode(importedSaves, RetailPresentationFixture())[0x1a00], "confirmed unused bytes are not perpetuated");
        AssertTrue(!GameSaveJsonCodec.Serialize(upgraded).Contains("preservedUntranslatedSram", StringComparison.Ordinal), "legacy import never re-emits its blob");
        var bad = RequiredJson.Parse(oldJson).AsObject();
        bad.Required("preservedUntranslatedSram").Required(0)["bytes"] = "not hex";
        AssertInvalidLegacyJsonSave(bad.ToJsonString(), "not hexadecimal");
        bad = RequiredJson.Parse(oldJson).AsObject();
        bad.Required("preservedUntranslatedSram").AsArray().RemoveAt(0);
        AssertInvalidLegacyJsonSave(bad.ToJsonString(), "32 pages");
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

    /// <summary>
    /// Rewrites each slot's current progression as schema four wrote it: the Zebetite count as the bits of the counter
    /// events <c>ZebetiteDestroyedBit0</c> through <c>ZebetiteDestroyedBit2</c> (the game's events three through five),
    /// listed in event order, and <see cref="StoryEvent.ShitroidEncounterMusicSkipped"/> under its schema-four name <c>Unused14</c>.
    /// </summary>
    private static void ToSchemaFourProgression(JsonObject root)
    {
        ToSchemaFiveProgression(root);
        foreach (JsonNode? slotNode in root.Required("slots").AsArray())
        {
            if (slotNode is null) continue;
            JsonObject progression = slotNode.Required("progression").AsObject();
            int count = progression.Required("zebetitesDestroyed").GetValue<int>();
            progression.Remove("zebetitesDestroyed");
            var byNumber = new SortedDictionary<int, string>();
            foreach (JsonNode? entry in progression.Required("events").AsArray())
            {
                StoryEvent named = Enum.Parse<StoryEvent>(entry?.GetValue<string>() ?? "", ignoreCase: true);
                byNumber.Add(named.Number(), named == StoryEvent.ShitroidEncounterMusicSkipped ? "Unused14" : named.ToString());
            }
            for (int bit = 0; bit < 3; bit++)
            {
                if ((count & (1 << bit)) != 0)
                    byNumber.Add(3 + bit, $"ZebetiteDestroyedBit{bit}");
            }
            var events = new JsonArray();
            foreach (string name in byNumber.Values)
                events.Add(name);
            progression["events"] = events;
        }
    }

    /// <summary>
    /// Rewrites each slot as schema eight wrote it (#1723): the checkpoint as its area and its number in the area's station
    /// list, and the used stations as one entry per area in area order, each listing its stations' numbers ascending.
    /// </summary>
    internal static void ToSchemaEightSlots(JsonObject root)
    {
        foreach (JsonNode? slotNode in root.Required("slots").AsArray())
        {
            if (slotNode is null) continue;
            JsonObject slot = slotNode.AsObject();
            string checkpointName = slot.Required("checkpoint").GetValue<string>();
            LoadStation checkpoint = LoadStationDefinitions.All.Single(station => station.Name == checkpointName);
            slot["checkpoint"] = new JsonObject
            {
                ["area"] = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(checkpoint.Area.ToString()),
                ["saveStation"] = LoadStationNumberCodec.Number(checkpoint),
            };
            JsonObject progression = slot.Required("progression").AsObject();
            LoadStation[] used = [.. progression.Required("usedStations").AsArray().Select(entry =>
                LoadStationDefinitions.Usable.Single(station => station.Name == entry?.GetValue<string>()))];
            var areas = new JsonArray();
            foreach (AreaId area in AreaIds.All)
            {
                var numbers = new JsonArray();
                foreach (int number in used.Where(station => station.Area == area).Select(LoadStationNumberCodec.Number).Order())
                    numbers.Add(number);
                areas.Add(new JsonObject
                {
                    ["area"] = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(area.ToString()),
                    ["stations"] = numbers,
                });
            }
            progression["usedStations"] = areas;
        }
    }

    /// <summary>
    /// Rewrites each slot as schema eight wrote it (<see cref="ToSchemaEightSlots"/>), then as schema seven wrote it
    /// (#1691): with the count of item set-ups, here the largest a save field holds.
    /// </summary>
    private static void ToSchemaSevenSlots(JsonObject root)
    {
        ToSchemaEightSlots(root);
        foreach (JsonNode? slotNode in root.Required("slots").AsArray())
        {
            if (slotNode is null) continue;
            slotNode.AsObject()["loadedItemCount"] = 65535;
        }
    }

    /// <summary>
    /// Rewrites each slot as schema seven wrote it (<see cref="ToSchemaSevenSlots"/>), then as schema six wrote it (#1605): the HUD selection as its place in the selection ring, the
    /// reserve-tank mode as none, auto or manual counted from zero, and the two words a new file sets, holding the one
    /// a new file stores.
    /// </summary>
    private static void ToSchemaSixSlots(JsonObject root)
    {
        ToSchemaSevenSlots(root);
        foreach (JsonNode? slotNode in root.Required("slots").AsArray())
        {
            if (slotNode is null) continue;
            JsonObject slot = slotNode.AsObject();
            slot["hudItem"] = Enum.Parse<HudItem>(slot.Required("hudItem").GetValue<string>(), ignoreCase: true).SavedNumber();
            slot["reserveMode"] = Enum.Parse<ReserveTankMode>(slot.Required("reserveMode").GetValue<string>(), ignoreCase: true).SavedNumber();
            slot["cartridgeDebugFlag"] = 1;
            slot["newFileMarker"] = 1;
        }
    }

    /// <summary>
    /// Rewrites each slot as schema six wrote it (<see cref="ToSchemaSixSlots"/>), then its progression as schema five
    /// wrote it (#1553): the item locations and doors by the numbers room data gives them, and an empty list of broken
    /// Chozo orbs.
    /// </summary>
    private static void ToSchemaFiveProgression(JsonObject root)
    {
        ToSchemaSixSlots(root);
        foreach (JsonNode? slotNode in root.Required("slots").AsArray())
        {
            if (slotNode is null) continue;
            JsonObject progression = slotNode.Required("progression").AsObject();
            var items = new JsonArray();
            foreach (JsonNode? entry in progression.Required("collectedItems").AsArray())
            {
                string name = entry?.GetValue<string>() ?? "";
                items.Add(SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(ItemLocationDefinitions.All.Single(location => location.Name == name)));
            }
            var doors = new JsonArray();
            foreach (JsonNode? entry in progression.Required("openedDoors").AsArray())
            {
                string name = entry?.GetValue<string>() ?? "";
                doors.Add(SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.Number(LockedDoorDefinitions.All.Single(door => door.Name == name)));
            }
            progression["brokenChozoOrbs"] = new JsonArray();
            progression["collectedItems"] = items;
            progression["openedDoors"] = doors;
        }
    }

    /// <summary>
    /// Rewrites each slot's current progression lists as schema two wrote them: bosses for eight area slots, used stations
    /// as bit positions of the sixteen-byte station table, acquired maps as every bit of each area's map-station byte, and
    /// the orb, item and door lists under their schema-two names.
    /// </summary>
    private static void ToSchemaTwoProgression(JsonObject root)
    {
        foreach (JsonNode? slotNode in root.Required("slots").AsArray())
        {
            if (slotNode is null) continue;
            JsonObject progression = slotNode.Required("progression").AsObject();
            var bosses = new JsonArray();
            foreach (JsonNode? entry in progression.Required("defeatedBosses").AsArray())
                bosses.Add(entry?.Required("bosses").DeepClone());
            bosses.Add("none");
            var stations = new JsonArray();
            foreach (JsonNode? entry in progression.Required("usedStations").AsArray())
            {
                int area = AreaNumberCodec.Encode(Enum.Parse<AreaId>(entry?.Required("area").GetValue<string>() ?? "", ignoreCase: true));
                // Schema two gave each area sixteen used-station bits, one per number its station list can give a station.
                foreach (JsonNode? station in entry?.Required("stations").AsArray() ?? [])
                    stations.Add(area * 16 + (station?.GetValue<int>() ?? 0));
            }
            var maps = new JsonArray();
            foreach (JsonNode? entry in progression.Required("acquiredMaps").AsArray())
            {
                int area = AreaNumberCodec.Encode(Enum.Parse<AreaId>(entry?.GetValue<string>() ?? "", ignoreCase: true));
                for (int bit = 0; bit < 8; bit++)
                    maps.Add(area * 8 + bit);
            }
            progression["roomChozoBits"] = progression.Required("brokenChozoOrbs").DeepClone();
            progression["collectedItemBits"] = progression.Required("collectedItems").DeepClone();
            progression["openedDoorBits"] = progression.Required("openedDoors").DeepClone();
            foreach (string name in new[] { "defeatedBosses", "usedStations", "acquiredMaps", "brokenChozoOrbs", "collectedItems", "openedDoors" })
                progression.Remove(name);
            progression["bossFlagsByAreaIndex"] = bosses;
            progression["usedSaveStationBits"] = stations;
            progression["acquiredMapStationBits"] = maps;
        }
    }

    /// <summary>Confirms the translated completion field is consumed by the live frontend.</summary>
    private static void VerifyNamedSaveRuntime(SaveSlotStore ram)
    {
        // Confirm the translated completion bit is consumed and produced by the frontend.
        var frontend = new SuperMetroidGame(ram);
        frontend.BindArtwork(frontend.Artwork with
        {
            MapPresentation = RetailPresentationFixture(),
        });
        // Boot fixes the demo playlist, so each marker state boots a session.
        ram.SelectSlot(2);
        ram.SetGameCompleted(false);
        AssertEqual(StockAttractDemoScenes.OrdinaryPlaylist, new SuperMetroidGame(ram).Session.DemoPlaylist, "incomplete save plays the ordinary demo sets");
        ram.SetGameCompleted(true);
        AssertEqual(StockAttractDemoScenes.CompletedGamePlaylist, new SuperMetroidGame(ram).Session.DemoPlaylist, "completed save unlocks the completion demo set");
        AssertEqual(2, ram.ReadSelectedSlot(), "twelve-byte completion signature preserves selected slot");
        ram.SetGameCompleted(false);
        var native = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var text = SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingTextPresentation.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Frontend.Ending.EndingTextExtractor.Extract(native), writable: false));
        var ending = new EndingCreditsState(SuperMetroid.Core.Audio.AudioQueues.Standalone(), new EndingCreditsSetup { GameTimeHours = 0, GameTimeMinutes = 0, EndingText = text.Typewriter() });
        ending.InstalledArtwork.BindBackdrops(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingBackdrops);
        // The game binds the installed sheets with the ending, which draws its stars from them.
        ending.InstalledArtwork.BindSpriteSheets(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SpriteSheets);
        ending.Stage.Phase = EndingCreditsPhase.ItemPercentageScrollDown;
        ending.Stage.PhaseTimer = 1;
        frontend.Session.Show(ending);
        frontend.GameState = SuperMetroidGameState.EndingAndCredits;
        int writes = 0;
        frontend.SavesChanged += () => writes++;
        frontend.Step(0);
        AssertTrue(ram.HasCompletedGame, "ending final-message transition persists completion");
        AssertEqual(1, writes, "ending requests durable save exactly on completion transition");
        frontend.Step(0);
        AssertEqual(1, writes, "final-message idle does not repeatedly save");

        ram.SelectSlot(0);
    }

    /// <summary>Confirms a malformed schema-one save is rejected by the hosts' upgrade with a diagnostic naming the invalid value.</summary>
    private static void AssertInvalidLegacyJsonSave(string json, string expectedMessagePart)
    {
        try
        {
            _ = LegacyGameSaveJson.Deserialize(json, "invalid legacy JSON save fixture", out _);
            throw new InvalidOperationException($"Invalid legacy JSON save containing '{expectedMessagePart}' was accepted.");
        }
        catch (InvalidDataException exception)
        {
            AssertTrue(exception.Message.Contains(expectedMessagePart, StringComparison.OrdinalIgnoreCase),
                $"invalid legacy JSON save identifies '{expectedMessagePart}'");
        }
    }
}
