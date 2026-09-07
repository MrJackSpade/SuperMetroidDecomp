using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Isolates post-wall-jump pose handling from landing and enemy contact.</summary>
internal static class WallJumpSpinAudit
{
    public static int Run(string romPath)
    {
        foreach (ushort input in new[] { (ushort)SnesButton.A,
            (ushort)(SnesButton.A | SnesButton.Right), (ushort)(SnesButton.A | SnesButton.Left), (ushort)0,
            (ushort)(SnesButton.A | SnesButton.X) })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 0, 0);
            var level = runtime.LevelData!;
            for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++)
                level.SetForegroundEntry(block, 0);
            var samus = runtime.Samus!;
            samus.XPosition = 800;
            samus.YPosition = 600;
            samus.Pose = SamusPoseIds.SpinJumpRightPose;
            samus.InitializeAnimation(bus);
            samus.ApplyWallJumpTrigger(bus);
            samus.InputLocked = false;
            var animations = new HashSet<(byte Pose, ushort Frame)>();
            for (int tick = 0; tick < 32; tick++)
            {
                runtime.StepFrame(input, allowCeresElevatorDeparture: false);
                var movement = samus.ReadMovementType(bus);
                animations.Add((samus.Pose, samus.AnimationFrame));
                // $91:A9EC checks held Fire before held Jump. A shot intentionally
                // cancels wall-jump rotation even if Jump remains held throughout.
                bool firing = (input & (ushort)SnesButton.X) != 0;
                if (firing ? movement != SamusMovementType.NormalJumping :
                    movement is not (SamusMovementType.WallJumping or SamusMovementType.SpinJumping))
                    throw new InvalidDataException($"Wall jump lost spin without landing/shot/aim: input={input:X4}, tick={tick}, pose={samus.Pose:X2}.");
            }
            if (animations.Count < 2 || samus.YPosition == 600)
                throw new InvalidDataException("Wall-jump fixture did not advance movement and animation.");
        }
        Console.WriteLine("Post-wall-jump spin: 160 runtime frames preserve rotation without Fire and select normal jumping with held Fire, matching native priority.");
        return 0;
    }
}
