using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>
/// Issue #1258: an unequipped Samus standing in shallow water holds jump for 60 frames.
/// The verifier compares every frame with the checked-in native trace; the debug runner exports
/// the room and takeoff seed that trace was captured from.
/// </summary>
internal sealed class ShallowWaterJumpScenario
{
    /// <summary>Names the seven columns in the native and port jump traces.</summary>
    internal const string TraceHeader = "frame,pose,y,yspeed,ydir,radius,medium";
    /// <summary>Covers takeoff through the recorded shallow-water jump arc.</summary>
    internal const int JumpFrameCount = 60;

    /// <summary>Owns the initialized retail-room runtime advanced by this scenario.</summary>
    internal SuperMetroidRuntime Runtime { get; }
    /// <summary>Exposes the runtime's Samus state for takeoff and per-frame capture.</summary>
    internal SamusState Samus => Runtime.Player.IntroducedSamus;

    /// <summary>Places Samus standing in the water and settles two frames, ready for takeoff.</summary>
    internal ShallowWaterJumpScenario(SuperMetroidRuntime runtime)
    {
        Runtime = runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.BelowSpazer, camera: new Position16<RoomPixels>(0, 256));
        var samus = Samus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(104, 427));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xffff);
        samus.EquippedItems = 0;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.StepFrame(0, new RuntimeFrameRequest());
        runtime.StepFrame(0, new RuntimeFrameRequest());
    }

    /// <summary>Native WRAM seed words for the takeoff state, in the capture tool's order.</summary>
    internal string TakeoffSeed() =>
        $"{Samus.Position.X.Whole:X4} {Samus.Kinematics.Position.X.Fraction:X4} {Samus.Position.Y.Whole:X4} {Samus.Kinematics.Position.Y.Fraction:X4} {(int)Samus.Pose:X4} {Samus.AnimationFrame:X4} {Samus.AnimationFrameTimer.RawValue:X4} {Samus.AnimationFrameBuffer:X4} {Samus.LiquidPhysics.FxYPosition:X4} {Samus.LiquidPhysics.LiquidOptions:X4} {(int)Samus.LiquidPhysics.LiquidPhysicsType:X4}\n";

    /// <summary>Holds jump for <see cref="JumpFrameCount"/> frames; returns the header and one row per frame, and the apex.</summary>
    internal JumpTrace Jump()
    {
        var rows = new List<string> { TraceHeader };
        uint apex = Samus.Kinematics.Position.Y.RawValue;
        for (int frame = 0; frame < JumpFrameCount; frame++)
        {
            Runtime.StepFrame(SnesButton.A, new RuntimeFrameRequest());
            apex = Math.Min(apex, Samus.Kinematics.Position.Y.RawValue);
            rows.Add($"{frame},{(int)Samus.Pose:X2},{Samus.Kinematics.Position.Y.RawValue:X8},{Samus.Kinematics.VerticalSpeedFixed:X8},{(int)Samus.Kinematics.YDirection},{Samus.Kinematics.Radius.Y},{(int)Samus.LiquidPhysics.LiquidPhysicsType}");
        }
        return new JumpTrace(rows, apex);
    }
}
