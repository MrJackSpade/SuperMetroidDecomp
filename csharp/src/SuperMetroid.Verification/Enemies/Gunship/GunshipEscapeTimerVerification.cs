using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Gunship;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Escape;
/// <summary>Checks escape-timer suspension and OAM removal as Samus boards the Landing Site gunship.</summary>
internal static class GunshipEscapeTimerVerification
{
    /// <summary>Confirms boarding suppresses timer sprites and countdown before liftoff reuses their VRAM tiles for dust.</summary>
    internal static int VerifyGunshipEscapeTimer()
    {
        var game=SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
        var maps = InstalledFixtures.Maps;
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.System.SetEvent(StoryEvent.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0x0400, 0x0400));
        var samus = runtime.Player.Samus;
        var top = runtime.Enemies.Places.InUpdateOrder.First(place => place.NativeHeader.InitializationAiPointer == 0xa644);
        samus.Control.Unlock();
        samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose);
        samus.Position = samus.Position.WithWhole(new(top.Position.X.Whole, (ushort)(top.Position.Y.Whole - 30)));
        SamusPresentation.InitializeAnimation(samus);
        runtime.EscapeTimer.RunIntoPlace();
        runtime.EscapeTimer.SetRemaining(EscapeTimerClock.Time(2, 59, 50));
        runtime.StepFrame(0);
        AssertTrue(samus.Control.Mode.Movement != SamusMovementMode.Gunship, "Idle ship retains normal Samus handler");
        AssertTrue(TimerSpritesPresent(), "Ordinary escape timer remains visible before boarding");
        samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose);
        samus.Position = samus.Position.WithWholeY(0x0440);
        SamusPresentation.InitializeAnimation(samus);
        var timeBeforeBoarding = Time();
        runtime.StepFrame(ControllerButton.Down);
        AssertTrue(runtime.Enemies.LastEnemyFrame.Reports(new GunshipEvent(GunshipFrameEvent.EntryStarted)), "Down starts actual Landing Site boarding");
        AssertEqual(timeBeforeBoarding, Time(), "Boarding handler stops the timer on its installation frame");
        AssertTrue(!TimerSpritesPresent(), "Boarding removes the timer draws immediately");
        bool sawDustUpload = false;
        for (int frame = 0; frame < 500; frame++)
        {
            runtime.StepFrame(0);
            AssertTrue(runtime.EscapeTimer.IsActive, "Gunship preserves native active timer status");
            AssertEqual(timeBeforeBoarding, Time(), "Gunship handler omits timer countdown");
            AssertTrue(!TimerSpritesPresent(), "No timer sprites reference repurposed dust tiles");
            if (runtime.Enemies.Places.InUpdateOrder.Single(place => place.SpeciesState<GunshipState>() is not null).RequireSpeciesState<GunshipState>().Phase == GunshipPhase.FireUpEngines)
            {
                sawDustUpload = true;
                break;
            }
        }
        AssertTrue(sawDustUpload, "Confirmation reaches the actual liftoff dust upload");
        // The takeoff look shows on the update the engines are readied (#1406, #1538).
        AssertTrue((runtime.Room.Scope ?? throw new InvalidOperationException("No room is loaded.")).Looks.Shows(GunshipTakeoffDefinitions.TakeoffLook), "Liftoff shows the takeoff look once its engines are readied");
        AssertTrue(!TimerSpritesPresent(), "No stale timer draw remains once the takeoff look shows");
        AssertEqual(timeBeforeBoarding, Time(), "Liftoff keeps the timer suspended");
        Console.WriteLine("Gunship escape timer: normal timer draws, same-frame boarding suppression, preserved active status and no timer sprites through liftoff dust upload passed.");
        return 0;

        int Time() => runtime.EscapeTimer.RemainingCentiseconds;
        bool TimerSpritesPresent() => runtime.Sprites.Draws.Any(draw => draw.Source.Kind == SpriteSourceKind.EscapeTimer);
    }

    /// <summary>Runs the gunship boarding escape-timer reproduction.</summary>
    [VerificationCommand("--gunship-escape-timer")]
    internal static int GunshipEscapeTimerCommand()
    {
        return VerifyGunshipEscapeTimer();
    }
}
