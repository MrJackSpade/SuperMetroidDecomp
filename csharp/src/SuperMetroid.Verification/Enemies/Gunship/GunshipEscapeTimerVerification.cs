using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Gunship;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
/// <summary>Checks escape-timer suspension and OAM removal as Samus boards the Landing Site gunship.</summary>
internal static class GunshipEscapeTimerVerification
{
    /// <summary>Confirms boarding suppresses timer sprites and countdown before liftoff reuses their VRAM tiles for dust.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native VRAM words")]
    internal static int VerifyGunshipEscapeTimer()
    {
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        var maps = InstalledFixtures.Maps;
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime=game.Session.runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0x0400, 0x0400));
        var samus = runtime.Player.IntroducedSamus;
        var top = runtime.Enemies.SlotTable.Slots.First(slot => slot.Definition.InitializationAiPointer == 0xa644);
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(top.Position.X.Whole, (ushort)(top.Position.Y.Whole - 30)));
        samus.Presentation.InitializeAnimation();
        samus.Presentation.PrimeGraphics();
        runtime.EscapeTimer.SelectStatus(EscapeTimerState.RunningInPlace, active: true);
        runtime.EscapeTimer.SetTime(PackedBcd.FromDecimal(2), PackedBcd.FromDecimal(59), PackedBcd.FromDecimal(50));
        runtime.QueueEscapeTimerSpriteTiles();
        runtime.StepFrame(0);
        var timerTilesBefore = runtime.Vram.Bytes.Slice(0xfc00, 0x400).ToArray();
        AssertTrue(samus.Control.Mode.Beta != SamusBetaHandler.Gunship, "Idle ship retains normal Samus handler");
        AssertTrue(TimerSpritesPresent(), "Ordinary escape timer remains visible before boarding");
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWholeY(0x0440);
        samus.Presentation.InitializeAnimation();
        var timeBeforeBoarding = Time();
        runtime.StepFrame(SnesButton.Down);
        AssertEqual(GunshipFrameEvent.EntryStarted, runtime.Enemies.LastEnemyFrame.Gunship, "Down starts actual Landing Site boarding");
        AssertEqual(timeBeforeBoarding, Time(), "Boarding handler stops the timer on its installation frame");
        AssertTrue(!TimerSpritesPresent(), "Boarding removes the timer draws immediately");
        bool sawDustUpload = false;
        for (int frame = 0; frame < 500; frame++)
        {
            runtime.StepFrame(0);
            AssertTrue(runtime.EscapeTimer.IsActive, "Gunship preserves native active timer status");
            AssertEqual(timeBeforeBoarding, Time(), "Gunship handler omits timer countdown");
            AssertTrue(!TimerSpritesPresent(), "No timer sprites reference repurposed dust tiles");
            if (GunshipPartState.Of(top).Function == GunshipFunction.FireUpEngines)
            {
                sawDustUpload = true;
                break;
            }
        }
        AssertTrue(sawDustUpload, "Confirmation reaches the actual liftoff dust upload");
        runtime.StepFrame(0); // NMI consumes the final queued dust tile transfer.
        AssertTrue(!runtime.Vram.Bytes.Slice(0xfc00, 0x400).SequenceEqual(timerTilesBefore), "Liftoff actually repurposes the timer tiles");
        AssertTrue(!TimerSpritesPresent(), "No stale timer draw remains after dust artwork reaches VRAM");
        AssertEqual(timeBeforeBoarding, Time(), "Liftoff keeps the timer suspended");
        Console.WriteLine("Gunship escape timer: normal timer draws, same-frame boarding suppression, preserved active status and no timer sprites through liftoff dust upload passed.");
        return 0;

        (byte, byte, byte) Time() => (runtime.EscapeTimer.Minutes.Raw, runtime.EscapeTimer.Seconds.Raw, runtime.EscapeTimer.Centiseconds.Raw);
        bool TimerSpritesPresent() => runtime.Sprites.Draws.Any(draw => draw.Source.Kind == SpriteSourceKind.EscapeTimer);
    }

    /// <summary>Runs the gunship boarding escape-timer reproduction.</summary>
    [VerificationCommand("--gunship-escape-timer")]
    internal static int GunshipEscapeTimerCommand()
    {
        return VerifyGunshipEscapeTimer();
    }
}
