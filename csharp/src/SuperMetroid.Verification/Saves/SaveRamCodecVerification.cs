using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Saves;

/// <summary>
/// Checks <see cref="SaveRamCodec"/> and <see cref="SaveSlotStore"/> (#1322): decoding applies the cartridge's validity,
/// selection and boot-carry rules to an image as given, and every store operation encodes to an image that decodes back
/// to the same store.
/// </summary>
internal static class SaveRamCodecVerification
{
    /// <summary>
    /// An image with one invalid slot and a noncanonical completion marker decodes to an empty slot, the other slots and
    /// selection as written, and the boot carry of the raw marker words; a later save keeps that carry, as SRAM keeps the
    /// marker words.
    /// </summary>
    internal static void VerifyInvalidSlotAndNoncanonicalMarker()
    {
        IAreaMapSource maps = RetailPresentationFixture();
        var written = new SaveSlotStore(maps);
        written.SaveSlot(0, Snapshot(health: 61, area: AreaId.Brinstar));
        written.SaveSlot(1, Snapshot(health: 77, area: AreaId.Norfair));
        written.SelectSlot(1);
        byte[] image = SaveRamCodec.Encode(written, maps);
        // Damage slot 1's payload under both directories, then mark the image with a non-signature marker.
        image[SaveRamLayout.SlotOffset(1) + SaveRamLayout.HealthOffset] ^= 16;

        foreach ((byte[] marker, bool expectedCarry) in new VerifyInvalidSlotAndNoncanonicalMarkerCase[]
                 {
                     // Last word "ie" ($6569) is above "id" ($6469): the first mismatch leaves carry set.
                     new VerifyInvalidSlotAndNoncanonicalMarkerCase("supermetroie"u8.ToArray(), true),
                     // Last word "ic" ($6369) is below "id": carry clear.
                     new VerifyInvalidSlotAndNoncanonicalMarkerCase("supermetroic"u8.ToArray(), false),
                 })
        {
            marker.CopyTo(image, SaveRamLayout.CompletionMarkerOffset);
            SaveSlotStore decoded = SaveRamCodec.Decode(image, maps);

            AssertTrue(decoded.ReadSlot(1) is null && !decoded.IsSlotValid(1), "a slot failing both checksum pairs decodes empty");
            AssertTrue(PayloadOf(decoded.ReadSlot(0), maps).SequenceEqual(PayloadOf(written.ReadSlot(0), maps)),
                "a valid slot decodes to the record that was written");
            AssertEqual(1, decoded.ReadSelectedSlot(), "the selection decodes as written, whatever the selected slot holds");
            AssertEqual(expectedCarry, AddsRumbleFrame(decoded), "the decoded boot carry compares the raw marker words");
            AssertTrue(!decoded.HasCompletedGame, "a noncanonical marker is not the completed-game signature");
            decoded.SaveSlot(2, Snapshot(health: 33, area: AreaId.Crateria));
            AssertEqual(expectedCarry, AddsRumbleFrame(decoded),
                "a later save keeps the raw marker comparison for the next boot");
        }
        Console.WriteLine("SRAM decode: an invalid slot decodes empty; noncanonical markers keep their raw boot carry across saves.");
    }

