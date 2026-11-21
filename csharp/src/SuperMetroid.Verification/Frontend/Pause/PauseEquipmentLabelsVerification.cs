using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed equipment labels against the native tilemaps.</summary>
internal static class PauseEquipmentLabelsVerification
{
    /// <summary>Compares inventory tilemaps with ROM reads blocked.</summary>
    internal static void VerifyPauseEquipmentLabelAssets(InstalledScreenContent original)
    {

        SamusState[] inventories =
        [
            new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()),
            new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { CollectedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma, EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Spazer,
                CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope), EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), MaxReserveEnergy = 100 },
            new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { CollectedBeams = SamusEquipmentWordCodec.FromSavedBeamWord(ushort.MaxValue), EquippedBeams = SamusEquipmentWordCodec.FromSavedBeamWord(ushort.MaxValue),
                CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope), EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope) }.WithHyperBeam(true),
        ];
        foreach (SamusState inventory in inventories)
        {
            byte[] expected = EquipmentPage(new PauseMenuState(new PauseMenuEntry { Audio = AudioQueues.Standalone(), Samus = Clone(inventory), System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 1), ScreenContent = original, SpriteSheets = MenuSheetFixtures.For(original), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.None }), original);
            byte[] actual = EquipmentPage(new PauseMenuState(new PauseMenuEntry { Audio = AudioQueues.Standalone(), Samus = Clone(inventory), System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 1), ScreenContent = original, SpriteSheets = MenuSheetFixtures.For(original), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.None }), original);
            AssertTrue(expected.SequenceEqual(actual),
                "installed equipment labels match native tilemap with label ROM sources forbidden");
        }

        Console.WriteLine("Pause equipment labels: native inventory/Hyper parity with blocked ROM sources passes.");

        static byte[] EquipmentPage(PauseMenuState pause, InstalledScreenContent maps)
        {
            EnterPauseEquipment(pause);
            return pause.ShownVram(maps).AsSpan(0x6000, 0x800).ToArray();
        }

        static SamusState Clone(SamusState value) => new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            CollectedBeams = value.CollectedBeams,
            EquippedBeams = value.EquippedBeams,
            CollectedItems = value.CollectedItems,
            EquippedItems = value.EquippedItems,
            MaxReserveEnergy = value.MaxReserveEnergy,
        }.WithHyperBeam(value.HyperBeamEnabled);
    }
}
