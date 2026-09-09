using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>#474: room-local arrival geometry diagnostic, not a native-CPU parity assertion.</summary>
internal static class ElevatorSpinjumpAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var doors = File.ReadLines("upstream-sm/assets/names.txt")
            .Where(line => line.StartsWith("0x83") && line.Contains(" kDoorDef_"))
            .Select(line => CartridgeDoorHeader.Load(bus, Convert.ToUInt16(line.Substring(4, 4), 16)))
            .ToArray();
        foreach (var (room, name) in new (ushort, string)[]
            { (RoomHeaderPointers.MorphBallRoom, "Blue Brinstar"),
              (RoomHeaderPointers.GreenBrinstarMainShaft, "Green Brinstar"),
              (RoomHeaderPointers.LowerNorfairMainHall, "Lower Norfair") })
        {
            var door = doors.Single(d => d.DestinationRoomPointer == room && (d.BitFlags & 0x80) != 0);
            foreach (int delay in new[] { 0, 1, 4, 8 })
            {
                var runtime = new SuperMetroidRuntime(bus);
                runtime.InitializeHud(HudSnapshot.CeresDebug);
                runtime.RunNmi(0, true);
                runtime.InitializeStartingCeresRoom();
                runtime.InitializeCeresStartSamus();
                runtime.InitializeDebugGroundedSamus(128, 0, 16);
                var samus = runtime.Samus!;
                samus.InputLocked = true;
                samus.Pose = SamusPoseIds.ForwardFacingPowerSuitPose;
                samus.InitializeAnimation(bus);
                runtime.Enemies.PrepareElevatorArrival();
                runtime.LoadCartridgeRoomThroughDoorForVerification(door,
                    (ushort)(door.DestinationScreenX * 256), (ushort)(door.DestinationScreenY * 256));
                int arrival = -1, spin = -1;
                ushort arrivalY = 0;
                for (int frame = 0; frame < 900; frame++)
                {
                    int afterArrival = arrival < 0 ? -1 : frame - arrival - 1;
                    ushort input = (ushort)SnesButton.A;
                    if (afterArrival >= delay) input |= (ushort)SnesButton.Left;
                    runtime.StepFrame(input);
                    if (arrival < 0 && runtime.Enemies.LastElevatorEvent == ElevatorFrameEvent.ArrivalCompleted)
                    {
                        arrival = frame;
                        arrivalY = samus.YPosition;
                    }
                    if (arrival >= 0 && spin < 0 && samus.ReadMovementType(bus) == SamusMovementType.SpinJumping)
                        spin = frame - arrival;
                    if (arrival >= 0 && frame - arrival <= 12)
                        Console.WriteLine($"ELEVATOR room={room:X4} delay={delay} frame={frame-arrival} pose={samus.Pose:X2} " +
                            $"x={samus.Kinematics.XFixed:X8} y={samus.Kinematics.YFixed:X8} direction={samus.Kinematics.YDirection}");
                    if (arrival >= 0 && frame - arrival >= 40) break;
                }
                if (arrival < 0) throw new InvalidDataException($"{name} elevator failed to release Samus.");
                Console.WriteLine($"ARRIVAL {name}: door={door.Pointer:X4}, frames={arrival+1}, Y={arrivalY}, delay={delay}, firstSpin={spin}.");
            }
        }
        return 0;
    }
}