    /// <summary>Each store operation has the cartridge's effect and encodes to an image that decodes back to the same store.</summary>
    internal static void VerifyStoreOperationsRoundTrip()
    {
        IAreaMapSource maps = RetailPresentationFixture();
        var store = new SaveSlotStore(maps);
        void RoundTrips(string step)
        {
            byte[] image = SaveRamCodec.Encode(store, maps);
            AssertTrue(SaveRamCodec.Encode(SaveRamCodec.Decode(image, maps), maps).SequenceEqual(image),
                $"the SRAM image round-trips through the codec after {step}");
        }
        string Marker() => System.Text.Encoding.ASCII.GetString(SaveRamCodec.Encode(store, maps),
            SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length);

        // Boot with every slot invalid keeps the carry set, writes the corrupt marker and fixes three demo sets.
        AssertTrue(AddsRumbleFrame(store), "an empty store's boot carry is set");
        AssertTrue(!store.DetermineCompletionDemoSetUnlocked(), "an all-invalid boot leaves the completion demo set out");
        AssertEqual("madadameyohn", Marker(), "an all-invalid boot writes the corrupt-SRAM marker");
        RoundTrips("an all-invalid boot");
        store.SaveSlot(0, Snapshot(health: 61, area: AreaId.Brinstar));
        // Last word "hn" ($6E68) is above "id" ($6469): the corrupt marker leaves the next boot's carry set.
        AssertTrue(AddsRumbleFrame(store), "boot carry over the corrupt marker");
        AssertEqual("madadameyohn", Marker(), "a save leaves the marker");
        RoundTrips("saving slot A");
        store.SaveSlot(2, Snapshot(health: 88, area: AreaId.Maridia));
        RoundTrips("saving slot C");
        store.CopySlot(0, 1);
        AssertTrue(PayloadOf(store.ReadSlot(1), maps).SequenceEqual(PayloadOf(store.ReadSlot(0), maps)), "a copied slot holds the source's save");
        AssertEqual(1, store.ReadSlot(1)?.Slot, "a copied slot reports its own index");
        RoundTrips("copying slot A to B");
        store.ClearSlot(2);
        AssertTrue(store.ReadSlot(2) is null, "a cleared slot is empty");
        RoundTrips("clearing slot C");
        store.SelectSlot(1);
        AssertEqual(1, SaveRamCodec.Decode(SaveRamCodec.Encode(store, maps), maps).ReadSelectedSlot(), "the selection encodes with its complement");
        RoundTrips("selecting slot B");
        store.SetGameCompleted(true);
        AssertEqual("supermetroid", Marker(), "completion writes the signature");
        AssertTrue(AddsRumbleFrame(store), "a completed save's boot carry is set");
        AssertTrue(store.DetermineCompletionDemoSetUnlocked(), "a completed save unlocks the completion demo set");
        RoundTrips("completing the game");
        store.SetGameCompleted(false);
        AssertTrue(!AddsRumbleFrame(store), "a valid incomplete save's boot carry is clear");
        RoundTrips("clearing completion");
        Console.WriteLine("Save store: boot, save, copy, clear, select and completion have the cartridge's effect and round-trip through SRAM.");
    }

