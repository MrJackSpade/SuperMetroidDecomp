using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.TestSupport.ScenarioFixtures;

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

    /// <summary>Enemy slot selecting the left or right shutter in the fixture room.</summary>
    internal int SlotIndex { get; }
    /// <summary>Whether the sequence begins one block pair away and moves onto the platform.</summary>
    internal bool Approach { get; }
    /// <summary>Update spacing between bomb presses before the sequence stops bombing.</summary>
    internal int Interval { get; }
    /// <summary>Update at which Samus first rolls away from the shutter.</summary>
    internal int RollAt { get; }
    /// <summary>Number of updates spent moving away, then the same number moving back.</summary>
    internal int Duration { get; }
    /// <summary>Initialized retail room fixture stepped by this controller sequence.</summary>
    internal SuperMetroidRuntime Runtime { get; }
    /// <summary>Samus state owned by the scenario runtime.</summary>
    internal SamusState Samus => Runtime.Player.IntroducedSamus;
    /// <summary>Shutter enemy tracked for position comparison.</summary>
    internal RoomEnemySlot Platform => Runtime.Room.LoadedScope.Enemies.SlotTable.Slots[SlotIndex];

    /// <summary>Places morphed, bomb-equipped Samus on or beside the selected shutter before the recorded sequence.</summary>
    /// <param name="runtime">A new invincible runtime bound to installed presentation assets.</param>
    /// <param name="slotIndex">Enemy slot identifying the shutter platform; slot zero uses the left-shutter approach direction.</param>
    /// <param name="approach">Whether to start 32 pixels from the platform and move toward it for the first twelve scenario updates.</param>
    /// <param name="interval">Positive update interval between bomb-button inputs, which stop at update 220.</param>
    /// <param name="rollAt">Update index at which movement away from the shutter begins.</param>
    /// <param name="duration">Updates spent rolling away, followed by the same number rolling back toward the shutter.</param>
    private ShutterBombArcScenario(SuperMetroidRuntime runtime, int slotIndex, bool approach, int interval, int rollAt, int duration)
    {
        (SlotIndex, Approach, Interval, RollAt, Duration) = (slotIndex, approach, interval, rollAt, duration);
        Runtime = runtime;
        Runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        Runtime.RoomLoader.InitializeStartingCeresRoom();
        Runtime.RoomLoader.InitializeCeresStartSamus();
        Runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.XrayScope);
        var samus = Samus;
        var platform = Platform;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.MorphBallGroundRightPose;
        samus.EquippedItems |= SamusEquipmentFlags.Bombs;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Position = samus.Position.WithWhole(new((ushort)(platform.Position.X.Whole - (approach ? Toward * 32 : 0)), (ushort)(platform.Position.Y.Whole - platform.Radius.Y - samus.Kinematics.Radius.Y)));
        Runtime.StepFrame(0, new RuntimeFrameRequest());
    }

    /// <summary>The reported case: left shutter, starting on it, bombing every 8 frames, a 4-frame roll at 75.</summary>
    internal static ShutterBombArcScenario Reproduced(SuperMetroidRuntime runtime) => new(runtime, 0, false, 8, 75, 4);

    /// <summary>Builds a configured shutter approach and bomb sequence for the native comparison fixture.</summary>
    internal static ShutterBombArcScenario Create(SuperMetroidRuntime runtime, int slotIndex, bool approach, int interval, int rollAt, int duration) =>
        new(runtime, slotIndex, approach, interval, rollAt, duration);

    /// <summary>Horizontal input sign that moves Samus toward the selected shutter.</summary>
    private int Toward => SlotIndex == 0 ? -1 : 1;

    /// <summary>Controller buttons for <paramref name="frame"/>: periodic bombs, an approach, then roll off and back.</summary>
    internal SnesButton Input(int frame)
    {
        SnesButton input = frame < 220 && frame % Interval == 0 ? Runtime.Options.ControllerBindings.Shoot : SnesButton.None;
        int direction = Approach && frame < 12 ? Toward : 0;
        if (frame >= RollAt && frame < RollAt + Duration) direction = -Toward;
        if (frame >= RollAt + Duration && frame < RollAt + Duration * 2) direction = Toward;
        if (direction != 0) input |= direction < 0 ? SnesButton.Left : SnesButton.Right;
        return input;
    }

    /// <summary>One native-comparison row after <paramref name="frame"/> has run.</summary>
    internal string ArcRow(int frame) =>
        $"{frame},{Samus.Kinematics.Position.X.RawValue},{Samus.Kinematics.Position.Y.RawValue},{Samus.Kinematics.VerticalSpeedFixed},{Samus.BombJumpDirection},{Platform.Position.Y.Whole},{Platform.Position.Y.Fraction}";
}
