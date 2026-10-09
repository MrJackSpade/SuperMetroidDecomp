using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Issue #347: a morphed Samus bombing on an X-ray-scope-room shutter while rolling off and back.
/// The verifier checks the reproduced case against its native trajectory; the debug runner
/// exports that case's native seed and sweeps the other approaches.
/// </summary>
internal sealed class ShutterBombArcScenario
{
    /// <summary>Checked-in native comparison for the reproduced case.</summary>
    internal const string FixtureDirectory = "csharp/test-fixtures/issue-347-repeated-bombs";
    /// <summary>First compared frame: the bomb ascent seeded into the native comparison.</summary>
    internal const int FirstArcFrame = 117;
    /// <summary>Frames the scenario runs; bombs stop at frame 220.</summary>
    internal const int FrameCount = 260;

    /// <summary>
    /// Gets the enemy-slot index used to locate the selected shutter platform.
    /// </summary>
    internal int SlotIndex { get; }

    /// <summary>
    /// Gets whether the scenario begins with Samus approaching the platform during its first twelve updates.
    /// </summary>
    internal bool Approach { get; }

    /// <summary>
    /// Gets the number of updates between successive bomb-button inputs before firing stops.
    /// </summary>
    internal int Interval { get; }

    /// <summary>
    /// Gets the update at which Samus begins rolling away from the shutter.
    /// </summary>
    internal int RollAt { get; }

    /// <summary>
    /// Gets the number of updates spent rolling away and then rolling back toward the shutter.
    /// </summary>
    internal int Duration { get; }

    /// <summary>
    /// Gets the initialized runtime that advances the scenario and supplies its controller bindings.
    /// </summary>
    internal SuperMetroidRuntime Runtime { get; }

    /// <summary>
    /// Gets Samus from the scenario runtime after the fixture initializes her in morph-ball pose.
    /// </summary>
    internal SamusState Samus => Runtime.Samus!;

    /// <summary>
    /// Gets the shutter enemy slot selected by <see cref="SlotIndex"/>.
    /// </summary>
    internal RoomEnemySlot Platform => Runtime.Enemies.Slots[SlotIndex];

    /// <param name="runtime">A new invincible runtime bound to installed presentation assets.</param>
    /// <param name="bus">Address space used to initialize Samus collision geometry and animation.</param>
    /// <param name="slotIndex">Enemy slot identifying the shutter platform; slot zero uses the left-shutter approach direction.</param>
    /// <param name="approach">Whether to start 32 pixels from the platform and move toward it for the first twelve scenario updates.</param>
    /// <param name="interval">Positive update interval between bomb-button inputs, which stop at update 220.</param>
    /// <param name="rollAt">Update index at which movement away from the shutter begins.</param>
    /// <param name="duration">Updates spent rolling away, followed by the same number rolling back toward the shutter.</param>
    private ShutterBombArcScenario(SuperMetroidRuntime runtime, ISnesAddressSpace bus, int slotIndex, bool approach, int interval, int rollAt, int duration)
    {
        (SlotIndex, Approach, Interval, RollAt, Duration) = (slotIndex, approach, interval, rollAt, duration);
        Runtime = runtime;
        Runtime.InitializeHud(HudSnapshot.CeresDebug);
        Runtime.InitializeStartingCeresRoom();
        Runtime.InitializeCeresStartSamus();
        Runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.BrinstarShutterRoom);
        var samus = Samus;
        var platform = Platform;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = (ushort)(platform.XPosition - (approach ? Toward * 32 : 0));
        samus.YPosition = (ushort)(platform.YPosition - platform.YRadius - samus.Kinematics.YRadius);
        Runtime.StepFrame(0);
    }

    /// <summary>The reported case: left shutter, starting on it, bombing every 8 frames, a 4-frame roll at 75.</summary>
    internal static ShutterBombArcScenario Reproduced(SuperMetroidRuntime runtime, ISnesAddressSpace bus) => new(runtime, bus, 0, false, 8, 75, 4);

    /// <summary>
    /// Creates a shutter-bomb trajectory scenario with the requested platform and input/movement timing.
    /// </summary>
    /// <param name="runtime">A new invincible runtime bound to installed presentation assets.</param>
    /// <param name="bus">Address space used to initialize Samus collision geometry and animation.</param>
    /// <param name="slotIndex">Enemy slot identifying the shutter platform; slot zero uses the left-shutter approach direction.</param>
    /// <param name="approach">Whether Samus starts 32 pixels away and approaches during the first twelve updates.</param>
    /// <param name="interval">Positive update interval between bomb inputs, which stop at update 220.</param>
    /// <param name="rollAt">Update index at which Samus begins moving away from the shutter.</param>
    /// <param name="duration">Updates spent rolling away, followed by the same number rolling back toward the shutter.</param>
    /// <returns>The initialized scenario for the selected shutter and timing parameters.</returns>
    internal static ShutterBombArcScenario Create(SuperMetroidRuntime runtime, ISnesAddressSpace bus, int slotIndex, bool approach, int interval, int rollAt, int duration) =>
        new(runtime, bus, slotIndex, approach, interval, rollAt, duration);

    /// <summary>
    /// Gets the horizontal sign pointing from Samus's starting position toward the selected shutter slot.
    /// </summary>
    private int Toward => SlotIndex == 0 ? -1 : 1;

    /// <summary>Controller word for <paramref name="frame"/>: periodic bombs, an approach, then roll off and back.</summary>
    internal ushort Input(int frame)
    {
        ushort input = frame < 220 && frame % Interval == 0 ? Runtime.ControllerBindings.Shoot : (ushort)0;
        int direction = Approach && frame < 12 ? Toward : 0;
        if (frame >= RollAt && frame < RollAt + Duration) direction = -Toward;
        if (frame >= RollAt + Duration && frame < RollAt + Duration * 2) direction = Toward;
        if (direction != 0) input |= (ushort)(direction < 0 ? SnesButton.Left : SnesButton.Right);
        return input;
    }

    /// <summary>One native-comparison row after <paramref name="frame"/> has run.</summary>
    internal string ArcRow(int frame) =>
        $"{frame},{Samus.Kinematics.XFixed},{Samus.Kinematics.YFixed},{Samus.Kinematics.VerticalSpeedFixed},{Samus.BombJumpDirection},{Platform.YPosition},{Platform.YSubposition}";
}