    /// <summary>
    /// The slot fields saves now carry as <see cref="int"/> (clock, dispatcher route and checkpoint) hold every value their
    /// own domain does: the clock's latest reading survives SRAM, store and SRAM again byte for byte, a value past its
    /// domain is rejected instead of wrapping into the image, and an imported image whose clock words lie outside the
    /// clock's domain is rejected on decode (#1692). The debug build's placement-mode word, the never-read word after it
    /// and the item set-up count nothing reads always encode as the words the port's saves hold, whatever an imported
    /// image held there (#1691).
    /// </summary>
    internal static void VerifyWidenedSlotFieldsRoundTrip()
    {
        IAreaMapSource maps = RetailPresentationFixture();
        var written = new SaveSlotStore(maps);
        SuperMetroidSaveSnapshot extremes = Snapshot(health: 61, area: AreaId.Brinstar) with
        {
            StartupRoute = SaveLoadingGameState.CeresDestruction,
            GameTimeFrames = 59,
            GameTimeSeconds = 0,
            GameTimeMinutes = 59,
            GameTimeHours = 99,
        };
        written.SaveSlot(0, extremes);
        byte[] image = SaveRamCodec.Encode(written, maps);
        SaveSlotStore decoded = SaveRamCodec.Decode(image, maps);
        AssertTrue(SaveRamCodec.Encode(decoded, maps).SequenceEqual(image),
            "SRAM -> store -> SRAM keeps the full-range slot fields byte for byte");
        SuperMetroidSaveSlot slot = decoded.ReadSlot(0) ?? throw new InvalidOperationException("The full-range slot failed its checksums.");
        AssertEqual(SaveRamLayout.DebugFlagSavedWord, SaveRamCodec.ReadSlotWord(image, 0, SaveRamLayout.DebugFlagOffset),
            "a save holds the placement-mode word a new file stores");
        AssertEqual(SaveRamLayout.NewFileMarkerSavedWord, SaveRamCodec.ReadSlotWord(image, 0, SaveRamLayout.NewFileMarkerOffset),
            "a save holds the never-read word a new file stores");
        AssertEqual(SaveRamLayout.LoadedItemCountSavedWord, SaveRamCodec.ReadSlotWord(image, 0, SaveRamLayout.LoadedItemCountOffset),
            "a save holds the item set-up count a new file saved from power-on holds");
        // An imported image whose unread words hold other values re-encodes with the port's words.
        byte[] importedPayload = SaveRamCodec.EncodePayload(SaveRamCodec.ToSnapshot(slot), maps);
        importedPayload[SaveRamLayout.DebugFlagOffset] = byte.MaxValue;
        importedPayload[SaveRamLayout.DebugFlagOffset + 1] = byte.MaxValue;
        importedPayload[SaveRamLayout.NewFileMarkerOffset] = 0;
        importedPayload[SaveRamLayout.LoadedItemCountOffset] = 0x21;
        importedPayload[SaveRamLayout.LoadedItemCountOffset + 1] = 0x43;
        byte[] importedImage = (byte[])image.Clone();
        SaveRamCodec.WriteSlot(importedImage, 0, importedPayload);
        AssertTrue(SaveRamCodec.Encode(SaveRamCodec.Decode(importedImage, maps), maps).SequenceEqual(image),
            "an imported save's unread words are not carried; it re-encodes as the game would save it");
        AssertEqual(SaveLoadingGameState.CeresDestruction, slot.StartupRoute, "the startup route round-trips through its SRAM word");
        AssertEqual(SaveLoadingGameState.MainGame, SuperMetroid.AssetExtraction.Progression.SaveLoadingGameStateCodec.Route(SaveFieldValue.MaximumValue),
            "a dispatcher word the cartridge does not name resumes the main game");
        AssertTrue(ReferenceEquals(SuperMetroid.Core.Progression.LoadStationDefinitions.GreenBrinstarSave, slot.Checkpoint), "the checkpoint decodes to the station it was saved at");
        AssertEqual(1, SaveRamCodec.ReadSlotWord(image, 0, SaveRamLayout.AreaOffset), "the checkpoint's area word holds Brinstar's number");
        AssertEqual(1, SaveRamCodec.ReadSlotWord(image, 0, SaveRamLayout.SaveStationOffset), "the checkpoint's station word holds its number in Brinstar's list");
        AssertEqual(99, slot.GameTimeHours, "the clock's hours decode their largest reading");
        AssertEqual(59, slot.GameTimeMinutes, "the clock's minutes decode their largest reading");
        AssertEqual(59, slot.GameTimeFrames, "the clock's frames decode their largest reading");
        AssertEqual(0, slot.GameTimeSeconds, "the clock's seconds decode zero");
        int[] clockOffsets =
        [
            SaveRamLayout.GameTimeFramesOffset, SaveRamLayout.GameTimeSecondsOffset,
            SaveRamLayout.GameTimeMinutesOffset, SaveRamLayout.GameTimeHoursOffset,
        ];
        foreach (int offset in clockOffsets)
        {
            int firstOutside = offset == SaveRamLayout.GameTimeHoursOffset ? 100 : 60;
            foreach (int word in new[] { firstOutside, ushort.MaxValue })
            {
                byte[] clockPayload = SaveRamCodec.EncodePayload(SaveRamCodec.ToSnapshot(slot), maps);
                clockPayload[offset] = (byte)word;
                clockPayload[offset + 1] = (byte)(word >> 8);
                byte[] clockImage = (byte[])image.Clone();
                SaveRamCodec.WriteSlot(clockImage, 0, clockPayload);
                AssertThrows<InvalidDataException>(() => SaveRamCodec.Decode(clockImage, maps),
                    $"an imported clock word {word} at slot offset {offset} outside the clock's domain is rejected");
            }
        }

        foreach ((string field, SuperMetroidSaveSnapshot outOfRange) in new VerifyWidenedSlotFieldsRoundTripCase[]
                 {
                     new VerifyWidenedSlotFieldsRoundTripCase("clock minutes", extremes with { GameTimeMinutes = -1 }),
                     new VerifyWidenedSlotFieldsRoundTripCase("clock frames", extremes with { GameTimeFrames = 60 }),
                     new VerifyWidenedSlotFieldsRoundTripCase("clock hours", extremes with { GameTimeHours = 100 }),
                 })
        {
            AssertThrows<InvalidOperationException>(() => written.SaveSlot(1, outOfRange),
                $"a {field} past its domain is rejected instead of wrapping");
        }
        AssertTrue(written.ReadSlot(1) is null, "a rejected save leaves its slot unchanged");
        Console.WriteLine("Save slot fields: full-domain values round-trip through SRAM exactly; out-of-domain values are rejected.");
    }

