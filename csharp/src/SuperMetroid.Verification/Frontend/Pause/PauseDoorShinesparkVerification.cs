using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
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
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend.Pause;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
/// <summary>Checks open-door contact during pause fade while Samus remains in a horizontal shinespark.</summary>
internal static class PauseDoorShinesparkVerification
{
    /// <summary>A door block showing block zero: the column Samus sparks into.</summary>
    private static readonly RoomTerrainBlock Door = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.DoorBlock);

    /// <summary>A plain solid block: the wall behind the door column.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>Where the door column leads: the Tourian elevator room's door into its save room.</summary>
    private static readonly DoorBlockExit TourianSaveDoor = DoorDefinitions.Resolve(0xdad5, 3);

    /// <summary>Confirms normal and terminal fade frames publish the door transition in native order without extra source-room movement.</summary>
    internal static void VerifyPauseDoorShinespark()
    {
        foreach (var scenario in new[] { new VerifyPauseDoorShinesparkCase(Brightening:false, Terminal:false), new VerifyPauseDoorShinesparkCase(Brightening:false, Terminal:true), new VerifyPauseDoorShinesparkCase(Brightening:true, Terminal:false), new VerifyPauseDoorShinesparkCase(Brightening:true, Terminal:true) })
        {
            var game=SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
            InstalledFixtures.BindGame(game);
            game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
            var runtime = game.Session.GameplayRuntime;
            runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
            runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
            runtime.System.SetEvent(StoryEvent.ZebesAwake);
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Parlor);
            foreach (var enemy in runtime.Enemies.Places.InUpdateOrder) enemy.Clear();
            var words = new RoomTerrainBlock[32 * 16];
            var bts = new RoomBlockBehavior[words.Length];
            var doorBlocks = new List<RoomDoorBlock>();
            for (int row = 0; row < 16; row++)
            {
                words[row * 32 + 10] = Door;
                doorBlocks.Add(new RoomDoorBlock(new(10, row), TourianSaveDoor));
                words[row * 32 + 12] = Solid;
            }
            var level = Harness.VerificationHelpersVerification.CreateRoom(new Extent16<RoomBlocks>(32, 16), words, bts,
            doorBlocks: doorBlocks);
            runtime.Room.Adopt((runtime.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.")) with { LevelData = level });
            var samus = runtime.Player.Samus;
            samus.Control.Unlock();
            samus.Health = 999;
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.Shinespark.TryStoreFromSpeedBooster(SamusSpeedBoostCounter.AtFullStage);
            samus.Shinespark.BeginWindup(samus);
            samus.Shinespark.BeginDirectionalLaunch(samus, SamusPoseId.ShinesparkHorizontalRightPose);
            samus.CommitPoseHistory();
            samus.Position = samus.Position.WithWhole(new(128, 128));
            game.Session.Enter(SuperMetroidGameState.MainGameplay);
            game.Step(ControllerButton.Start);
            AssertTrue(game.IsAt(PauseStep.DarkeningGameplay), "Start begins darkening before reaching the open door");
            AssertTrue(runtime.EnteredDoor is null, "initial Start frame is short of the door");
            if (scenario.Brightening)
                game.Session.Enter(PauseStep.BrighteningGameplay);
            // The pause's fade keeps its step delay with no update left to wait, so the next update steps.
            game.Session.screenFade.Show(DisplayBrightness.Black.Brighter(scenario.Brightening ? (scenario.Terminal ? 14 : 5) : (scenario.Terminal ? 1 : 15)));
            game.Session.screenFade.SetCadence(SuperMetroid.Core.Frontend.Pause.PauseFadeTiming.WaitBeforeEachStep, 0);
            // Put the leading edge immediately before the door for the next real mover call.
            samus.Position = samus.Position.WithWholeX((ushort)(160 - samus.Kinematics.Radius.X - 1));
            game.Step(0);
            AssertTrue(runtime.EnteredDoor is not null, "horizontal shinespark actually touches the open door during the fade");
            AssertEqual(ShinesparkPhase.Horizontal, samus.Shinespark.Phase, "door contact does not crash the shinespark");
            bool publishedExpected = scenario.Terminal
                ? scenario.Brightening ? game.GameState == SuperMetroidGameState.MainGameplay : game.IsAt(DoorTransitionStep.BeginningTransition)
                : game.IsAt(DoorTransitionStep.EnteringDoor);
            AssertTrue(publishedExpected, $"door publication retains native fade completion ordering; ended in {game.GameState}");
            if (!scenario.Brightening || !scenario.Terminal)
            {
                ushort contactX = samus.Position.X.Whole;
                game.Step(0);
                AssertTrue(game.IsAt(DoorTransitionStep.Transitioning), "published door state starts the transition coroutine");
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
