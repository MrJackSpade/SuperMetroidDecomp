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
            new SaveSnapshotContext { Area = (ushort)area, SaveStation = 1, GameTime = time });
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
        return 0;
    }
}
