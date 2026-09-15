using System.Globalization;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Replays the supplied CWJ movie from the source-room run through Moat control.</summary>
internal static class MoatMovieTransitionProbe
{
    private const int SeedFrame = 102;
    private const int DoorCollisionFrame = 103;
    private const int DestinationControlFrame = 265;
    private const int FinalComparedFrame = 269;

    public static int Run(string romPath, string directory)
    {
        byte[] memory = MoatMovieFixture.LoadCheckpoint(
            directory,
            "frame-102.wram",
            "50F49FAAF602843328275D695EBCD958BD040A62044B497433D8802C74F4A0BE");
        ushort[] inputs = MoatMovieFixture.LoadInputs(directory);
        Dictionary<int, NativeTransitionCheckpoint> native = LoadExpected(directory);
        AssertNativeDispatcherBoundary(native);
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        SuperMetroidRuntime runtime = MoatMovieFixture.CreateRuntime(bus, memory);

        AssertCheckpoint(runtime, native[SeedFrame]);
        runtime.Controller1.Latch(inputs[SeedFrame - 1]);
        for (int frame = SeedFrame; frame < DoorCollisionFrame; frame++)
        {
            runtime.StepFrame(inputs[frame]);
            AssertCheckpoint(runtime, native[frame + 1]);
        }

        CartridgeDoorHeader pendingDoor = runtime.PendingDoorTransition ??
            throw new InvalidDataException(
                "The supplied source-room run did not publish its native Moat doorway.");
        if (pendingDoor.Pointer != MoatMovieDefinitions.MoatEntryDoor ||
            pendingDoor.DestinationRoomPointer != MoatMovieDefinitions.MoatRoom)
        {
            throw new InvalidDataException(
                $"The supplied run published door $83:{pendingDoor.Pointer:X4} to " +
                $"$8F:{pendingDoor.DestinationRoomPointer:X4}, not its Moat entrance.");
        }

        // Native checkpoint 103 is state $0A immediately after collision. The bank-$82
        // state-$0A owner captures the door and creates state $0B without moving Samus;
        // Begin is that boundary in the translated frontend.
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        int transitionFrames = 0;
        while (transition.IsActive && transitionFrames++ < 512)
            transition.Step(runtime, audio, inputs[DoorCollisionFrame]);
        if (transition.IsActive)
            throw new InvalidDataException("The supplied Moat door transition did not complete.");

        // Audio queue duration is not imported from the native APU snapshot, so wall-clock
        // transition length is intentionally outside this fixture. Its gameplay handoff is
        // cartridge state 265: exact position, subpixels, retained speed, camera, and pose.
        AssertCheckpoint(runtime, native[DestinationControlFrame]);
        if (runtime.ActiveRoom?.Pointer != MoatMovieDefinitions.MoatRoom ||
            runtime.ActiveDoor?.Pointer != MoatMovieDefinitions.MoatEntryDoor)
        {
            throw new InvalidDataException("The transition completed without the native Moat room/door pair.");
        }

        for (int frame = DestinationControlFrame; frame < FinalComparedFrame; frame++)
        {
            runtime.StepFrame(inputs[frame]);
            AssertCheckpoint(runtime, native[frame + 1]);
        }

        Console.WriteLine(
            $"PASS: player CWJ incoming run and production door handoff; frames " +
            $"{SeedFrame}-{DoorCollisionFrame} and {DestinationControlFrame}-{FinalComparedFrame} " +
            $"match native position/subpixels, pose, retained speed, camera, room, and door " +
            $"({transitionFrames} translated transition frames; native APU wait excluded). ");
        return 0;
    }

    private static void AssertNativeDispatcherBoundary(
        IReadOnlyDictionary<int, NativeTransitionCheckpoint> native)
    {
        if (native[SeedFrame].GameState != (ushort)SuperMetroidGameState.MainGameplay ||
            native[DoorCollisionFrame].GameState != (ushort)SuperMetroidGameState.LoadingNextRoomA ||
            native[DestinationControlFrame].GameState != (ushort)SuperMetroidGameState.MainGameplay)
        {
            throw new InvalidDataException(
                "Native CWJ checkpoints do not contain the expected gameplay -> door -> gameplay boundary.");
        }

        for (int frame = DoorCollisionFrame + 1; frame < DestinationControlFrame; frame++)
        {
            if (native[frame].GameState != (ushort)SuperMetroidGameState.LoadingNextRoomB)
                throw new InvalidDataException(
                    $"Native CWJ frame {frame} left state $0B before control handoff.");
        }
    }

    private static Dictionary<int, NativeTransitionCheckpoint> LoadExpected(string directory)
    {
        var result = new Dictionary<int, NativeTransitionCheckpoint>();
        foreach (string line in File.ReadLines(Path.Combine(directory, "native-transition.csv")).Skip(1))
        {
            string[] fields = line.Split(',');
            if (fields.Length != 11)
                throw new InvalidDataException($"Invalid native transition row: {line}");
            uint H32(int index) => uint.Parse(fields[index], NumberStyles.HexNumber);
            ushort H16(int index) => ushort.Parse(fields[index], NumberStyles.HexNumber);
            int frame = int.Parse(fields[0], CultureInfo.InvariantCulture);
            result.Add(frame, new NativeTransitionCheckpoint(
                frame,
                H16(1),
                H16(2),
                H16(3),
                H32(4),
                H32(5),
                (byte)H16(6),
                H32(7),
                H32(8),
                H16(9),
                H16(10)));
        }

        return result;
    }

    private static void AssertCheckpoint(
        SuperMetroidRuntime runtime,
        NativeTransitionCheckpoint expected)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            $"Movie frame {expected.Frame} has no live Samus state.");
        ushort room = runtime.ActiveRoom?.Pointer ?? 0;
        ushort door = runtime.PendingDoorTransition?.Pointer ?? runtime.ActiveDoor?.Pointer ?? 0;
        // The frame-100 room-load fixture begins after the movie's prior doorway and does
        // not import that irrelevant historical ActiveDoor reference. From the reported
        // pre-Moat collision onward, door identity is part of the assertion.
        if (expected.Frame < DoorCollisionFrame)
            door = expected.Door;
        ushort cameraX = runtime.Camera?.XPosition ?? 0;
        ushort cameraY = runtime.Camera?.YPosition ?? 0;
        uint extra = ((uint)samus.HorizontalSpeed.ExtraRunSpeed << 16) |
            samus.HorizontalSpeed.ExtraRunSubspeed;
        string actual =
            $"room={room:X4},door={door:X4},x={samus.Kinematics.XFixed:X8}," +
            $"y={samus.Kinematics.YFixed:X8},pose={samus.Pose:X2}," +
            $"base={samus.HorizontalSpeed.BaseFixed:X8}," +
            $"extra={extra:X8},camera={cameraX:X4}/{cameraY:X4}";
        string wanted =
            $"room={expected.Room:X4},door={expected.Door:X4},x={expected.X:X8}," +
            $"y={expected.Y:X8},pose={expected.Pose:X2},base={expected.Base:X8}," +
            $"extra={expected.Extra:X8},camera={expected.CameraX:X4}/{expected.CameraY:X4}";
        if (actual != wanted)
        {
            throw new InvalidDataException(
                $"Movie transition frame {expected.Frame} differs.\nNative: {wanted}\nPort:   {actual}");
        }
    }

    private readonly record struct NativeTransitionCheckpoint(
        int Frame,
        ushort GameState,
        ushort Room,
        ushort Door,
        uint X,
        uint Y,
        byte Pose,
        uint Base,
        uint Extra,
        ushort CameraX,
        ushort CameraY);
}
