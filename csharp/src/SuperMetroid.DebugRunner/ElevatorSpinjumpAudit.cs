using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// #474: captures room-local elevator arrivals and compares post-release movement
/// against native CPU traces. This does not verify native elevator release timing.
/// </summary>
internal static class ElevatorSpinjumpAudit
{
    public static int Compare(string directory)
    {
        int actorSamples = 0;
        foreach (string room in new[] { "9E9F", "9AD9", "B236" })
        {
            string prefix = Path.Combine(directory, $"{room}-0.actor");
            string[] managed = File.ReadAllLines(prefix + ".managed.csv");
            string[] native = File.ReadAllLines(prefix + ".native.csv");
            if (native.Length == 0 || managed.Length != native.Length)
                throw new InvalidDataException($"Incomplete elevator actor trace: {room}.");
            for (int frame = 0; frame < native.Length; frame++)
            {
                if (!native[frame].StartsWith(frame + ",", StringComparison.Ordinal) ||
                    native[frame] != managed[frame])
                    throw new InvalidDataException($"Elevator actor {room} frame {frame}: managed {managed[frame]}, native {native[frame]}.");
                actorSamples++;
            }
            if (!native[^1].EndsWith(",0,0,0", StringComparison.Ordinal))
                throw new InvalidDataException($"Elevator actor trace ended before unlock: {room}.");
        }
        Console.WriteLine($"Elevator arrival: {actorSamples} exact actor/status/input-lock samples agree.");
        int samples = 0;
        foreach (string room in new[] { "9E9F", "9AD9", "B236" })
        foreach (int delay in new[] { 0, 1, 4, 8 })
        {
            string prefix = Path.Combine(directory, $"{room}-{delay}");
            string[] managed = File.ReadAllLines(prefix + ".managed.csv");
            string[] native = File.ReadAllLines(prefix + ".native.csv");
            if (managed.Length != 41 || native.Length != 41)
                throw new InvalidDataException($"Incomplete elevator trace: {room}-{delay}.");
            for (int frame = 0; frame < 40; frame++)
            {
                if (!native[frame + 1].StartsWith(frame + ",", StringComparison.Ordinal) ||
                    native[frame + 1] != managed[frame + 1])
                    throw new InvalidDataException($"Elevator {room}-{delay} frame {frame}: managed {managed[frame+1]}, native {native[frame+1]}.");
                samples++;
            }
        }
        Console.WriteLine($"Elevator release: {samples} exact cartridge position/pose samples agree.");
        return 0;
    }

    public static int Run(string rom, string? outputDirectory = null, bool directionHeldDuringArrival = false)
    {
        if (outputDirectory != null) Directory.CreateDirectory(outputDirectory);
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
            foreach (int delay in directionHeldDuringArrival ? new[] { 0 } : new[] { 0, 1, 4, 8 })
            {
                string? prefix = outputDirectory == null ? null : Path.Combine(outputDirectory, $"{room:X4}-{delay}");
                using var trace = prefix == null ? null : new StreamWriter(prefix + ".managed.csv");
                trace?.WriteLine("frame,x,y,pose");
                var runtime = new SuperMetroidRuntime(bus);
                runtime.InitializeHud(HudSnapshot.CeresDebug);
                runtime.RunNmi(0, true);
                runtime.InitializeStartingCeresRoom();
                runtime.InitializeCeresStartSamus();
                runtime.InitializeDebugGroundedSamus(128, 0, 16);
                // Leave the bootstrap room through the debug loader's lifecycle reset.
                // The lower-level door verification seam assumes that reset already ran;
                // otherwise the Ceres arrival actor survives and unlocks input at frame 59.
                runtime.LoadCartridgeRoomForDebug(room);
                if (runtime.CeresElevatorArrival != null)
                    throw new InvalidDataException("Elevator fixture retained its Ceres arrival owner.");
                var samus = runtime.Samus!;
                samus.InputLocked = true;
                samus.Pose = SamusPoseIds.ForwardFacingPowerSuitPose;
                samus.InitializeAnimation(bus);
                runtime.Enemies.PrepareElevatorArrival();
                runtime.LoadCartridgeRoomThroughDoorForVerification(door,
                    (ushort)(door.DestinationScreenX * 256), (ushort)(door.DestinationScreenY * 256));
                var elevator = runtime.Enemies.Slots.Single(slot =>
                    runtime.Enemies.ElevatorStates[slot.SlotIndex] != null);
                using var actorTrace = prefix == null ? null : new StreamWriter(prefix + ".actor.managed.csv");
                if (prefix != null)
                {
                    using var seed = new StreamWriter(prefix + ".actor-seed.csv");
                    seed.WriteLine($"{elevator.XPosition},{elevator.YPosition},{elevator.YSubposition},{elevator.Parameter1},{elevator.VariableA}");
                }
                int arrival = -1, spin = -1;
                ushort arrivalY = 0;
                for (int frame = 0; frame < 900; frame++)
                {
                    int afterArrival = arrival < 0 ? -1 : frame - arrival - 1;
                    ushort input = (ushort)SnesButton.A;
                    if (directionHeldDuringArrival || afterArrival >= delay) input |= (ushort)SnesButton.Left;
                    runtime.StepFrame(input);
                    if (arrival < 0 && runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.ArrivalCompleted)
                    {
                        if (!samus.InputLocked || samus.Pose != SamusPoseIds.ForwardFacingPowerSuitPose ||
                            runtime.ProspectiveSamusPose != null)
                            throw new InvalidDataException($"{name}: pose input escaped active elevator at frame {frame}.");
                    }
                    if (arrival < 0)
                        actorTrace?.WriteLine($"{frame},{elevator.YPosition},{elevator.YSubposition},{(ushort)runtime.Enemies.ElevatorStatus},{runtime.Enemies.ElevatorFlags},{(samus.InputLocked ? 1 : 0)}");
                    if (arrival < 0 && runtime.Enemies.LastElevatorEvent == ElevatorFrameEvent.ArrivalCompleted)
                    {
                        arrival = frame;
                        arrivalY = samus.YPosition;
                        if (prefix != null) RoomMovementSeedExporter.Write(runtime, prefix + ".movement-seed", includeScrollOwners: true);
                    }
                    else if (arrival >= 0)
                        trace?.WriteLine($"{frame-arrival-1},{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2}");
                    if (arrival >= 0 && spin < 0 && samus.ReadMovementType(bus) == SamusMovementType.SpinJumping)
                        spin = frame - arrival;
                    if (arrival >= 0 && frame - arrival <= 12)
                        Console.WriteLine($"ELEVATOR room={room:X4} delay={delay} frame={frame-arrival} pose={samus.Pose:X2} " +
                            $"x={samus.Kinematics.XFixed:X8} y={samus.Kinematics.YFixed:X8} direction={samus.Kinematics.YDirection}");
                    if (arrival >= 0 && frame - arrival >= 40) break;
                }
                if (arrival < 0) throw new InvalidDataException($"{name} elevator failed to release Samus.");
                Console.WriteLine($"ARRIVAL {name}: door={door.Pointer:X4}, frames={arrival+1}, Y={arrivalY}, delay={delay}, preheld={directionHeldDuringArrival}, firstSpin={spin}.");
            }
        }
        return 0;
    }
}
