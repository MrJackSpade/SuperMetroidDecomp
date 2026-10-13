using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SaveSchemaTranslationVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static RetailPresentationFixtureVerification;

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
        SuperMetroidAddressSpace source = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var saveRam = new SuperMetroidSaveRam(source, RetailPresentationFixture());
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
            ReserveTankMode = 1,
            SelectedHudItem = 2,
            CollectedItems = (ushort)(
                SamusEquipmentFlags.MorphBall |
                SamusEquipmentFlags.Bombs |
                SamusEquipmentFlags.VariaSuit),
            EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.VariaSuit),
            CollectedBeams = (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Wave),
            EquippedBeams = (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Wave),
        };
        var system = new Bank80SystemState();
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
            physicalButton: (ushort)SnesButton.Y);
        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(samus, system, new SaveSnapshotContext { Area = (ushort)AreaId.Brinstar, SaveStation = 4, GameTime = time, ControllerBindings = bindings, MoonwalkEnabled = true, IconCancelEnabled = true });
        saveRam.SaveSlot(1, snapshot);

        // The former preservation-only word is the native reserve-missile accumulator.
        snapshot = snapshot with { ReserveMissiles = 165, JapaneseText = true, LoadedItemCount = 321 };
        saveRam.SaveSlot(1, snapshot);
        saveRam.SetGameCompleted(true);
        saveRam.SelectSlot(1);
        byte[] expectedSram = source.SaveRam.ToArray();

        GameSaveJsonDocument document = GameSaveJsonCodec.Capture(source, RetailPresentationFixture());
        string json = GameSaveJsonCodec.Serialize(document);
        AssertTrue(json.Contains("\"checkpoint\"", StringComparison.Ordinal) &&
                   json.Contains("\"zebesAwake\"", StringComparison.OrdinalIgnoreCase) &&
                   json.Contains("\"exploredAreas\"", StringComparison.Ordinal),
            "JSON save exposes named checkpoint, event, and explored-map domains");
        AssertEqual(json, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Deserialize(json)),
            "JSON save formatting and property order are deterministic");

        SuperMetroidAddressSpace restored = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(json), restored, RetailPresentationFixture());
        AssertTrue(expectedSram.SequenceEqual(restored.SaveRam),
            "JSON save round trip rebuilds canonical SRAM from named state");
        SuperMetroidSaveSlot restoredSlot = new SuperMetroidSaveRam(restored, RetailPresentationFixture()).ReadSlot(1) ??
            throw new InvalidOperationException("JSON round trip lost slot 1.");
        AssertEqual((ushort)AreaId.Brinstar, restoredSlot.Area, "JSON checkpoint area");
        AssertEqual(4, restoredSlot.SaveStation, "JSON checkpoint station");
        AssertEqual(94, restoredSlot.Health, "JSON health");
        AssertEqual(15, restoredSlot.MaxMissiles, "JSON missile capacity");
        AssertTrue(restoredSlot.CollectedItems.HasAll(
            SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.VariaSuit),
            "JSON collected equipment flags");
        AssertTrue(restoredSlot.BossBytes.SequenceEqual(snapshot.BossBytes), "JSON boss flags");
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
            SuperMetroidAddressSpace migrated = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
            GameSaveLoadResult firstLoad = GameSaveFileStore.LoadOrMigrate(
                migrated,
                jsonPath,
                legacyPath, RetailPresentationFixture());
            AssertTrue(firstLoad.MigratedLegacySram && File.Exists(jsonPath),
                "legacy SRAM migrates to JSON on first load");
            AssertTrue(expectedSram.SequenceEqual(File.ReadAllBytes(legacyPath)),
                "legacy SRAM remains unchanged after migration");
            AssertTrue(expectedSram.SequenceEqual(migrated.SaveRam),
                "legacy migration retains the exact SRAM image");

            SuperMetroidAddressSpace jsonReload = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
            GameSaveLoadResult secondLoad = GameSaveFileStore.LoadOrMigrate(
                jsonReload,
                jsonPath,
                legacyPath, RetailPresentationFixture());
            AssertTrue(!secondLoad.MigratedLegacySram && expectedSram.SequenceEqual(jsonReload.SaveRam),
                "subsequent load prefers JSON and restores identical state");
            GameSaveFileStore.WriteAtomic(jsonReload, jsonPath, RetailPresentationFixture());
            AssertTrue(File.Exists(jsonPath + ".bak"),
                "atomic JSON replacement retains the previous complete save");
            AssertEqual(0, Directory.GetFiles(temporaryDirectory, "*.tmp").Length,
                "atomic JSON replacement leaves no partial file");

            File.Delete(jsonPath);
            File.WriteAllBytes(legacyPath, new byte[17]);
            try
            {
                _ = GameSaveFileStore.LoadOrMigrate(jsonReload, jsonPath, legacyPath, RetailPresentationFixture());
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
        AreaMapPresentationCatalog presentation = RetailPresentationFixture();
        const ushort AllBits = ushort.MaxValue;
        const ushort SignBit = 1 << 15;
        (ushort Slot, ushort Complement, int Expected)[] cases =
        [
            (0, AllBits, 0), // canonical slot A
            (1, AllBits ^ 1, 1), // canonical slot B
            (2, AllBits ^ 2, 2), // canonical slot C
            (1, 0, 1), // issue #1340: a zero complement is accepted
            (2, 0, 2),
            (1, 2, 1), // no bits shared with the slot
            (2, SignBit | 1, 2),
            (0, 0, 0),
            (1, 1, 0), // a shared bit rejects
            (2, AllBits, 0),
            (3, 0, 0), // CMP #$0003 / BPL rejects
            (SignBit - 1, 0, 0),
            (SignBit | 1, 0, 0), // CMP #$0000 / BMI rejects negative words
            (AllBits, 0, 0),
        ];
        // Every consumer only reads the selected-slot pair, so one imported image is
        // rewritten per case.
        SuperMetroidAddressSpace addressSpace =
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach ((ushort slot, ushort complement, int expected) in cases)
        {
            string label = $"selected slot ${slot:X4} complement ${complement:X4}";
            BinaryPrimitives.WriteUInt16LittleEndian(
                addressSpace.SaveRam[SaveRamLayout.SelectedSlotOffset..], slot);
            BinaryPrimitives.WriteUInt16LittleEndian(
                addressSpace.SaveRam[SaveRamLayout.SelectedSlotComplementOffset..], complement);

            AssertEqual(expected, new SuperMetroidSaveRam(addressSpace, presentation).ReadSelectedSlot(),
                $"{label}: SRAM decode matches the cartridge");
            AssertEqual(expected, new FileSelectMenuState(addressSpace, mapPresentation: presentation).SelectedItem,
                $"{label}: file-select initial cursor matches $81:9ED8");
            AssertEqual(expected, GameSaveJsonCodec.Capture(addressSpace, presentation).SelectedSlot,
                $"{label}: JSON capture keeps the cartridge-selected slot");
        }

        Console.WriteLine("  Selected save slot: imported noncanonical SRAM decodes with the cartridge's range and complement rule.");
    }

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

    [VerificationCommand("--game-save-json")]
    internal static int GameSaveJsonCommand()
    {
        Suite(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence());
        Suite(nameof(VerifySaveSchemaTranslation), () => VerifySaveSchemaTranslation());
        Suite(nameof(VerifySelectedSaveSlotDecode), () => VerifySelectedSaveSlotDecode());
        return 0;
    }

    [VerificationCommand("--selected-save-slot-decode")]
    internal static int SelectedSaveSlotDecodeCommand()
    {
        Suite(nameof(VerifySelectedSaveSlotDecode), () => VerifySelectedSaveSlotDecode());
        return 0;
    }

    [VerificationCommand("--save-json-persistence")]
    internal static int SaveJsonPersistenceCommand()
    {
        Suite(nameof(VerifyGameSaveJsonPersistence), () => VerifyGameSaveJsonPersistence());
        return 0;
    }
}
