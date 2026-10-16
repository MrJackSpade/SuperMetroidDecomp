using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
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
/// Checks <see cref="SaveRamCodec"/> and <see cref="SaveSlotStore"/> against the bus-backed SRAM path they replace
/// (#1322): decoding applies the cartridge's validity and boot-carry rules to an image as given, and store operations
/// encode to the bytes the SRAM path writes.
/// </summary>
internal static class SaveRamCodecVerification
{
    /// <summary>
    /// An image with one invalid slot and a noncanonical completion marker decodes to an empty slot, the same valid
    /// slots, selection and boot carry as the bus implementation reads from that image.
    /// </summary>
    internal static void VerifyInvalidSlotAndNoncanonicalMarker()
    {
        IAreaMapSource maps = RetailPresentationFixture();
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var sram = new SuperMetroidSaveRam(bus, maps);
        sram.SaveSlot(0, Snapshot(health: 61, area: AreaId.Brinstar));
        sram.SaveSlot(1, Snapshot(health: 77, area: AreaId.Norfair));
        sram.SelectSlot(1);
        byte[] image = bus.SaveRam.ToArray();
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
            var old = SuperMetroidAddressSpace.CreateWithoutCartridge();
            image.CopyTo(old.SaveRam);
            var oldSram = new SuperMetroidSaveRam(old, maps);
            SaveSlotStore decoded = SaveRamCodec.Decode(image, maps);

            AssertTrue(oldSram.ReadSlot(1) is null && decoded.ReadSlot(1) is null && !decoded.IsSlotValid(1),
                "a slot failing both checksum pairs decodes empty");
            AssertTrue(PayloadOf(decoded.ReadSlot(0), maps).SequenceEqual(PayloadOf(oldSram.ReadSlot(0), maps)),
                "a valid slot decodes to the record the bus implementation reads");
            AssertEqual(oldSram.ReadSelectedSlot(), decoded.ReadSelectedSlot(), "selected slot decodes as the bus implementation reads it");
            AssertEqual(expectedCarry, oldSram.DetermineBootMainLoopCarry(), "the bus implementation's boot carry for the marker");
            AssertEqual(expectedCarry, decoded.DetermineBootMainLoopCarry(), "the decoded boot carry compares the raw marker words");
            AssertTrue(!decoded.HasCompletedGame, "a noncanonical marker is not the completed-game signature");
        }
        Console.WriteLine("SRAM decode: an invalid slot decodes empty; noncanonical markers keep the bus implementation's boot carry.");
    }

    /// <summary>Every store operation encodes to the SRAM bytes the same operations write through the bus.</summary>
    internal static void VerifyStoreMatchesSramPath()
    {
        IAreaMapSource maps = RetailPresentationFixture();
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var sram = new SuperMetroidSaveRam(bus, maps);
        var store = new SaveSlotStore(maps);
        void Both(Action<SuperMetroidSaveRam> onSram, Action<SaveSlotStore> onStore, string step)
        {
            onSram(sram);
            onStore(store);
            AssertTrue(SaveRamCodec.Encode(store, maps).SequenceEqual(bus.SaveRam.ToArray()), $"store encodes the SRAM image after {step}");
            AssertTrue(SaveRamCodec.Encode(SaveRamCodec.Decode(bus.SaveRam, maps), maps).SequenceEqual(bus.SaveRam.ToArray()),
                $"the SRAM image round-trips through the codec after {step}");
        }

        // A canonical image stores the selection's complement; power-on SRAM starts with neither word written.
        Both(s => s.SelectSlot(0), s => s.SelectSlot(0), "selecting slot A");
        // Boot with every slot invalid writes the corrupt marker and fixes three demo sets.
        AssertEqual(sram.DetermineBootMainLoopCarry(), store.DetermineBootMainLoopCarry(), "empty boot carry");
        int sramSets = 0, storeSets = 0;
        Both(s => sramSets = s.DetermineNumberOfDemoSets(), s => storeSets = s.DetermineNumberOfDemoSets(), "an all-invalid boot");
        AssertEqual(sramSets, storeSets, "an all-invalid boot fixes the same demo-set count");
        SuperMetroidSaveSnapshot first = Snapshot(health: 61, area: AreaId.Brinstar);
        SuperMetroidSaveSnapshot second = Snapshot(health: 88, area: AreaId.Maridia);
        Both(s => s.SaveSlot(0, first), s => s.SaveSlot(0, first), "saving slot A");
        AssertEqual(sram.DetermineBootMainLoopCarry(), store.DetermineBootMainLoopCarry(), "boot carry over the corrupt marker");
        Both(s => s.SaveSlot(2, second), s => s.SaveSlot(2, second), "saving slot C");
        Both(s => s.CopySlot(0, 1), s => s.CopySlot(0, 1), "copying slot A to B");
        Both(s => s.ClearSlot(2), s => s.ClearSlot(2), "clearing slot C");
        Both(s => s.SelectSlot(1), s => s.SelectSlot(1), "selecting slot B");
        Both(s => s.SetGameCompleted(true), s => s.SetGameCompleted(true), "completing the game");
        AssertEqual(sram.DetermineBootMainLoopCarry(), store.DetermineBootMainLoopCarry(), "completed boot carry");
        AssertEqual(sram.DetermineNumberOfDemoSets(), store.DetermineNumberOfDemoSets(), "completed demo-set count");
        AssertTrue(PayloadOf(store.ReadSlot(1), maps).SequenceEqual(PayloadOf(sram.ReadSlot(1), maps)),
            "a copied slot reads back as the bus implementation's copy");
        Console.WriteLine("Save store: boot, save, copy, clear, select and completion encode to the SRAM path's bytes and round-trip.");
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

    /// <summary>Runs the SRAM codec and save-store equivalence checks.</summary>
    [VerificationCommand("--save-ram-codec")]
    internal static int SaveRamCodecCommand()
    {
        Suite(nameof(VerifyInvalidSlotAndNoncanonicalMarker), () => VerifyInvalidSlotAndNoncanonicalMarker());
        Suite(nameof(VerifyStoreMatchesSramPath), () => VerifyStoreMatchesSramPath());
        return 0;
    }
}
