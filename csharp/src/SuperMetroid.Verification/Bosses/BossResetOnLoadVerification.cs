using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Frontend.GameConfigurationVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
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
            !HostOptionsIni.Parse("").Game.ResetBossesOnLoad &&
            !HostOptionsIni.Parse(HostOptionsIni.DefaultFileContents).Game.ResetBossesOnLoad,
            "boss reset is disabled in programmatic, legacy and shipped defaults");
        AssertTrue(HostOptionsIni.Parse("[Game]\nResetBossesOnLoad=true").Game.ResetBossesOnLoad,
            "INI enables boss reset");
        AssertInvalidGameConfiguration("[Game]\nResetBossesOnLoad=yes", "must be either true or false");
        AssertInvalidGameConfiguration("[Game]\nResetBossesOnLoad=true\nresetbossesonload=false", "duplicate");

        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var snapshot = new SuperMetroidSaveSnapshot
        {
            Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.CrateriaSave, Health = 77, MaxHealth = 199,
            Missiles = 12, MaxMissiles = 25,
            CollectedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = SamusEquipmentFlags.MorphBall,
        };
        // Every boss of every area, every event and door, and every third pickup.
        foreach (AreaId area in AreaIds.All)
            snapshot.Progress.SetDefeatedBosses(area, BossBitMasks.AllKnown);
        snapshot.Progress.Events.AddAll();
        snapshot.Progress.OpenedDoors.AddAll();
        for (int place = 0; place < ItemLocationDefinitions.All.Count; place++)
            snapshot.Progress.CollectedItems.Set(ItemLocationDefinitions.All[place], place % 3 == 0);
        // Escape is outside this tester option's scope. Keep this particular save
        // out of escape so room selection can demonstrate both Torizo encounters.
        snapshot.Progress.ClearEvent(StoryEvent.ZebesTimebombSet);
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        saves.SaveSlot(0, snapshot);
        var slot = saves.ReadSlot(0) ?? throw new InvalidDataException("Boss reset fixture save failed validation.");
        LockedDoor[] eyeDoors = [LockedDoorDefinitions.KraidEyeDoorEyeDoorFacingLeft, LockedDoorDefinitions.LNFarmingEyeDoorFacingRight,
            LockedDoorDefinitions.BasementDefaultEyeDoorFacingLeft, LockedDoorDefinitions.ThePreciousEyeDoorFacingRight,
            LockedDoorDefinitions.TourianEyeDoorEyeDoorFacingLeft];
        foreach (bool enabled in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture(
                encounterResetOnLoad: new SuperMetroidGameOptions { ResetBossesOnLoad = enabled }.ToAssists().EncounterResetOnLoad);
            runtime.RoomLoader.InitializeSavedGame(slot);
            foreach (AreaId area in AreaIds.All)
            {
                bool resetArea = enabled && area != AreaId.Tourian;
                AssertEqual(resetArea ? BossBits.None : BossBitMasks.AllKnown, runtime.System.GetBossBits(area),
                    $"boss/miniboss/Torizo load policy area {area}, enabled={enabled}");
            }
            foreach (StoryEvent storyEvent in Enum.GetValues<StoryEvent>())
            {
                bool expected = snapshot.Progress.Events.Contains(storyEvent) &&
                    !(enabled && storyEvent == StoryEvent.ShaktoolClearedPath);
                AssertEqual(expected, runtime.System.Progress.Events.Contains(storyEvent),
                    $"only Shaktool's encounter event resets; event {storyEvent}, enabled={enabled}");
            }
            foreach (LockedDoor door in LockedDoorDefinitions.All)
                AssertEqual(!(enabled && eyeDoors.Contains(door)), runtime.System.HasOpenedDoor(door),
                    $"only eye-door deaths reset; door {door}, enabled={enabled}");
            AssertTrue(snapshot.Progress.CollectedItems.SetEquals(runtime.System.Progress.CollectedItems), "pickups preserved");
            AssertEqual(slot.CollectedItems, runtime.Player.IntroducedSamus.CollectedItems, "collected inventory preserved");
            AssertEqual(slot.EquippedItems, runtime.Player.IntroducedSamus.EquippedItems, "equipment selection preserved");
            AssertEqual(slot.Health, runtime.Player.IntroducedSamus.Health, "health preserved");
            AssertEqual(slot.Missiles, runtime.Player.IntroducedSamus.Missiles, "ammo preserved");
            foreach (var (room, alive, defeated) in new VerifyBossResetOnLoadCase[]
                { new VerifyBossResetOnLoadCase(0x9804, 0x981b, 0x9835), new VerifyBossResetOnLoadCase(0x9dc7, 0x9dd9, 0x9df3),
                  new VerifyBossResetOnLoadCase(0xb283, 0xb295, 0xb2af), new VerifyBossResetOnLoadCase(0xd8c5, 0xd8d7, 0xd8f1) })
            {
                var header = runtime.RoomLoader.LoadSelectedRoom(RoomHeaderDefinitions.Get(room));
                AssertEqual(enabled ? alive : defeated, header.State.Pointer,
                    $"loaded progression selects encounter state for room {room:X4}");
            }
            // Prove this is not a continuously enforced immortality/respawn mode.
            runtime.System.SetBossBits(AreaId.Brinstar, BossBits.AreaMiniBoss);
            _ = runtime.RoomLoader.LoadSelectedRoom(RoomHeaderDefinitions.SporeSpawn);
            AssertTrue(runtime.System.HasAnyBossBits(AreaId.Brinstar, BossBits.AreaMiniBoss),
                "subsequent defeat persists until another explicit save load");
        }
        AssertTrue((saves.ReadSlot(0) ?? throw new InvalidOperationException("Verification requires saves.ReadSlot(0).")).Progression.SameAs(slot.Progression),
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
