using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Compares installed pause reserve tiles, arrow color phases, and rendering with native source data.</summary>
internal static class PauseReserveUiAssetsVerification
{
    /// <summary>Checks reserve labels, digits, and arrow phases against native rendering.</summary>
    internal static void VerifyPauseReserveUiAssets(ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        var guard = NewPauseReserveUiReadGuard(bus);

        var nativeSamus = Samus(); var installedSamus = Samus();
        var native = new PauseMenuState(new PauseMenuEntry { Samus = nativeSamus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Brinstar, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var installed = new PauseMenuState(new PauseMenuEntry { Samus = installedSamus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Brinstar, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        EnterEquipment(native); EnterEquipment(installed);
        for (int frame = 0; frame < 64; frame++)
        {
            native.Step(0, 0, frameCycle: (byte)frame);
            installed.Step(0, 0, frameCycle: (byte)frame);
            var shownNative = native.ShownVram(original);
            var shownInstalled = installed.ShownVram(original);
            AssertTrue(shownNative.SequenceEqual(shownInstalled),
                "installed reserve labels/digits/arrow tile palettes match native with their ROM sources forbidden");
            AssertTrue(PauseShownColours.Of(native, original).SequenceEqual(PauseShownColours.Of(installed, original)),
                "all 32 reserve-arrow color phases match native");
            AssertTrue(ScenePixels.Same(native.CaptureRenderSnapshot(), installed.CaptureRenderSnapshot()),
                "reserve UI migration retains native pixels");
        }

        Console.WriteLine("Pause reserve UI: labels, ten digits and 32 arrow phases match native with guarded reads.");

        static SamusState Samus() => new(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            MaxHealth = 499, Health = 499, MaxReserveEnergy = 400, ReserveEnergy = 245,
            ReserveTankMode = ReserveTankMode.Auto,
        };
        static void EnterEquipment(PauseMenuState pause)
        {
            byte nmiFrame = 0; ChangePausePage(pause, ControllerButton.R, input => pause.Step(input, input, frameCycle: nmiFrame++));
            AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "reserve UI fixture enters equipment");
        }
    }

    /// <summary>Forbids the migrated reserve tilemap and arrow color ranges during installed menu playback.</summary>
    private static CartridgeReadGuard NewPauseReserveUiReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Block(int start, int count) { for (int i = 0; i < count; i++) blocked.Add(start + i); }
        Block(0x82c068, 4); Block(0x82c088, 4); Block(0x82bf22, 16);
        Block(0x82ad5d, 64); Block(0x82ad9d, 64);
        for (int index = 0; index < 2; index++)
        {
            int sourceAddress = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c088 + index * 2);
            Block(sourceAddress, 14);
        }
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read reserve UI at {address:X6}.");
    }
}