    /// <summary>
    /// The progression owner's saved fields: the saved startup route accepts only a defined route, and an area's boss state holds the named flags a save image's boss byte stores; the bits no routine
    /// tests are not progress (#1475).
    /// </summary>
    internal static void VerifyProgressionSavedFields()
    {
        var system = new SharedGameState(new SharedRandom());
        AssertThrows<ArgumentOutOfRangeException>(() => system.LoadSavedStartupRoute((SaveLoadingGameState)(-1)),
            "an undefined startup route is rejected");

        // A save image's boss byte with a bit no routine tests beside two named ones: only the named flags are progress,
        // and typed operations change only the named flags; writing the table back leaves the untested bit clear.
        byte[] areas = new byte[ProgressionTableCodec.AreaBossByteCount];
        areas[SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Norfair)] = 0x85;
        ProgressionTableCodec.ReadAreaBosses(areas, system.Progress);
        AssertEqual(DefeatedBosses.AreaBoss | DefeatedBosses.AreaTorizo, system.BossesDefeatedIn(AreaId.Norfair), "a loaded boss byte keeps its named flags");
        system.RecordBossesDefeated(AreaId.Norfair, DefeatedBosses.AreaMiniBoss);
        system.ForgetBossesDefeated(AreaId.Norfair, DefeatedBosses.AreaBoss);
        AssertEqual(DefeatedBosses.AreaMiniBoss | DefeatedBosses.AreaTorizo, system.BossesDefeatedIn(AreaId.Norfair), "typed set and clear change only their flags");
        AssertTrue(system.HasDefeatedAnyOf(AreaId.Norfair, DefeatedBosses.AreaTorizo), "a named flag is found");
        AssertTrue(!system.HasDefeatedAnyOf(AreaId.Norfair, DefeatedBosses.AreaBoss), "a cleared named flag is absent");
        ProgressionTableCodec.WriteAreaBosses(system.Progress, areas);
        AssertEqual((byte)0x06, areas[SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Norfair)], "the written boss byte holds only the named flags");
        AssertEqual(DefeatedBosses.None, system.BossesDefeatedIn(AreaId.Crateria), "other areas stay clear");
        Console.WriteLine("Progression saved fields: save-field ranges are enforced and boss state holds the named flags.");
    }

    /// <summary>Whether a store's power-on adds a frame to the earthquake rumble's delay.</summary>
    private static bool AddsRumbleFrame(SaveSlotStore store) => store.EarthquakeSoundDelayAtPowerOn().ExtraFrames == 1;

    /// <summary>A save with a distinct health, area, progression and explored map.</summary>
    private static SuperMetroidSaveSnapshot Snapshot(ushort health, AreaId area)
    {
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = health, MaxHealth = 199, Missiles = 5, MaxMissiles = 10 };
        var system = new SharedGameState(new SharedRandom());
        system.SetEvent(StoryEvent.ZebesAwake);
        system.RecordBossesDefeated(area, DefeatedBosses.AreaBoss);
        system.SetCollectedItemBit(health % 64);
        system.MarkSaveStationUsed(LoadStationNumberCodec.Decode(area, 1, "save fixture"));
        system.MarkExploredMapTile(area, new(9, 1));
        var time = new GameTimeState();
        time.Load(frames: 12, seconds: 34, minutes: 5, hours: 1);
        return SuperMetroidSaveSnapshot.Capture(samus, system,
            new SaveSnapshotContext { Checkpoint = SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Decode(area, 1, "save fixture"), GameTime = time });
    }

    /// <summary>The native payload a decoded slot re-encodes to, for field-by-field comparison.</summary>
    private static byte[] PayloadOf(SuperMetroidSaveSlot? slot, IAreaMapSource maps)
    {
        SuperMetroidSaveSlot valid = slot ?? throw new InvalidOperationException("Expected a valid save slot.");
        return SaveRamCodec.EncodePayload(SaveRamCodec.ToSnapshot(valid), maps);
    }

    /// <summary>Runs the SRAM codec and save-store checks.</summary>
    [VerificationCommand("--save-ram-codec")]
    internal static int SaveRamCodecCommand()
    {
        Suite(nameof(VerifyInvalidSlotAndNoncanonicalMarker), () => VerifyInvalidSlotAndNoncanonicalMarker());
        Suite(nameof(VerifyStoreOperationsRoundTrip), () => VerifyStoreOperationsRoundTrip());
        Suite(nameof(VerifyWidenedSlotFieldsRoundTrip), () => VerifyWidenedSlotFieldsRoundTrip());
        Suite(nameof(VerifyProgressionSavedFields), () => VerifyProgressionSavedFields());
        return 0;
    }
}
