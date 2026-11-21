using SuperMetroid.TestSupport.SamusNativeCodecs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus;

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
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.BelowSpazer, camera: new Position16<RoomPixels>(0, 256));
        var samus = Samus;
        samus.Control.Unlock();
        samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose);
        samus.Position = samus.Position.WithWhole(new(104, 427));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(PixelFraction.Last);
        samus.EquippedItems = 0;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.StepFrame(0, new RuntimeFrameRequest());
        runtime.StepFrame(0, new RuntimeFrameRequest());
    }

    /// <summary>
    /// Native WRAM seed words for the takeoff state, in the capture tool's order; the liquid options word is the room water's,
    /// which Samus's liquid physics copies.
    /// </summary>
    internal string TakeoffSeed() =>
        $"{Samus.Position.X.Whole:X4} {Samus.Kinematics.Position.X.Fraction:X4} {Samus.Position.Y.Whole:X4} {Samus.Kinematics.Position.Y.Fraction:X4} {(int)SamusPoseNumbers.Number(Samus.Pose):X4} {Samus.AnimationFrame:X4} {Samus.AnimationFrameTimer.RemainingFrames:X4} {Samus.ExtraAnimationDelay:X4} {Samus.LiquidPhysics.WaterSurfaceY ?? ushort.MaxValue:X4} {RoomFxRecordCodec.LiquidOptionsByte(Runtime.Room.LoadedScope.FxOverlay.Liquid):X4} {(int)Samus.LiquidPhysics.MediumLastEntered:X4}\n";

    /// <summary>Holds jump for <see cref="JumpFrameCount"/> frames; returns the header and one row per frame, and the apex.</summary>
    internal JumpTrace Jump()
    {
        var rows = new List<string> { TraceHeader };
        FixedCoordinate apex = Samus.Kinematics.Position.Y;
        for (int frame = 0; frame < JumpFrameCount; frame++)
        {
            Runtime.StepFrame(ControllerButton.A, new RuntimeFrameRequest());
            FixedCoordinate y = Samus.Kinematics.Position.Y;
            if (y.Whole < apex.Whole || (y.Whole == apex.Whole && y.Fraction < apex.Fraction))
                apex = y;
            rows.Add($"{frame},{(int)SamusPoseNumbers.Number(Samus.Pose):X2},{Samus.Kinematics.Position.Y.Whole:X4}{Samus.Kinematics.Position.Y.Fraction:X4},{Samus.Kinematics.VerticalVelocity.Speed.RawValue:X8},{SamusVerticalDirectionCodec.ToNative(Samus.Kinematics.VerticalVelocity.Direction)},{Samus.Kinematics.Radius.Y},{SamusLiquidMediumCodec.ToNative(Samus.LiquidPhysics.MediumLastEntered)}");
        }
        return new JumpTrace(rows, apex);
    }
}
