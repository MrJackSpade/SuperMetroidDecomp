using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>
/// Protects the reserve-page arrow's picture across presentation rebinding and debugger-state restoration: the arrow is
/// derived from the cursor and the reserve mode (#1597), so neither changes it.
/// </summary>
internal static class PauseReserveArrowRebindVerification
{
    /// <summary>
    /// Rebinding the same presentation on a settled reserve page, and restoring a debugger state, show the same arrow
    /// and the same page pixels.
    /// </summary>
    internal static void VerifyPauseReserveArrowRebindKeepsPicture()
    {
        var catalog = RetailPresentationFixture();
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { MaxReserveEnergy = 100, ReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Manual, Health = 50, MaxHealth = 99 };
        var menu = new PauseMenuState(new PauseMenuEntry { Audio = AudioQueues.Standalone(), Samus = samus, System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 1), ScreenContent = catalog, SpriteSheets = MenuSheetFixtures.For(catalog), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.None });
        EnterPauseEquipment(menu);
        AssertEqual(PauseEquipmentCategory.Reserves, menu.SelectedCategory, "fixture settles on the reserve page selection");
        var pixels = menu.Render();
        byte[] vram = menu.ShownVram(catalog).ToArray();

        menu.BindFixtureArtwork(catalog);
        AssertTrue(vram.SequenceEqual(menu.ShownVram(catalog)), "same-content rebind keeps the reserve arrow tilemap");
        AssertTrue(pixels.SequenceEqual(menu.Render()), "same-content rebind keeps exact reserve page pixels");

        // A debugger state binds the installed content rather than writing it, as the debugger's store does, so the round
        // trip carries only the pause session's own state.
        PauseMenuState restored = GraphDigestVerification.RoundTrip(menu);
        restored.BindFixtureArtwork(catalog);
        AssertTrue(pixels.SequenceEqual(restored.Render()), "restore plus rebind keeps exact reserve page pixels");
        Console.WriteLine("Reserve arrow rebind: same-content rebind and restore keep the arrow and exact pixels.");
    }
}
