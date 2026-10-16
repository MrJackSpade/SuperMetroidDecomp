using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>Checks Yard kick distance publication and airborne fixed-point trajectories.</summary>
internal static class YardAirborneVerification
{
    /// <summary>Confirms the per-frame Samus distance includes native pixel bias and fractional camera movement.</summary>
    internal static void VerifyYardRuntimeDistancePublication()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.StepFrame(0);
        AssertEqual(0x00010000u, runtime.Player.IntroducedSamus.AbsoluteMovedLastFrameXFixed,
            "stationary native distance includes one pixel for the next Yard kick");
        runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithFractionX(0xE000);
        runtime.StepFrame(0);
        AssertEqual(0x0001E000u, runtime.Player.IntroducedSamus.AbsoluteMovedLastFrameXFixed,
            "Yard receives full fractional camera checkpoint displacement");
        runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithFractionX(0xC000);
        runtime.StepFrame(0);
        AssertEqual(0x0000E000u, runtime.Player.IntroducedSamus.AbsoluteMovedLastFrameXFixed,
            "native integer-only direction test allows biased distance below one pixel");
    }

    /// <summary>Checks dropped, kicked, and beam-launched motion across signed carry and vertical speed caps.</summary>
    internal static void VerifyYardAirborneTrajectories(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        var empty = new ushort[64 * 64];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(64, 64), new RoomLevelPlanes(empty, new byte[empty.Length], new ushort[empty.Length]), new byte[8]);
        // Signed whole/fraction boundary cases exercise native ADC carry, the
        // unusual suppressed zero-high-word write, and the vertical speed cap.
        uint[] horizontal = [0, 0x00008000, 0x00010000, 0x00018000, 0xFFFF8000, 0xFFFF0000, 0xFFFE8000];
        uint[] vertical = [0xFFFE0000, 0xFFFF8000, 0, 0x0003F000];
        int samples = 0;
        foreach (uint initialX in horizontal)
        foreach (uint initialY in vertical)
        foreach (YardBehavior behavior in new[] { YardBehavior.Dropped, YardBehavior.Kicked, YardBehavior.BeamLaunched })
        {
            var enemies = new RoomEnemySystem { TileArtwork = FixtureEnemyTileArtwork() };
            enemies.Load(new RoomEnemyLoadRequest(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(() => 0)));
            var actor = enemies.SlotTable.Slots[0];
            var state = enemies.YardStates[0];
            actor.Position = new(new(512, 0xF000), new(512, 0xF000));
            actor.InstructionTimer = NativeCountdown.Start(ushort.MaxValue);
            (state ?? throw new InvalidOperationException("Verification requires state.")).MovementFunction = YardMovementFunction.Airborne;
            state.Behavior = behavior;
            state.AirborneVelocity = new FixedVector<RoomPixels>(
                SnesSignedSixteenSixteen.FromRaw(unchecked((int)initialX)),
                SnesSignedSixteenSixteen.FromRaw(unchecked((int)initialY)));
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)),
                Pose = SamusPoseId.FacingRightNormalPose, Health = 999, MaxHealth = 999 };
            samus.PoseMetadata.RefreshCollisionRadii();
            uint x = 0x0200F000, y = 0x0200F000, vx = initialX, vy = initialY;
            for (int frame = 0; frame < 24; frame++)
            {
                // Oracle follows $A3:D1B3's word stores independently of production
                // helpers. Movement consumes old velocity, then friction/gravity run.
                if (behavior != YardBehavior.Dropped)
                {
                    x = unchecked(x + vx);
                    uint changed = unchecked(vx + ((vx & 0x80000000) != 0 ? 0x1000u : 0xFFFFF000u));
                    vx = SnesSixteenSixteen.WholeOf(changed) == 0
                        ? SnesSixteenSixteen.Compose((ushort)SnesSixteenSixteen.WholeOf(vx), (ushort)SnesSixteenSixteen.FractionOf(changed)) : changed;
                }
                y = unchecked(y + vy);
                uint accelerated = unchecked(vy + 0x2000);
                vy = unchecked((short)(SnesSixteenSixteen.WholeOf(accelerated) - 4)) < 0
                    ? accelerated : SnesSixteenSixteen.Compose((ushort)SnesSixteenSixteen.WholeOf(vy), (ushort)SnesSixteenSixteen.FractionOf(accelerated));
                enemies.StepFrame(new RoomEnemyFrame { Camera = new(384, 384), TimeIsFrozen = false, Samus = samus, Level = level });
                AssertEqual(x, actor.Position.X.RawValue, "Yard airborne X trajectory");
                AssertEqual(y, actor.Position.Y.RawValue, "Yard airborne Y trajectory");
                AssertEqual(vx, unchecked((uint)state.AirborneVelocity.X.RawValue), "Yard native friction words");
                AssertEqual(vy, unchecked((uint)state.AirborneVelocity.Y.RawValue), "Yard native gravity words");
                AssertEqual(YardMovementFunction.Airborne, state.MovementFunction, "empty fixture remains airborne");
                samples++;
            }
        }
        Console.WriteLine($"Yard airborne: {samples} exact position/velocity samples across drop, kick and shot states.");
    }
}
