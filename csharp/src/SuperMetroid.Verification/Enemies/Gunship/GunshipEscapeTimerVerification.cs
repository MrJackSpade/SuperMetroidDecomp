using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
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
using SuperMetroid.Core.Escape;
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
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0x0400, 0x0400));
        var samus = runtime.Player.IntroducedSamus;
        var top = runtime.Enemies.SlotTable.Slots.First(slot => slot.NativeHeader.InitializationAiPointer == 0xa644);
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(top.Position.X.Whole, (ushort)(top.Position.Y.Whole - 30)));
        SamusPresentation.InitializeAnimation(samus);
        samus.Presentation.PrimeGraphics(samus);
        runtime.EscapeTimer.SelectRunningStatus(EscapeTimerState.RunningInPlace);
        runtime.EscapeTimer.SetTime(PackedBcd.FromDecimal(2), PackedBcd.FromDecimal(59), PackedBcd.FromDecimal(50));
        runtime.ShowEscapeTimerSpriteTiles();
        runtime.StepFrame(0);
        var timerTilesBefore = new ObjCharacterTablePage().Page(runtime.Enemies.Room.Session.SpriteCharacters, runtime.Artwork)
            .Characters.Slice(0x3c00, 0x400).ToArray();
        AssertTrue(samus.Control.Mode.Beta != SamusBetaHandler.Gunship, "Idle ship retains normal Samus handler");
        AssertTrue(TimerSpritesPresent(), "Ordinary escape timer remains visible before boarding");
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWholeY(0x0440);
        SamusPresentation.InitializeAnimation(samus);
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
        // The final dust page shows on the update that recorded it, over the characters the timer drew from (#1406).
        byte[] shownObjects = new ObjCharacterTablePage().Page(runtime.Enemies.Room.Session.SpriteCharacters, runtime.Artwork).Characters.ToArray();
        AssertTrue(!shownObjects.AsSpan(0x3c00, 0x400).SequenceEqual(timerTilesBefore), "Liftoff actually repurposes the timer tiles");
        AssertTrue(!TimerSpritesPresent(), "No stale timer draw remains once the dust page shows");
        AssertEqual(timeBeforeBoarding, Time(), "Liftoff keeps the timer suspended");
        Console.WriteLine("Gunship escape timer: normal timer draws, same-frame boarding suppression, preserved active status and no timer sprites through liftoff dust upload passed.");
        return 0;

        TimeResult Time() => new TimeResult(runtime.EscapeTimer.Minutes.Raw, runtime.EscapeTimer.Seconds.Raw, runtime.EscapeTimer.Centiseconds.Raw);
        bool TimerSpritesPresent() => runtime.Sprites.Draws.Any(draw => draw.Source.Kind == SpriteSourceKind.EscapeTimer);
    }

    /// <summary>Runs the gunship boarding escape-timer reproduction.</summary>
    [VerificationCommand("--gunship-escape-timer")]
    internal static int GunshipEscapeTimerCommand()
    {
        return VerifyGunshipEscapeTimer();
    }
}
