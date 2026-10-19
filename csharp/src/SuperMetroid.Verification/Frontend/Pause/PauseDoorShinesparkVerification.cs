using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
/// <summary>Checks open-door contact during pause fade while Samus remains in a horizontal shinespark.</summary>
internal static class PauseDoorShinesparkVerification
{
    /// <summary>Confirms normal and terminal fade frames publish the door transition in native order without extra source-room movement.</summary>
    internal static void VerifyPauseDoorShinespark()
    {
        foreach (var scenario in new[] { (Brightening: false, Terminal: false), (Brightening: false, Terminal: true), (Brightening: true, Terminal: false), (Brightening: true, Terminal: true) })
        {
            var installation=RepositoryInstallation.Installation;
            var bus=installation.OpenRuntimeAddressSpace();
            var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
            InstalledFixtures.BindGame(game);
            game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
            var runtime=game.Session.runtime;
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.System.SetEvent(EventNumber.ZebesAwake);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Parlor);
            foreach (var enemy in runtime.Enemies.SlotTable.Slots) enemy.Clear();
            var words = new ushort[32 * 16];
            var bts = new byte[words.Length];
            for (int row = 0; row < 16; row++)
            {
                words[row * 32 + 10] = 0x9000;
                bts[row * 32 + 10] = 3;
                words[row * 32 + 12] = 0x8000;
            }
            var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 16), new RoomLevelPlanes(words, bts, new ushort[512]), new byte[8], doors: RoomDoorListDefinitions.ForListPointer(0xdad5));
            runtime.Room.Adopt((runtime.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.")) with { LevelData = level });
            var samus = runtime.Player.IntroducedSamus;
            samus.InputLocked = false;
            samus.Health = 999;
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Presentation.InitializeAnimation();
            samus.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter));
            samus.Shinespark.BeginWindup(samus);
            samus.Shinespark.BeginDirectionalLaunch(samus, SamusPoseId.ShinesparkHorizontalRightPose);
            samus.CommitPoseHistory();
            samus.Position = samus.Position.WithWhole(new(128, 128));
            game.GameState = SuperMetroidGameState.MainGameplay;
            game.Step(SnesButton.Start);
            AssertEqual(SuperMetroidGameState.PausingDarkening, game.GameState, "Start begins darkening before reaching the open door");
            AssertTrue(!runtime.RoomLoader.HasPendingDoorTransition, "initial Start frame is short of the door");
            if (scenario.Brightening)
                game.GameState = SuperMetroidGameState.Unpausing;
            game.Session.pauseBrightness = (byte)(scenario.Brightening ? (scenario.Terminal ? 14 : 5) : (scenario.Terminal ? 1 : 15));
            game.Session.screenFade.Counter = 0;
            // Put the leading edge immediately before the door for the next real mover call.
            samus.Position = samus.Position.WithWholeX((ushort)(160 - samus.Kinematics.Radius.X - 1));
            game.Step(0);
            AssertTrue(runtime.RoomLoader.HasPendingDoorTransition, "horizontal shinespark actually touches the open door during the fade");
            AssertEqual(ShinesparkPhase.Horizontal, samus.Shinespark.Phase, "door contact does not crash the shinespark");
            var expected = scenario.Terminal
                ? scenario.Brightening ? SuperMetroidGameState.MainGameplay : SuperMetroidGameState.LoadingNextRoomA
                : SuperMetroidGameState.HitDoorBlock;
            AssertEqual(expected, game.GameState, "door publication retains native fade completion ordering");
            if (!scenario.Brightening || !scenario.Terminal)
            {
                ushort contactX = samus.Position.X.Whole;
                game.Step(0);
                AssertEqual(SuperMetroidGameState.LoadingNextRoomB, game.GameState, "published door state starts the transition coroutine");
                AssertEqual(contactX, samus.Position.X.Whole, "no additional source-room shinespark movement before transition");
            }
        }
        Console.WriteLine("Pause-door shinespark: door hits during both fades preserve native normal/terminal state ordering and start transitions without extra source movement.");
    }

    /// <summary>Runs the pause-fade shinespark door contact reproduction.</summary>
    [VerificationCommand("--pause-door-shinespark")]
    internal static int PauseDoorShinesparkCommand()
    {
        Suite(nameof(VerifyPauseDoorShinespark), () => VerifyPauseDoorShinespark());
        return 0;
    }
}
