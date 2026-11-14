using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed pause equipment base tiles through page transitions and debugger rebinding.</summary>
internal static class PauseEquipmentBaseAssetsVerification
{
    /// <summary>Compares 1,024 native cells and the page transitions with native output.</summary>
    internal static void VerifyPauseEquipmentBaseAssets(ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        byte[] nativeBase = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), PauseMenuSourceDefinitions.EquipmentPage,
            PauseEquipmentBaseDefinitions.Cells * sizeof(ushort));
        AssertTrue(BackgroundTileWordBytes.Tiles(nativeBase).SequenceEqual(InstalledFixtures.ImportOf(original).Pause.EquipmentBase.CreateTilemap()),
            "all 1024 equipment base cells match the native tiles");

        var native = new PauseMenuState(new PauseMenuEntry { Samus = Samus(), System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var installed = new PauseMenuState(new PauseMenuEntry { Samus = Samus(), System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        int toMap = PausePageTransitionNativeLength + 1;
        for (int frame = 0; frame < toMap + PausePageTransitionNativeLength + 2; frame++)
        {
            ControllerButton input = frame == 0 ? ControllerButton.R : frame == toMap ? ControllerButton.L : 0;
            native.Step(input, input, frameCycle: (byte)frame);
            installed.Step(input, input, frameCycle: (byte)frame);
            var shownNative = native.ShownVram(original);
            var shownInstalled = installed.ShownVram(original);
            AssertTrue(shownNative.SequenceEqual(shownInstalled),
                "equipment base migration retains exact native VRAM through both page transitions");
            AssertTrue(ScenePixels.Same(native.CaptureRenderSnapshot(), installed.CaptureRenderSnapshot()),
                "equipment base migration retains native pixels with its source forbidden");
        }
        AssertEqual(PauseScreenMode.Map, installed.ScreenMode, "equipment base fixture returns to the map page");

        Console.WriteLine("Pause equipment base: 1024 native words and guarded transitions pass.");

        static SamusState Samus() => new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedBeams = (SamusBeamFlags.Charge | SamusBeamFlags.Ice | SamusBeamFlags.Wave),
            EquippedBeams = (SamusBeamFlags.Charge | SamusBeamFlags.Wave),
            CollectedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit |
                SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.HiJumpBoots |
                SamusEquipmentFlags.SpeedBooster),
            EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.MorphBall |
                SamusEquipmentFlags.Bombs | SamusEquipmentFlags.HiJumpBoots),
            MaxHealth = 499, Health = 499, MaxReserveEnergy = 400, ReserveEnergy = 245, ReserveTankMode = ReserveTankMode.Auto,
        };
    }
}
