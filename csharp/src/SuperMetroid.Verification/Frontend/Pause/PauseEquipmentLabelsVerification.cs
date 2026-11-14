using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed equipment labels against the native tilemaps.</summary>
internal static class PauseEquipmentLabelsVerification
{
    /// <summary>Compares inventory tilemaps with ROM reads blocked.</summary>
    internal static void VerifyPauseEquipmentLabelAssets(AreaMapPresentation original)
    {

        SamusState[] inventories =
        [
            new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()),
            new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { CollectedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma, EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Spazer,
                CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope), EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), MaxReserveEnergy = 100 },
            new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { HyperBeamEnabled = true, CollectedBeams = SamusEquipmentWordCodec.FromSavedBeamWord(ushort.MaxValue), EquippedBeams = SamusEquipmentWordCodec.FromSavedBeamWord(ushort.MaxValue),
                CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.UnassignedItem | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope), EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.UnassignedItem | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope) },
        ];
        foreach (SamusState inventory in inventories)
        {
            byte[] expected = EquipmentPage(new PauseMenuState(new PauseMenuEntry { Samus = Clone(inventory), System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) }), original);
            byte[] actual = EquipmentPage(new PauseMenuState(new PauseMenuEntry { Samus = Clone(inventory), System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) }), original);
            AssertTrue(expected.SequenceEqual(actual),
                "installed equipment labels match native tilemap with label ROM sources forbidden");
        }

        Console.WriteLine("Pause equipment labels: native inventory/Hyper parity with blocked ROM sources passes.");

        static byte[] EquipmentPage(PauseMenuState pause, AreaMapPresentation maps)
        {
            EnterPauseEquipment(pause);
            return pause.ShownVram(maps).AsSpan(0x6000, 0x800).ToArray();
        }

        static SamusState Clone(SamusState value) => new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedBeams = value.CollectedBeams,
            EquippedBeams = value.EquippedBeams,
            CollectedItems = value.CollectedItems,
            EquippedItems = value.EquippedItems,
            HyperBeamEnabled = value.HyperBeamEnabled,
            MaxReserveEnergy = value.MaxReserveEnergy,
        };
    }
}
