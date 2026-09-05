using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;

/// <summary>
/// Diagnostic traces for #307, not a claim that the player's ledge report is fixed.
/// Keeps isolated movement and the complete retail-room runtime observable separately.
/// </summary>
internal static class UnderwaterTurnProbe
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        TraceRetailRoom(bus);
        TraceConstructedFloor(bus);
        return 0;
    }

    private static void TraceRetailRoom(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.MaridiaNoobTubeBroken);
        runtime.LoadCartridgeRoomForDebug(UnderwaterTurnProbeData.BrokenTubeRoom, 0, 256);
        runtime.InitializeDebugGroundedSamus(128, 166, 20);
        runtime.Samus!.EquippedItems = UnderwaterTurnProbeData.ComparisonEquipment;
        // One left-input frame after twenty completely stationary frames; no continued
        // walking input can account for subsequent displacement. This retail placement
        // is on the lower sloped floor, NOT the player's still-unidentified test ledge.
        for (int frame = 0; frame < 80; frame++)
        {
            runtime.StepFrame(frame == 20 ? (ushort)SnesButton.Left : (ushort)0);
            var player = runtime.Samus;
            Console.WriteLine($"RUNTIME frame={frame} pose={player.Pose:X2} x={player.XPosition}.{player.Kinematics.XSubposition:X4} y={player.YPosition} speed={player.HorizontalSpeed.BaseSpeed:X4}.{player.HorizontalSpeed.BaseSubspeed:X4} mode={player.HorizontalSpeed.AccelerationMode} medium={player.LiquidPhysics.DetermineMovementMedium(player)}");
        }
    }

    private static void TraceConstructedFloor(SuperMetroidAddressSpace bus)
    {
        var tiles = new ushort[16 * 32];
        Array.Fill(tiles, (ushort)0x8000, 16 * 16, 16);
        var level = new RoomLevelData(16, 32, tiles, new byte[tiles.Length], new ushort[tiles.Length], []);
        foreach (bool left in new[] { true, false })
        {
            var samus = new SamusState
            {
                Pose = left ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose,
                XPosition = 128,
                EquippedItems = UnderwaterTurnProbeData.ComparisonEquipment,
            };
            samus.LiquidPhysics.ConfigureWater(8, 0x80);
            samus.RefreshCollisionRadii(bus);
            // Standing radius is 21, not neutral-jump's 19. Starting two pixels inside
            // the floor would make horizontal collision mask the reported displacement.
            samus.YPosition = (ushort)(256 - samus.Kinematics.YRadius);
            samus.InitializeAnimation(bus);
            samus.ApplyGroundedTurn(bus, left ? SamusPoseIds.TurningRightToLeftPose : SamusPoseIds.TurningLeftToRightPose);
            Console.WriteLine($"Turn direction={samus.ReadPoseXDirection(bus)} radius={samus.Kinematics.YRadius}");
            bool completed = false;
            for (ushort frame = 0; frame < 40; frame++)
            {
                SamusGroundedMovement.StepTurningOnGround(bus, level, samus, frame);
                samus.AnimateNoFx(bus, 0, frame);
                Console.WriteLine($"TURN left={left} frame={frame} x={samus.XPosition:X4}.{samus.Kinematics.XSubposition:X4} speed={samus.HorizontalSpeed.BaseSpeed:X4}.{samus.HorizontalSpeed.BaseSubspeed:X4} mode={samus.HorizontalSpeed.AccelerationMode} anim={samus.AnimationFrame}/{samus.AnimationFrameTimer} pose={samus.Pose:X2} pending={samus.PendingTransitionalPose}");
                if (samus.ApplyPendingVerifiedAnimationTransition(bus))
                {
                    completed = true;
                    break;
                }
            }
            if (!completed)
                throw new InvalidDataException("Turn probe did not reach its animation-owned standing transition.");
        }
    }
}
