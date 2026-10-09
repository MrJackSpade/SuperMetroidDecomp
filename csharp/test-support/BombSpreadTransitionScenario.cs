using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Issue #415: earning a charged bomb spread by morphing in the air, either while turning around
/// (the aerial route) or out of a charged walljump (the wall route). Only room clearing, equipment
/// and the start position are set up; pose, charge and animation are earned through input.
/// The verifier asserts the route's contract; the debug runner writes the same rows as a trace.
/// </summary>
internal sealed class BombSpreadTransitionScenario
{
    /// <summary>Column names emitted by <see cref="Row"/> for each per-frame trace record.</summary>
    internal const string TraceHeader = "left,delay,frame,input,pose,x,xsub,y,ysub,charge,spread,bombs";

    /// <summary>Selects the left-facing variant of the selected bomb-spread route.</summary>
    internal bool Left { get; }
    /// <summary>Selects the wall-jump route instead of earning the spread during an aerial turn-around.</summary>
    internal bool WallRoute { get; }
    /// <summary>Selected timing variant, including the wall-route late-success and negative-control cases.</summary>
    internal int TimingCase { get; }
    /// <summary>Frames the timed release or morph is late; wall cases 10..13 are the 40..43 controls.</summary>
    internal int Delay { get; }
    /// <summary>Runtime initialized with the cleared Landing Site fixture used by this scenario.</summary>
    internal SuperMetroidRuntime Runtime { get; }
    /// <summary>Samus instance whose input-earned pose, charge, and projectile state the scenario records.</summary>
    internal SamusState Samus => Runtime.Samus!;
    /// <summary>Number of frames required to run the selected route through its observation window.</summary>
    internal int FrameCount => WallRoute ? 300 : 260;

    /// <summary>Timing cases per direction: ten delays, plus the wall route's late-success and two negative controls.</summary>
    internal static int TimingCaseCount(bool wallRoute) => wallRoute ? 14 : 10;

    /// <summary>Initializes the test room and Samus state while leaving movement and bomb-spread charge to controller input.</summary>
    /// <param name="runtime">Runtime whose Landing Site room is prepared for the route.</param>
    /// <param name="bus">Address space used to initialize Samus collision radii and animation.</param>
    /// <param name="left">Selects the left-facing variant of the route.</param>
    /// <param name="timingCase">Selects the release or morph delay, including route-specific control cases.</param>
    /// <param name="wallRoute">Selects the charged wall-jump route when true, otherwise the aerial route.</param>
    internal BombSpreadTransitionScenario(SuperMetroidRuntime runtime, ISnesAddressSpace bus, bool left, int timingCase, bool wallRoute)
    {
        (Runtime, Left, TimingCase, WallRoute) = (runtime, left, timingCase, wallRoute);
        Delay = timingCase < 10 ? timingCase : timingCase + 30;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var level = runtime.LevelData!;
        // Match the grounded technique clearing, extending it upward for the jump.
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.WidthInBlocks + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 || (wallRoute && x == (left ? 29 : 33)) ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = Samus;
        samus.InputLocked = false;
        samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
        samus.XPosition = 512;
        samus.YPosition = 490;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
    }

    /// <summary>Controller word for <paramref name="frame"/>.</summary>
    internal ushort Input(int frame)
    {
        var bindings = Runtime.ControllerBindings;
        ushort input = bindings.Shoot;
        if (!WallRoute)
        {
            if (frame is >= 70 and < 125) input |= bindings.Jump;
            if (frame >= 76 && frame < 105 && frame != 78 + Delay)
                input |= (ushort)SnesButton.Down;
            if (frame is >= 78 and < 125) input |= (ushort)(Left ? SnesButton.Right : SnesButton.Left);
            return input;
        }
        int morphDelay = TimingCase >= 12 ? 0 : Delay;
        input = TimingCase == 12 || frame < 87 || frame >= 94 + morphDelay ? bindings.Shoot : (ushort)0;
        if (frame is >= 70 and < 86 or >= 89 and < 200) input |= bindings.Jump;
        if (frame is >= 68 and < 87) input |= (ushort)(Left ? SnesButton.Left : SnesButton.Right);
        if (frame is >= 87 and < 140) input |= (ushort)(Left ? SnesButton.Right : SnesButton.Left);
        if (TimingCase == 13 && frame == 93)
        {
            input &= unchecked((ushort)~(SnesButton.Left | SnesButton.Right));
            input |= (ushort)(Left ? SnesButton.Left : SnesButton.Right);
        }
        if (frame >= 94 + morphDelay && frame < 115 + morphDelay) input |= (ushort)SnesButton.Down;
        return input;
    }

    /// <summary>Samus row after <paramref name="frame"/> ran with <paramref name="input"/>.</summary>
    internal string Row(int frame, ushort input) =>
        $"{(Left ? 1 : 0)},{Delay},{frame},{input:X4},{Samus.Pose:X4},{Samus.XPosition:X4},{Samus.Kinematics.XSubposition:X4},{Samus.YPosition:X4},{Samus.Kinematics.YSubposition:X4},{Samus.ProjectileFlareCounter:X4},{Samus.BombSpreadChargeTimeoutCounter:X4},{Runtime.BombProjectiles.BombCounter:X4}";

    /// <summary>Whether <paramref name="frame"/> also records every bomb slot: the release window onward.</summary>
    internal bool RecordsBombs(int frame) => WallRoute ? frame >= 115 : Delay == 6 && frame >= 105;

    /// <summary>One row per bomb slot. Inactive native slots retain scratch words that are overwritten
    /// at allocation, so an inactive slot records only its ownership, not dead bytes.</summary>
    internal IEnumerable<string> BombRows()
    {
        for (int slotIndex = 0; slotIndex < SamusBombProjectileSystem.SlotCount; slotIndex++)
        {
            var slot = Runtime.BombProjectiles.Slots[slotIndex];
            string bomb = $"bomb,{slotIndex},{(slot.IsActive ? 1 : 0)}";
            if (slot.IsActive)
                bomb += $",{slot.Type:X4},{slot.XPosition:X4},{slot.XSubposition:X4},{slot.YPosition:X4},{slot.YSubposition:X4},{slot.BombSpreadXVelocity:X4},{slot.BombSpreadYVelocity:X4},{slot.BombSpreadYSubvelocity:X4},{slot.BombTimer:X4},{slot.XRadius:X4},{slot.YRadius:X4},{slot.InstructionPointer:X4},{slot.InstructionTimer:X4},{slot.SpritemapPointer:X4}";
            yield return bomb;
        }
    }
}
