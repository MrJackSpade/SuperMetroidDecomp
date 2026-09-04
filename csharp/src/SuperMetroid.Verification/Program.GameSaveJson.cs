using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// Exercises the named JSON schema, lossless native preservation, strict parsing,
    /// atomic replacement, and one-time legacy migration against the retail map packer.
    /// </summary>
    private static void VerifyGameSaveJsonPersistence()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
            throw new FileNotFoundException("JSON save verification requires the private retail ROM.", romPath);
        SuperMetroidAddressSpace source = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var saveRam = new SuperMetroidSaveRam(source);
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
        system.MarkExploredMapTile(AreaId.Brinstar, 9, 1);
        system.MarkExploredMapTile(AreaId.Brinstar, 23, 2);
        var time = new GameTimeState();
        time.Load(frames: 41, seconds: 32, minutes: 21, hours: 7);
        ControllerBindings bindings = ControllerBindings.Default.AssignAndSwap(
            action: 0,
            physicalButton: (ushort)SnesButton.Y);
        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(
            samus,
            system,
            area: (ushort)AreaId.Brinstar,
            saveStation: 4,
            time,
            bindings,
            moonwalkEnabled: true,
            iconCancelEnabled: true);
        saveRam.SaveSlot(1, snapshot);

        // This gap is intentionally not represented by the translated snapshot. Preserve a
        // sentinel there and rebuild the checksum so migration proves unknown bytes survive.
        int untranslatedOffset = SaveRamLayout.SlotOffsets[1] + SaveRamLayout.ReserveEnergyOffset + 2;
        source.WriteByte((int)new SnesAddress(SaveRamLayout.SramBank, (ushort)untranslatedOffset), 0xa5);
        saveRam.SaveSlotPreservingUntranslatedBytes(1, snapshot);
        saveRam.SelectSlot(1);
        byte[] expectedSram = source.SaveRam.ToArray();

        GameSaveJsonDocument document = GameSaveJsonCodec.Capture(source);
        string json = GameSaveJsonCodec.Serialize(document);
        AssertTrue(json.Contains("\"checkpoint\"", StringComparison.Ordinal) &&
                   json.Contains("\"zebesAwake\"", StringComparison.OrdinalIgnoreCase) &&
                   json.Contains("\"exploredAreas\"", StringComparison.Ordinal),
            "JSON save exposes named checkpoint, event, and explored-map domains");
        AssertEqual(json, GameSaveJsonCodec.Serialize(GameSaveJsonCodec.Deserialize(json)),
            "JSON save formatting and property order are deterministic");

        SuperMetroidAddressSpace restored = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        GameSaveJsonCodec.Apply(GameSaveJsonCodec.Deserialize(json), restored);
        AssertTrue(expectedSram.SequenceEqual(restored.SaveRam),
            "JSON save round trip preserves complete translated and untranslated SRAM");
        SuperMetroidSaveSlot restoredSlot = new SuperMetroidSaveRam(restored).ReadSlot(1) ??
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
            json.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 99", StringComparison.Ordinal),
            "schemaVersion");
        AssertInvalidJsonSave(
            json.Replace("\"selectedSlot\": 1,", "\"selectedSlot\": 1,\n  \"unknownField\": true,",
                StringComparison.Ordinal),
            "unknownField");
        AssertInvalidJsonSave(
            json.Replace("\"health\": 94", "\"health\": 999", StringComparison.Ordinal),
            "health");

        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "super-metroid-json-save-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            string legacyPath = Path.Combine(temporaryDirectory, "game.srm");
            string jsonPath = Path.Combine(temporaryDirectory, "game.save.json");
            File.WriteAllBytes(legacyPath, expectedSram);
            SuperMetroidAddressSpace migrated = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            GameSaveLoadResult firstLoad = GameSaveFileStore.LoadOrMigrate(
                migrated,
                jsonPath,
                legacyPath);
            AssertTrue(firstLoad.MigratedLegacySram && File.Exists(jsonPath),
                "legacy SRAM migrates to JSON on first load");
            AssertTrue(expectedSram.SequenceEqual(File.ReadAllBytes(legacyPath)),
                "legacy SRAM remains unchanged after migration");
            AssertTrue(expectedSram.SequenceEqual(migrated.SaveRam),
                "legacy migration retains the exact SRAM image");

            SuperMetroidAddressSpace jsonReload = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            GameSaveLoadResult secondLoad = GameSaveFileStore.LoadOrMigrate(
                jsonReload,
                jsonPath,
                legacyPath);
            AssertTrue(!secondLoad.MigratedLegacySram && expectedSram.SequenceEqual(jsonReload.SaveRam),
                "subsequent load prefers JSON and restores identical state");
            GameSaveFileStore.WriteAtomic(jsonReload, jsonPath);
            AssertTrue(File.Exists(jsonPath + ".bak"),
                "atomic JSON replacement retains the previous complete save");
            AssertEqual(0, Directory.GetFiles(temporaryDirectory, "*.tmp").Length,
                "atomic JSON replacement leaves no partial file");

            File.Delete(jsonPath);
            File.WriteAllBytes(legacyPath, new byte[17]);
            try
            {
                _ = GameSaveFileStore.LoadOrMigrate(jsonReload, jsonPath, legacyPath);
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
            "  JSON saves: named schema, all slot domains, native preservation, strict errors, atomic writes, and legacy migration agree.");
    }

    private static void AssertInvalidJsonSave(string json, string expectedMessagePart)
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
}
