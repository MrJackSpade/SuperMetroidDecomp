using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>
/// Issue #415: earning a charged bomb spread by morphing in the air, either while turning around
/// (the aerial route) or out of a charged walljump (the wall route). Only room clearing, equipment
/// and the start position are set up; pose, charge and animation are earned through input.
/// The verifier asserts the route's contract; the debug runner writes the same rows as a trace.
/// </summary>
internal sealed class BombSpreadTransitionScenario
{
    /// <summary>Column order shared by the verifier and debug-runner input traces.</summary>
    internal const string TraceHeader = "left,delay,frame,input,pose,x,xsub,y,ysub,charge,spread,bombs";

    /// <summary>Whether the scenario begins facing left.</summary>
    internal bool Left { get; }
    /// <summary>Whether the charged walljump route supplies the aerial morph.</summary>
    internal bool WallRoute { get; }
    /// <summary>Index selecting release, morph, or negative-control timing.</summary>
    internal int TimingCase { get; }
    /// <summary>Frames the timed release or morph is late; wall cases 10..13 are the 40..43 controls.</summary>
    internal int Delay { get; }
    /// <summary>Production gameplay runtime stepped by the scenario.</summary>
    internal SuperMetroidRuntime Runtime { get; }
    /// <summary>Samus state owned by the scenario runtime.</summary>
    internal SamusState Samus => Runtime.Player.IntroducedSamus;
    /// <summary>Number of updates recorded for the aerial or walljump route.</summary>
    internal int FrameCount => WallRoute ? 300 : 260;

    /// <summary>Timing cases per direction: ten delays, plus the wall route's late-success and two negative controls.</summary>
    internal static int TimingCaseCount(bool wallRoute) => wallRoute ? 14 : 10;

    /// <summary>Clears a bounded Landing Site fixture and equips Samus for an input-earned charged spread.</summary>
    internal BombSpreadTransitionScenario(SuperMetroidRuntime runtime, bool left, int timingCase, bool wallRoute)
    {
        Runtime = runtime;
        Left = left;
        TimingCase = timingCase;
        WallRoute = wallRoute;
        Delay = timingCase < 10 ? timingCase : timingCase + 30;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        var level = runtime.Room.LoadedLevelData;
        // Match the grounded technique clearing, extending it upward for the jump.
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.Size.X + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 || (wallRoute && x == (left ? 29 : 33)) ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = Samus;
        samus.Control.Unlock();
        samus.Pose = left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose;
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.EquippedBeams = SamusBeamFlags.Charge;
        samus.Position = samus.Position.WithWhole(new(512, 490));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>Controller word for <paramref name="frame"/>.</summary>
    internal SnesButton Input(int frame)
    {
        var bindings = Runtime.Options.ControllerBindings;
        SnesButton input = bindings.Shoot;
        if (!WallRoute)
        {
            if (frame is >= 70 and < 125) input |= bindings.Jump;
            if (frame >= 76 && frame < 105 && frame != 78 + Delay)
                input |= SnesButton.Down;
            if (frame is >= 78 and < 125) input |= (Left ? SnesButton.Right : SnesButton.Left);
            return input;
        }
        int morphDelay = TimingCase >= 12 ? 0 : Delay;
        input = TimingCase == 12 || frame < 87 || frame >= 94 + morphDelay ? bindings.Shoot : SnesButton.None;
        if (frame is >= 70 and < 86 or >= 89 and < 200) input |= bindings.Jump;
        if (frame is >= 68 and < 87) input |= (Left ? SnesButton.Left : SnesButton.Right);
        if (frame is >= 87 and < 140) input |= (Left ? SnesButton.Right : SnesButton.Left);
        if (TimingCase == 13 && frame == 93)
        {
            input &= ~(SnesButton.Left | SnesButton.Right);
            input |= (Left ? SnesButton.Left : SnesButton.Right);
        }
        if (frame >= 94 + morphDelay && frame < 115 + morphDelay) input |= SnesButton.Down;
        return input;
    }

    /// <summary>Samus row after <paramref name="frame"/> ran with <paramref name="input"/>.</summary>
    internal string Row(int frame, SnesButton input) =>
        $"{(Left ? 1 : 0)},{Delay},{frame},{input:X},{(int)Samus.Pose:X4},{Samus.Position.X.Whole:X4},{Samus.Kinematics.Position.X.Fraction:X4},{Samus.Position.Y.Whole:X4},{Samus.Kinematics.Position.Y.Fraction:X4},{Samus.ProjectileFlareCounter:X4},{Samus.BombSpreadChargeTimeoutCounter:X4},{Runtime.Room.LoadedScope.BombProjectiles.BombCounter:X4}";

    /// <summary>Whether <paramref name="frame"/> also records every bomb slot: the release window onward.</summary>
    internal bool RecordsBombs(int frame) => WallRoute ? frame >= 115 : Delay == 6 && frame >= 105;

    /// <summary>The runtime's installed projectile list layout, whose instruction pointers the rows print.</summary>
    private SamusProjectileListLayout Lists => (Runtime.Artwork.SamusOverreads ?? throw new InvalidOperationException(
        "The bomb-spread trace needs the installed Samus overread catalog.")).ProjectileLists;

    /// <summary>One row per bomb slot. Inactive native slots retain scratch words that are overwritten
    /// at allocation, so an inactive slot records only its ownership, not dead bytes.</summary>
    internal IEnumerable<string> BombRows()
    {
        for (int slotIndex = 0; slotIndex < SamusBombProjectileSystem.SlotCount; slotIndex++)
        {
            var slot = Runtime.Room.LoadedScope.BombProjectiles.Slots[slotIndex];
            string bomb = $"bomb,{slotIndex},{(slot.IsActive ? 1 : 0)}";
            if (slot.IsActive)
                bomb += $",{slot.Type:X4},{slot.Position.X.Whole:X4},{slot.Position.X.Fraction:X4},{slot.Position.Y.Whole:X4},{slot.Position.Y.Fraction:X4},{slot.BombSpreadDirectionMagnitude:X4},{slot.BombSpreadYVelocity:X4},{slot.BombSpreadYSubvelocity:X4},{slot.BombTimer.RawValue:X4},{slot.Radius.X:X4},{slot.Radius.Y:X4},{Lists.Address(slot.Cursor):X4},{slot.InstructionTimer.RawValue:X4},{slot.Sprite?.Name}";
            yield return bomb;
        }
    }
}
