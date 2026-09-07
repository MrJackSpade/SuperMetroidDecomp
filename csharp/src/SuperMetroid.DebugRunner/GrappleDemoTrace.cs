using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;

/// <summary>Frame-level evidence for the ROM-authored grapple demos; no trajectory is injected.</summary>
internal static class GrappleDemoTrace
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int set = 0; set < AttractDemoRomData.SetCount; set++)
            for (int index = 0; AttractDemoScene.Read(bus, set, index) is { } scene; index++)
            {
                if (scene.InputObject is not (AttractDemoRomData.InputObjects.GrappleBeam or
                    AttractDemoRomData.InputObjects.AdvancedGrappleBeam)) continue;
                var runtime = new SuperMetroidRuntime(bus);
                runtime.InitializeAttractDemo(scene);
                // This independent interpreter only exposes the exact input words for the
                // trace; runtime still executes its own normal attract-input handoff.
                var input = new AttractDemoInput(bus, scene);
                var samus = runtime.Samus!;
                Console.WriteLine($"DEMO {set}/{index} room={scene.RoomPointer:X4} duration={scene.Duration}");
                var level = runtime.LevelData!;
                for (int y = 0; y < level.HeightInBlocks; y++)
                {
                    var anchors = Enumerable.Range(0, level.WidthInBlocks)
                        .Where(x => level.GetCollisionBlock(x, y).CollisionType == SuperMetroid.Core.Rooms.RoomCollisionType.GrappleBlock)
                        .Select(x => $"{x:X2}:{level.GetCollisionBlock(x, y).Behavior:X2}");
                    if (anchors.Any()) Console.WriteLine($"ANCHORS row={y:X2} x:bts={string.Join(',', anchors)}");
                }
                for (int frame = 0; frame < scene.Duration; frame++)
                {
                    input.Step(bus, SuperMetroidGameState.PlayingDemo, samus.ReadMovementType(bus));
                    runtime.StepFrame(0, advanceGameTime: false);
                    var grapple = samus.Grapple;
                    Console.WriteLine($"f={frame:D3} held={input.Script.Held:X4} new={input.Script.NewlyPressed:X4} " +
                        $"xy={samus.XPosition:X4},{samus.YPosition:X4} fixed={samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8} " +
                        $"vx={samus.HorizontalSpeed.BaseFixed:X8} vy={samus.Kinematics.VerticalSpeedFixed:X8} ydir={samus.Kinematics.YDirection} " +
                        $"pose={samus.Pose:X2} hud={samus.SelectedHudItem} " +
                        $"grapple={grapple.Phase} anchor={grapple.AnchorX:X4},{grapple.AnchorY:X4} " +
                        $"length={grapple.RopeLength} angle={grapple.Angle} omega={grapple.AngularVelocity} " +
                        $"releaseMover={grapple.ReleasedMovementActive} terrain={runtime.LastGrappleMovement?.TerrainCollided}");
                    // First ordinary pose-input frame after release cleanup. Bank $91's
                    // normal-jump input handler must honor the script's held aim-up shoulder
                    // even while beta still uses the grapple-release movement handler.
                    if (scene.InputObject == AttractDemoRomData.InputObjects.GrappleBeam && frame == 164 &&
                        samus.Pose != SamusPoseIds.NormalJumpAimDiagonalUpRightPose)
                        throw new InvalidDataException($"Grapple demo release discarded held aim input: pose={samus.Pose:X2}.");
                }
            }
        return 0;
    }
}
