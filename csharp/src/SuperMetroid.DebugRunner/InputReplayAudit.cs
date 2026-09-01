using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Headless consumer for desktop <c>.smrec</c> files. It drives the same public frontend
/// dispatcher as the window and retains a compact tail of cartridge-owned state so a user
/// report can be diagnosed from the exact controller history rather than reconstructed.
/// </summary>
internal static class InputReplayAudit
{
    // A failed desktop session normally ends within a few seconds of the condition the
    // player is reporting.  Keeping the final second of raw states makes the console
    // output small enough to survive the test harness while still showing held/new
    // input and every movement-state field involved in a lock.
    private const int RetainedTailFrames = 60;

    public static int Run(string recordingPath, string romPath)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        PrintInputRuns(recording.ControllerInputs, startFrame: 5400);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] digest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            digest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(digest, recording.RomSha256))
            throw new InvalidDataException("Replay ROM SHA-256 does not match the recording.");

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        var tail = new Queue<ReplayFrameState>(RetainedTailFrames);
        ushort? previousRoom = null;
        ushort previousCeresStatus = ushort.MaxValue;
        bool previousEjection = false;
        SuperMetroidRuntime? lastRuntime = null;

        for (int index = 0; index < recording.ControllerInputs.Length; index++)
        {
            ushort input = recording.ControllerInputs[index];
            FrontendFrame frontend = game.Step(input);
            SuperMetroidRuntime? runtime = game.RuntimeForVerification;
            lastRuntime = runtime;
            SamusState? samus = runtime?.Samus;
            RidleyEnemyState? ridley = runtime?.Enemies.CeresRidley;
            ushort? room = runtime?.ActiveRoom?.Pointer;
            ushort ceresStatus = runtime?.Enemies.CeresStatus ?? 0;
            bool ejection = samus?.CeresRidleyEjection.IsActive ?? false;
            var state = new ReplayFrameState(
                index,
                frontend.GameState,
                room,
                input,
                runtime?.Controller1.NewlyPressed ?? 0,
                samus?.Pose,
                samus?.XPosition,
                samus?.YPosition,
                samus?.Health,
                ceresStatus,
                ejection,
                samus?.CeresRidleyEjection.IsPending ?? false,
                samus?.InputLocked ?? false,
                samus?.KnockbackActive ?? false,
                samus?.KnockbackTimer ?? 0,
                samus?.KnockbackDirection ?? 0,
                samus?.KnockbackXDirection ?? 0,
                ridley?.Function,
                ridley?.Mode7Active ?? false,
                runtime?.HasPendingDoorTransition ?? false);
            if (tail.Count == RetainedTailFrames)
                tail.Dequeue();
            tail.Enqueue(state);

            if (room != previousRoom || ceresStatus != previousCeresStatus || ejection != previousEjection)
                Console.WriteLine(state.Format());
            previousRoom = room;
            previousCeresStatus = ceresStatus;
            previousEjection = ejection;
        }

        Console.WriteLine($"Replay completed {recording.ControllerInputs.Length} recorded calls. Tail:");
        foreach (ReplayFrameState state in tail)
            Console.WriteLine(state.Format());

        // A controller trace is most useful when it also preserves the exact final image
        // produced from that deterministic state. Render only once after replay: the
        // software PPU is intentionally faithful rather than fast, and recording/replay
        // diagnostics must never multiply its cost by every input frame.
        if (lastRuntime?.ActiveDoor is not null)
        {
            string captureDirectory = Path.Combine("csharp", "test-temp", "input-replays");
            Directory.CreateDirectory(captureDirectory);
            string capturePath = Path.Combine(
                captureDirectory,
                $"{Path.GetFileNameWithoutExtension(recordingPath)}.last.png");
            PngWriter.WriteRgba(
                capturePath,
                FrontendFrame.Width,
                FrontendFrame.Height,
                SuperMetroidRuntimeFrameRenderer.Render(lastRuntime));
            SamusMode7Transform? mode7 = lastRuntime.DisplayedSamusMode7Transform;
            Console.WriteLine(
                $"Final PPU capture: {Path.GetFullPath(capturePath)}; displayed Mode 7=" +
                (mode7 is { } transform
                    ? $"A/D=${transform.MatrixA:X4}, B=${transform.MatrixB:X4}, " +
                      $"C=${transform.MatrixC:X4}, center=(${transform.CenterX:X4},${transform.CenterY:X4})"
                    : "inactive"));
        }
        return 0;
    }

    /// <summary>
    /// Prints only non-zero controller runs near the reported failure.  This distinguishes
    /// a cartridge state that rejected player input from a session in which the controller
    /// was actually neutral, without flooding the diagnostic with one line per video frame.
    /// </summary>
    private static void PrintInputRuns(ReadOnlySpan<ushort> inputs, int startFrame)
    {
        for (int start = Math.Max(0, startFrame); start < inputs.Length;)
        {
            ushort value = inputs[start];
            int end = start + 1;
            while (end < inputs.Length && inputs[end] == value)
                end++;
            if (value != 0)
                Console.WriteLine($"input rec={start}..{end - 1} value=${value:X4}");
            start = end;
        }
    }

    private readonly record struct ReplayFrameState(
        int RecordingFrame,
        SuperMetroidGameState GameState,
        ushort? Room,
        ushort Input,
        ushort NewInput,
        byte? Pose,
        ushort? X,
        ushort? Y,
        ushort? Health,
        ushort CeresStatus,
        bool EjectionActive,
        bool EjectionPending,
        bool InputLocked,
        bool KnockbackActive,
        ushort KnockbackTimer,
        ushort KnockbackDirection,
        ushort KnockbackXDirection,
        RidleyAiFunction? RidleyFunction,
        bool Mode7Active,
        bool PendingDoor)
    {
        public string Format() =>
            $"rec={RecordingFrame,6} gs=${(ushort)GameState:X2} room=" +
            (Room is ushort room ? $"${room:X4}" : "----") +
            $" in=${Input:X4}/${NewInput:X4} pose=" +
            (Pose is byte pose ? $"${pose:X2}" : "--") +
            $" xy=" + (X is ushort x && Y is ushort y ? $"${x:X4},${y:X4}" : "----,----") +
            $" hp={Health?.ToString() ?? "-"} ceres=${CeresStatus:X4}" +
            $" eject={EjectionActive}/{EjectionPending} lock={InputLocked}" +
            $" kb={KnockbackActive}/${KnockbackTimer}/${KnockbackDirection}/{KnockbackXDirection} ridley=" +
            (RidleyFunction is { } function ? $"${(ushort)function:X4}" : "----") +
            $" m7={Mode7Active} door={PendingDoor}";
    }
}
