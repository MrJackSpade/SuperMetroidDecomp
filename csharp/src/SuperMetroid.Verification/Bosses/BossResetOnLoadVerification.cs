using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Frontend.GameConfigurationVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the opt-in boss reset policy when a saved game is loaded.</summary>
internal static class BossResetOnLoadVerification
{
    /// <summary>Verifies strict INI parsing and one-time encounter resets while preserving inventory and stored saves.</summary>
    internal static void VerifyBossResetOnLoad()
    {
        Suite(nameof(VerifyGameConfigurationIni), () => VerifyGameConfigurationIni());
        AssertTrue(!new SuperMetroidGameOptions().ResetBossesOnLoad &&
            !SuperMetroidGameOptionsIni.Parse("").ResetBossesOnLoad &&
            !SuperMetroidGameOptionsIni.Parse(SuperMetroidGameOptionsIni.DefaultFileContents).ResetBossesOnLoad,
            "boss reset is disabled in programmatic, legacy and shipped defaults");
        AssertTrue(SuperMetroidGameOptionsIni.Parse("[Game]\nResetBossesOnLoad=true").ResetBossesOnLoad,
            "INI enables boss reset");
        AssertInvalidGameConfiguration("[Game]\nResetBossesOnLoad=yes", "must be either true or false");
        AssertInvalidGameConfiguration("[Game]\nResetBossesOnLoad=true\nresetbossesonload=false", "duplicate");

        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var snapshot = new SuperMetroidSaveSnapshot
        {
            Area = 0, SaveStation = 1, Health = 77, MaxHealth = 199,
            Missiles = 12, MaxMissiles = 25,
            CollectedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = SamusEquipmentFlags.MorphBall,
            BossFlags = Enumerable.Repeat(BossBitMasks.AllKnown, Bank80SystemState.AreaCount).ToArray(),
            EventBytes = Enumerable.Repeat((byte)0xff, Bank80SystemState.EventByteCount).ToArray(),
            OpenedDoorBytes = Enumerable.Repeat((byte)0xff, Bank80SystemState.DoorBitByteCount).ToArray(),
            CollectedItemBytes = Enumerable.Repeat((byte)0xa5, Bank80SystemState.ItemBitByteCount).ToArray(),
            RoomChozoBytes = Enumerable.Repeat((byte)0x5a, Bank80SystemState.RoomChozoBitByteCount).ToArray(),
        };
        // Escape is outside this tester option's scope. Keep this particular save
        // out of escape so room selection can demonstrate both Torizo encounters.
        snapshot.EventBytes[(int)EventNumber.ZebesTimebombSet >> 3] &=
            unchecked((byte)~(1 << ((int)EventNumber.ZebesTimebombSet & 7)));
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        saves.SaveSlot(0, snapshot);
        var slot = saves.ReadSlot(0) ?? throw new InvalidDataException("Boss reset fixture save failed validation.");
        ushort[] eyeBits = [0x45, 0x5c, 0x85, 0x9b, 0xa8];
        foreach (bool enabled in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture();
            runtime.RoomLoader.InitializeSavedGame(slot, resetBossesOnLoad: enabled);
            for (int area = 0; area < Bank80SystemState.AreaCount; area++)
            {
                bool resetArea = enabled && area < AreaIds.RetailCount && area != (int)AreaId.Tourian;
                AssertEqual(resetArea ? (byte)0 : (byte)0x07, runtime.System.GetBossBitsRaw(area),
                    $"boss/miniboss/Torizo load policy area {area}, enabled={enabled}");
            }
            for (int bit = 0; bit < Bank80SystemState.EventByteCount * 8; bit++)
            {
                bool expected = (snapshot.EventBytes[bit >> 3] & (1 << (bit & 7))) != 0 &&
                    !(enabled && bit == (int)EventNumber.ShaktoolClearedPath);
                AssertEqual(expected, runtime.System.HasEventRaw(bit),
                    $"only Shaktool's encounter event resets; event {bit}, enabled={enabled}");
            }
            for (int bit = 0; bit < Bank80SystemState.DoorBitByteCount * 8; bit++)
                AssertEqual(!(enabled && eyeBits.Contains((ushort)bit)), runtime.System.HasOpenedDoorBit(bit),
                    $"only eye-door deaths reset; door {bit}, enabled={enabled}");
            for (int index = 0; index < Bank80SystemState.ItemBitByteCount; index++)
                AssertEqual(snapshot.CollectedItemBytes[index], runtime.System.GetCollectedItemByteRaw(index), "pickups preserved");
            for (int index = 0; index < Bank80SystemState.RoomChozoBitByteCount; index++)
                AssertEqual(snapshot.RoomChozoBytes[index], runtime.System.GetRoomChozoByteRaw(index), "Chozo orbs preserved");
            AssertEqual(slot.CollectedItems, runtime.Player.IntroducedSamus.CollectedItems, "collected inventory preserved");
            AssertEqual(slot.EquippedItems, runtime.Player.IntroducedSamus.EquippedItems, "equipment selection preserved");
            AssertEqual(slot.Health, runtime.Player.IntroducedSamus.Health, "health preserved");
            AssertEqual(slot.Missiles, runtime.Player.IntroducedSamus.Missiles, "ammo preserved");
            foreach (var (room, alive, defeated) in new (ushort, ushort, ushort)[]
                { (0x9804, 0x981b, 0x9835), (0x9dc7, 0x9dd9, 0x9df3),
                  (0xb283, 0xb295, 0xb2af), (0xd8c5, 0xd8d7, 0xd8f1) })
            {
                var header = runtime.RoomLoader.LoadCartridgeRoomHeader(RoomHeaderDefinitions.Get(room));
                AssertEqual(enabled ? alive : defeated, header.State.Pointer,
                    $"loaded progression selects encounter state for room {room:X4}");
            }
            // Prove this is not a continuously enforced immortality/respawn mode.
            runtime.System.SetBossBits(AreaId.Brinstar, BossBits.AreaMiniBoss);
            _ = runtime.RoomLoader.LoadCartridgeRoomHeader(RoomHeaderDefinitions.SporeSpawn);
            AssertTrue(runtime.System.HasAnyBossBits(AreaId.Brinstar, BossBits.AreaMiniBoss),
                "subsequent defeat persists until another explicit save load");
        }
        AssertTrue((saves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires saves.ReadSlot(0).")).BossBytes.SequenceEqual(slot.BossBytes),
            "loading with reset does not rewrite the stored save");
        Console.WriteLine("Boss reset: disabled defaults, strict INI, full saved-state preservation, outside-Tourian flags, five eye doors, Spore/Torizo/Shaktool room selection and load-only application pass.");
    }

    /// <summary>Runs the saved-game boss reset comparison.</summary>
    [VerificationCommand("--boss-reset-on-load")]
    internal static int BossResetOnLoadCommand()
    {
        Suite(nameof(VerifyBossResetOnLoad), () => VerifyBossResetOnLoad());
        return 0;
    }
}
