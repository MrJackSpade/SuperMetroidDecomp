using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
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

        foreach ((byte[] marker, bool expectedCarry) in new (byte[], bool)[]
                 {
                     // Last word "ie" ($6569) is above "id" ($6469): the first mismatch leaves carry set.
                     ("supermetroie"u8.ToArray(), true),
                     // Last word "ic" ($6369) is below "id": carry clear.
                     ("supermetroic"u8.ToArray(), false),
                 })
        {
            marker.CopyTo(image, SaveRamLayout.CompletionMarkerOffset);
            SaveSlotStore decoded = SaveRamCodec.Decode(image, maps);

            AssertTrue(decoded.ReadSlot(1) is null && !decoded.IsSlotValid(1), "a slot failing both checksum pairs decodes empty");
            AssertTrue(PayloadOf(decoded.ReadSlot(0), maps).SequenceEqual(PayloadOf(written.ReadSlot(0), maps)),
                "a valid slot decodes to the record that was written");
            AssertEqual(1, decoded.ReadSelectedSlot(), "the selection decodes as written, whatever the selected slot holds");
            AssertEqual(expectedCarry, decoded.DetermineBootMainLoopCarry(), "the decoded boot carry compares the raw marker words");
            AssertTrue(!decoded.HasCompletedGame, "a noncanonical marker is not the completed-game signature");
            decoded.SaveSlot(2, Snapshot(health: 33, area: AreaId.Crateria));
            AssertEqual(expectedCarry, decoded.DetermineBootMainLoopCarry(),
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
        AssertTrue(store.DetermineBootMainLoopCarry(), "an empty store's boot carry is set");
        AssertEqual(DemoSetUnlockRomData.DefaultSetCount, store.DetermineNumberOfDemoSets(), "an all-invalid boot fixes three demo sets");
        AssertEqual("madadameyohn", Marker(), "an all-invalid boot writes the corrupt-SRAM marker");
        RoundTrips("an all-invalid boot");
        store.SaveSlot(0, Snapshot(health: 61, area: AreaId.Brinstar));
        // Last word "hn" ($6E68) is above "id" ($6469): the corrupt marker leaves the next boot's carry set.
        AssertTrue(store.DetermineBootMainLoopCarry(), "boot carry over the corrupt marker");
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
        AssertTrue(store.DetermineBootMainLoopCarry(), "a completed save's boot carry is set");
        AssertEqual(DemoSetUnlockRomData.SetCount, store.DetermineNumberOfDemoSets(), "a completed save enables the fourth demo set");
        RoundTrips("completing the game");
        store.SetGameCompleted(false);
        AssertTrue(!store.DetermineBootMainLoopCarry(), "a valid incomplete save's boot carry is clear");
        RoundTrips("clearing completion");
        Console.WriteLine("Save store: boot, save, copy, clear, select and completion have the cartridge's effect and round-trip through SRAM.");
    }

    /// <summary>
    /// The slot fields saves now carry as <see cref="int"/> (item-load counter, clock, dispatcher route, checkpoint and
    /// the two checksummed markers) hold every value their SRAM field can: the range's ends survive SRAM, store and SRAM
    /// again byte for byte, and a value past the range is rejected instead of wrapping into the image.
    /// </summary>
    internal static void VerifyWidenedSlotFieldsRoundTrip()
    {
        IAreaMapSource maps = RetailPresentationFixture();
        var written = new SaveSlotStore(maps);
        SuperMetroidSaveSnapshot extremes = Snapshot(health: 61, area: AreaId.Brinstar) with
        {
            LoadedItemCount = SaveFieldValue.MaximumValue,
            DebugFlag = SaveFieldValue.MaximumValue,
            NewFileMarker = 0,
            LoadingGameState = SaveFieldValue.MaximumValue,
            SaveStation = SaveFieldValue.MaximumValue,
            Area = SaveFieldValue.MaximumValue,
            GameTimeFrames = SaveFieldValue.MaximumValue,
            GameTimeSeconds = 0,
            GameTimeMinutes = SaveFieldValue.MaximumValue,
            GameTimeHours = SaveFieldValue.MaximumValue,
        };
        written.SaveSlot(0, extremes);
        byte[] image = SaveRamCodec.Encode(written, maps);
        SaveSlotStore decoded = SaveRamCodec.Decode(image, maps);
        AssertTrue(SaveRamCodec.Encode(decoded, maps).SequenceEqual(image),
            "SRAM -> store -> SRAM keeps the full-range slot fields byte for byte");
        SuperMetroidSaveSlot slot = decoded.ReadSlot(0) ?? throw new InvalidOperationException("The full-range slot failed its checksums.");
        AssertEqual(SaveFieldValue.MaximumValue, slot.LoadedItemCount, "the item-load counter decodes its largest value");
        AssertEqual(SaveFieldValue.MaximumValue, slot.DebugFlag, "the debug value decodes its largest value");
        AssertEqual(0, slot.NewFileMarker, "the new-file marker decodes zero");
        AssertEqual(SaveFieldValue.MaximumValue, slot.LoadingGameState, "an unnamed dispatcher route decodes unchanged");
        AssertEqual(SaveLoadingGameState.MainGame, slot.StartupRoute, "an unnamed dispatcher route resumes the main game");
        AssertEqual(SaveFieldValue.MaximumValue, slot.SaveStation, "the load station decodes its largest value");
        AssertEqual(SaveFieldValue.MaximumValue, slot.Area, "the checkpoint area decodes its largest value");
        AssertEqual(SaveFieldValue.MaximumValue, slot.GameTimeHours, "the clock's hours decode their largest value");
        AssertEqual(SaveFieldValue.MaximumValue, SaveRamCodec.ReadSlotWord(image, 0, SaveRamLayout.LoadedItemCountOffset),
            "the item-load counter's SRAM field holds the counter's largest value");

        foreach ((string field, SuperMetroidSaveSnapshot outOfRange) in new (string, SuperMetroidSaveSnapshot)[]
                 {
                     ("item-load counter", extremes with { LoadedItemCount = SaveFieldValue.MaximumValue + 1 }),
                     ("dispatcher route", extremes with { LoadingGameState = -1 }),
                     ("load station", extremes with { SaveStation = SaveFieldValue.MaximumValue + 1 }),
                     ("clock minutes", extremes with { GameTimeMinutes = -1 }),
                 })
        {
            AssertThrows<InvalidOperationException>(() => written.SaveSlot(1, outOfRange),
                $"a {field} past the SRAM field's range is rejected instead of wrapping");
        }
        AssertTrue(written.ReadSlot(1) is null, "a rejected save leaves its slot unchanged");
        Console.WriteLine("Save slot fields: full-range values round-trip through SRAM exactly; out-of-range values are rejected.");
    }

    /// <summary>
    /// The progression owner's saved fields: the loaded-item count and the saved dispatcher route accept exactly a save
    /// field's range, and an area's boss byte keeps its unproven bits through typed set, clear and query, so the byte the
    /// save mirror receives is the byte it was loaded with, changed only in the named bits.
    /// </summary>
    internal static void VerifyProgressionSavedFields()
    {
        var system = new Bank80SystemState { LoadedItemCount = SaveFieldValue.MaximumValue };
        AssertEqual(SaveFieldValue.MaximumValue, system.LoadedItemCount, "the loaded-item count holds a save field's largest value");
        AssertThrows<ArgumentOutOfRangeException>(() => system.LoadedItemCount = SaveFieldValue.MaximumValue + 1,
            "a loaded-item count past a save field is rejected instead of wrapping");
        AssertThrows<ArgumentOutOfRangeException>(() => system.LoadedItemCount = -1, "a negative loaded-item count is rejected");
        AssertEqual(SaveFieldValue.MaximumValue, system.LoadedItemCount, "a rejected count leaves the count unchanged");
        system.LoadSavedLoadingGameState(SaveFieldValue.MaximumValue);
        AssertEqual(SaveFieldValue.MaximumValue, system.SavedLoadingGameState, "an unnamed dispatcher route is kept as loaded");
        AssertThrows<ArgumentOutOfRangeException>(() => system.LoadSavedLoadingGameState(-1),
            "a dispatcher route outside a save field is rejected");

        // An unproven bit beside two named ones: typed operations change only the named bits around it.
        byte unprovenBit = 128;
        byte loaded = (byte)(unprovenBit | (BossBits.AreaBoss | BossBits.AreaTorizo).ToNativeByte());
        byte[] areas = new byte[Bank80SystemState.AreaCount];
        areas[AreaIds.ToIndex(AreaId.Norfair)] = loaded;
        system.LoadBossBytes(areas);
        AssertEqual(loaded, system.GetBossBitsRaw(AreaId.Norfair), "a loaded boss byte reads back exactly");
        system.SetBossBits(AreaId.Norfair, BossBits.AreaMiniBoss);
        system.ClearBossBits(AreaId.Norfair, BossBits.AreaBoss);
        AssertEqual((byte)(unprovenBit | (BossBits.AreaMiniBoss | BossBits.AreaTorizo).ToNativeByte()),
            system.GetBossBitsRaw(AreaId.Norfair), "typed set and clear keep the unproven bit");
        AssertTrue(system.HasAnyBossBits(AreaId.Norfair, BossBits.AreaTorizo), "a named bit beside an unproven one is found");
        AssertTrue(!system.HasAnyBossBits(AreaId.Norfair, BossBits.AreaBoss), "a cleared named bit is absent");
        AssertThrows<ArgumentOutOfRangeException>(() => system.GetBossBits(AreaId.Norfair),
            "named boss state refuses to hand out an unproven bit");
        AssertEqual(0, system.GetBossBitsRaw(AreaId.Crateria), "other areas stay clear");
        Console.WriteLine("Progression saved fields: save-field ranges are enforced and boss bytes keep their unproven bits.");
    }

    /// <summary>A save with a distinct health, area, progression and explored map.</summary>
    private static SuperMetroidSaveSnapshot Snapshot(ushort health, AreaId area)
    {
        var samus = new SamusState { Health = health, MaxHealth = 199, Missiles = 5, MaxMissiles = 10 };
        var system = new Bank80SystemState();
        system.SetEvent(EventNumber.ZebesAwake);
        system.SetBossBits(area, BossBits.AreaBoss);
        system.SetCollectedItemBit(health % 64);
        system.MarkSaveStationUsed(area, 1);
        system.MarkExploredMapTile(area, new(9, 1));
        var time = new GameTimeState();
        time.Load(frames: 12, seconds: 34, minutes: 5, hours: 1);
        return SuperMetroidSaveSnapshot.Capture(samus, system,
            new SaveSnapshotContext { Area = area, SaveStation = 1, GameTime = time });
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
