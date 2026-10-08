using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Issue #1258: an unequipped Samus standing in shallow water holds jump for 60 frames.
/// The verifier compares every frame with the checked-in native trace; the debug runner exports
/// the room and takeoff seed that trace was captured from.
/// </summary>
internal sealed class ShallowWaterJumpScenario
{
    internal const string TraceHeader = "frame,pose,y,yspeed,ydir,radius,medium";
    internal const int JumpFrameCount = 60;

    internal SuperMetroidRuntime Runtime { get; }
    internal SamusState Samus => Runtime.Samus!;

    /// <summary>Places Samus standing in the water and settles two frames, ready for takeoff.</summary>
    internal ShallowWaterJumpScenario(SuperMetroidRuntime runtime, ISnesAddressSpace bus)
    {
        Runtime = runtime;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.BrinstarShallowWaterRoom, cameraY: 256);
        var samus = Samus;
        samus.InputLocked = false;
        samus.Pose = (byte)SamusPoseId.FacingRightNormalPose;
        samus.XPosition = 104; samus.YPosition = 427;
        samus.Kinematics.YSubposition = 0xffff;
        samus.EquippedItems = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.StepFrame(0);
        runtime.StepFrame(0);
    }

    /// <summary>Native WRAM seed words for the takeoff state, in the capture tool's order.</summary>
    internal string TakeoffSeed() =>
        $"{Samus.XPosition:X4} {Samus.Kinematics.XSubposition:X4} {Samus.YPosition:X4} {Samus.Kinematics.YSubposition:X4} {Samus.Pose:X4} {Samus.AnimationFrame:X4} {Samus.AnimationFrameTimer:X4} {Samus.AnimationFrameBuffer:X4} {Samus.LiquidPhysics.FxYPosition:X4} {Samus.LiquidPhysics.LiquidOptions:X4} {Samus.LiquidPhysics.LiquidPhysicsType:X4}\n";

    /// <summary>Holds jump for <see cref="JumpFrameCount"/> frames; returns the header and one row per frame, and the apex.</summary>
    internal (List<string> Rows, uint Apex) Jump()
    {
        var rows = new List<string> { TraceHeader };
        uint apex = Samus.Kinematics.YFixed;
        for (int frame = 0; frame < JumpFrameCount; frame++)
        {
            Runtime.StepFrame((ushort)SnesButton.A);
            apex = Math.Min(apex, Samus.Kinematics.YFixed);
            rows.Add($"{frame},{Samus.Pose:X2},{Samus.Kinematics.YFixed:X8},{Samus.Kinematics.VerticalSpeedFixed:X8},{Samus.Kinematics.YDirection},{Samus.Kinematics.YRadius},{Samus.LiquidPhysics.LiquidPhysicsType}");
        }
        return (rows, apex);
    }
}
