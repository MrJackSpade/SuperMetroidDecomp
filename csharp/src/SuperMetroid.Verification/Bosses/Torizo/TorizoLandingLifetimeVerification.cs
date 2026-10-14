using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.Verification.Bosses.Torizo;
/// <summary>Checks the Bomb Torizo landing earthquake through the full frontend.</summary>
internal static class TorizoLandingLifetimeVerification
{
    /// <summary>Advances landing and audio for 40 frames, requiring exactly 32 displaced frames.</summary>
    internal static int VerifyTorizoLandingLifetime()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=GameAssetInstaller.EnsureInstalled(Path.GetFullPath("out/workbook-investigation/crocomire-install"));
        var bus=(installation ?? throw new InvalidOperationException("Verification requires installation.")).OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        InstalledFixtures.BindGame(game);
        game.CreateGameplayRuntime(false);
        var runtime=game.runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(0x9804);
        var body=runtime.Enemies.SlotTable.Slots.First(e=>e.EnemyDefinitionPointer==EnemyDefinitionId.BombTorizo);
        var state=(TorizoEnemyState)(runtime.Enemies.BombTorizoBehavior._torizoState ?? throw new InvalidOperationException("Verification requires runtime.Enemies.BombTorizoBehavior._torizoState."));
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function=TorizoFunction.Active;state.PreInstruction=TorizoPreInstruction.Jump;state.ReturnInstruction=0xbc88;
        state.Velocity=new EightEightVector<RoomPixels>(0,0x400);state.VerticalAcceleration=40;
        body.Health=body.Definition.Health;body.Parameter1=body.Parameter2=0;
        body.Properties=0x2000;body.CurrentInstruction=0xbc80;body.InstructionTimer=NativeCountdown.Start(5);
        body.Position = body.Position.WithWhole(new(128, (ushort)(190-body.Radius.Y)));
        var words=new ushort[256];for(int i=12*16;i<13*16;i++)words[i]=0x8000;
        var level=new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(words, new byte[256], new ushort[256]), new byte[8]);
        runtime.Room.LevelData = level;
        var samus=runtime.Player.IntroducedSamus;samus.Pose=SamusPoseId.FacingRightNormalPose;
        samus.Health=samus.MaxHealth=999;
        samus.PoseMetadata.RefreshCollisionRadii(bus);samus.Presentation.InitializeAnimation(bus);samus.CommitPoseHistory(bus);
        samus.Position = samus.Position.WithWhole(new(240, (ushort)(192-samus.Kinematics.Radius.Y)));samus.InputLocked=false;
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 0));
        game.GameState = SuperMetroidGameState.MainGameplay;
        int shaken=0;
        for(int frame=0;frame<40;frame++)
        {
            var result=game.Step(0);
            // Every rendered earthquake type displaces BG1 or BG2, so only an unapplied frame leaves the default delta.
            var shake=runtime.Enemies.Earthquake.LastRoomShake;bool shakeApplied=shake!=default;if(shakeApplied)shaken++;
            if(frame%8==0 || frame>=30)Console.WriteLine($"frame={frame} actor={body.Position.X.Whole},{body.Position.Y.Whole} hp={body.Health} pre={(int)state.PreInstruction:X4} timer={runtime.Enemies.Earthquake.EarthquakeTimer.RawValue} shake={shake}");
            if(shakeApplied!=(frame<32))throw new InvalidDataException("Full frontend landing-shake lifetime differs from 32 native frames.");
        }
        Console.WriteLine($"shaken={shaken}");
        return 0;
    }
}
