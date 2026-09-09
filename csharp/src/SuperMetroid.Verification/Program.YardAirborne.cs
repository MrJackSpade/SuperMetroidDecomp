using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyYardRuntimeDistancePublication()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.StepFrame(0);
        AssertEqual(0x00010000u, runtime.Samus!.AbsoluteMovedLastFrameXFixed,
            "stationary native distance includes one pixel for the next Yard kick");
        runtime.Samus.Kinematics.XSubposition = 0xE000;
        runtime.StepFrame(0);
        AssertEqual(0x0001E000u, runtime.Samus.AbsoluteMovedLastFrameXFixed,
            "Yard receives full fractional camera checkpoint displacement");
        runtime.Samus.Kinematics.XSubposition = 0xC000;
        runtime.StepFrame(0);
        AssertEqual(0x0000E000u, runtime.Samus.AbsoluteMovedLastFrameXFixed,
            "native integer-only direction test allows biased distance below one pixel");
    }

    private static void VerifyYardAirborneTrajectories(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        var empty = new ushort[64 * 64];
        var level = new RoomLevelData(64, 64, empty, new byte[empty.Length],
            new ushort[empty.Length], new byte[8]);
        // Signed whole/fraction boundary cases exercise native ADC carry, the
        // unusual suppressed zero-high-word write, and the vertical speed cap.
        uint[] horizontal = [0, 0x00008000, 0x00010000, 0x00018000, 0xFFFF8000, 0xFFFF0000, 0xFFFE8000];
        uint[] vertical = [0xFFFE0000, 0xFFFF8000, 0, 0x0003F000];
        int samples = 0;
        foreach (uint initialX in horizontal)
        foreach (uint initialY in vertical)
        foreach (ushort behavior in new ushort[] { 3, 4, 5 })
        {
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), () => 0);
            var actor = enemies.Slots[0];
            var state = enemies.YardStates[0]!;
            actor.XPosition = actor.YPosition = 512;
            actor.XSubposition = actor.YSubposition = 0xF000;
            actor.InstructionTimer = ushort.MaxValue;
            state.MovementFunction = YardMovementFunction.Airborne;
            state.Behavior = behavior;
            state.AirborneXVelocity = (ushort)(initialX >> 16);
            state.AirborneXSubvelocity = (ushort)initialX;
            state.AirborneYVelocity = (ushort)(initialY >> 16);
            state.AirborneYSubvelocity = (ushort)initialY;
            var samus = new SamusState { XPosition = 32, YPosition = 32,
                Pose = SamusPoseIds.FacingRightNormalPose, Health = 999, MaxHealth = 999 };
            samus.RefreshCollisionRadii(bus);
            uint x = 0x0200F000, y = 0x0200F000, vx = initialX, vy = initialY;
            for (int frame = 0; frame < 24; frame++)
            {
                // Oracle follows $A3:D1B3's word stores independently of production
                // helpers. Movement consumes old velocity, then friction/gravity run.
                if (behavior != 3)
                {
                    x = unchecked(x + vx);
                    uint changed = unchecked(vx + ((vx & 0x80000000) != 0 ? 0x1000u : 0xFFFFF000u));
                    vx = (changed & 0xFFFF0000) == 0
                        ? (vx & 0xFFFF0000) | (changed & 0xFFFF) : changed;
                }
                y = unchecked(y + vy);
                uint accelerated = unchecked(vy + 0x2000);
                vy = unchecked((short)((accelerated >> 16) - 4)) < 0
                    ? accelerated : (vy & 0xFFFF0000) | (accelerated & 0xFFFF);
                enemies.StepFrame(384, 384, false, samus, level: level);
                AssertEqual(x, ((uint)actor.XPosition << 16) | actor.XSubposition, "Yard airborne X trajectory");
                AssertEqual(y, ((uint)actor.YPosition << 16) | actor.YSubposition, "Yard airborne Y trajectory");
                AssertEqual(vx, ((uint)state.AirborneXVelocity << 16) | state.AirborneXSubvelocity, "Yard native friction words");
                AssertEqual(vy, ((uint)state.AirborneYVelocity << 16) | state.AirborneYSubvelocity, "Yard native gravity words");
                AssertEqual(YardMovementFunction.Airborne, state.MovementFunction, "empty fixture remains airborne");
                samples++;
            }
        }
        Console.WriteLine($"Yard airborne: {samples} exact position/velocity samples across drop, kick and shot states.");
    }
}
