using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Replays recorded Start edges and compares the complete raster on the frame immediately
/// before and after each edge. This deliberately begins below keyboard/gamepad mapping: a
/// desktop recording already contains the exact SNES word the frontend actually consumed.
/// </summary>
internal static class StartInputVisualAudit
{
    private const int MaximumHorizontalShift = 32;
    private const int MaximumVerticalShift = 8;
    private const int PauseFollowFrameCount = 48;
    private const int TranslationSampleStride = 4;

    public static int Run(string recordingPath, string romPath, string outputDirectory)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] digest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            digest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(digest, recording.RomSha256))
            throw new InvalidDataException("Start audit ROM SHA-256 does not match the recording.");

        Directory.CreateDirectory(outputDirectory);
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);

        FrontendFrame? previousFrame = null;
        ushort previousInput = 0;
        int edgeCount = 0;
        int? followedGameplayStart = null;
        FrontendFrame? followedBaseline = null;
        SuperMetroidGameState previousGameState = game.GameState;
        int lastStartEdge = FindLastStartEdge(recording.ControllerInputs);
        for (int frameIndex = 0; frameIndex < recording.ControllerInputs.Length; frameIndex++)
        {
            ushort input = recording.ControllerInputs[frameIndex];
            SuperMetroidGameState stateBefore = game.GameState;
            SuperMetroidRuntime? runtimeBefore = game.RuntimeForVerification;
            ushort? cameraXBefore = runtimeBefore?.Camera?.XPosition;
            ushort? cameraYBefore = runtimeBefore?.Camera?.YPosition;
            FrontendFrame currentFrame = game.Step(input);

            if (followedBaseline is FrontendFrame resumeBaseline &&
                previousGameState != SuperMetroidGameState.MainGameplay &&
                currentFrame.GameState == SuperMetroidGameState.MainGameplay)
            {
                RasterTranslation resumeTranslation = FindBestTranslation(
                    resumeBaseline.Pixels,
                    currentFrame.Pixels,
                    TranslationSampleStride);
                PngWriter.WriteRgba(
                    Path.Combine(outputDirectory, $"pause-resume-{frameIndex:D6}.png"),
                    FrontendFrame.Width,
                    FrontendFrame.Height,
                    currentFrame.Pixels);
                Console.WriteLine(
                    $"Pause resumed gameplay at recording frame {frameIndex}: " +
                    $"camera ({FormatWord(game.RuntimeForVerification?.Camera?.XPosition)}," +
                    $"{FormatWord(game.RuntimeForVerification?.Camera?.YPosition)}), " +
                    $"pre-pause translation dx={resumeTranslation.X}, dy={resumeTranslation.Y}, " +
                    $"mean error={resumeTranslation.MeanAbsoluteChannelError:F3}.");
            }

            bool startEdge = (input & (ushort)SnesButton.Start) != 0 &&
                (previousInput & (ushort)SnesButton.Start) == 0;
            if (startEdge && previousFrame is FrontendFrame before)
            {
                int contextStart = Math.Max(0, frameIndex - 4);
                int contextEnd = Math.Min(recording.ControllerInputs.Length - 1, frameIndex + 8);
                string inputContext = string.Join(", ",
                    Enumerable.Range(contextStart, contextEnd - contextStart + 1)
                        .Select(index => $"{index}:${recording.ControllerInputs[index]:X4}"));
                Console.WriteLine($"  input context: {inputContext}");
                edgeCount++;
                // Sample the full native raster for frontend and gameplay presses alike.
                // The reported displacement can happen "anywhere", so excluding title or
                // file-select Start edges would discard the shortest exact reproduction.
                RasterTranslation translation = FindBestTranslation(
                    before.Pixels,
                    currentFrame.Pixels,
                    TranslationSampleStride);
                string stem = $"start-{frameIndex:D6}-{(ushort)stateBefore:X2}-{(ushort)currentFrame.GameState:X2}";
                PngWriter.WriteRgba(
                    Path.Combine(outputDirectory, stem + ".before.png"),
                    FrontendFrame.Width,
                    FrontendFrame.Height,
                    before.Pixels);
                PngWriter.WriteRgba(
                    Path.Combine(outputDirectory, stem + ".after.png"),
                    FrontendFrame.Width,
                    FrontendFrame.Height,
                    currentFrame.Pixels);

                SuperMetroidRuntime? runtimeAfter = game.RuntimeForVerification;
                Console.WriteLine(
                    $"Start edge {edgeCount} at recording frame {frameIndex}, input ${input:X4}: " +
                    $"state ${(ushort)stateBefore:X2}->{(ushort)currentFrame.GameState:X2}, " +
                    $"room {FormatRoom(runtimeBefore)}->{FormatRoom(runtimeAfter)}, " +
                    $"camera ({FormatWord(cameraXBefore)},{FormatWord(cameraYBefore)})->" +
                    $"({FormatWord(runtimeAfter?.Camera?.XPosition)},{FormatWord(runtimeAfter?.Camera?.YPosition)}), " +
                    $"best raster translation dx={translation.X}, dy={translation.Y}, " +
                    $"mean error={translation.MeanAbsoluteChannelError:F3}.");

                // The report concerns the visible pause entry, not necessarily the one
                // controller edge on which state $08 accepts Start. Preserve the first
                // retail gameplay edge and follow every subsequent rendered frame through
                // darkening, pause setup, and the first stable pause screen. Comparing
                // only before/after the edge previously disproved a Right controller bit,
                // but could not detect a delayed presentation displacement.
                if (followedGameplayStart is null &&
                    stateBefore == SuperMetroidGameState.MainGameplay)
                {
                    followedGameplayStart = frameIndex;
                    followedBaseline = before;
                }
            }

            if (followedGameplayStart is int gameplayStart &&
                followedBaseline is FrontendFrame baseline)
            {
                int offset = frameIndex - gameplayStart;
                if (offset is >= 0 and <= PauseFollowFrameCount)
                {
                    RasterTranslation translation = FindBestTranslation(
                        baseline.Pixels,
                        currentFrame.Pixels,
                        TranslationSampleStride);
                    string stem = $"pause-follow-{offset:D2}-{(ushort)currentFrame.GameState:X2}";
                    PngWriter.WriteRgba(
                        Path.Combine(outputDirectory, stem + ".png"),
                        FrontendFrame.Width,
                        FrontendFrame.Height,
                        currentFrame.Pixels);
                    Console.WriteLine(
                        $"  pause +{offset:D2}: state ${(ushort)currentFrame.GameState:X2}, " +
                        $"camera ({FormatWord(game.RuntimeForVerification?.Camera?.XPosition)}," +
                        $"{FormatWord(game.RuntimeForVerification?.Camera?.YPosition)}), " +
                        $"baseline translation dx={translation.X}, dy={translation.Y}, " +
                        $"mean error={translation.MeanAbsoluteChannelError:F3}.");
                }
            }

            previousFrame = currentFrame;
            previousInput = input;
            previousGameState = currentFrame.GameState;
            if (frameIndex >= lastStartEdge + 32)
                break;
        }

        if (edgeCount == 0)
            throw new InvalidDataException("Recording contains no rising Start edge.");
        Console.WriteLine($"Captured {edgeCount} Start edges in {Path.GetFullPath(outputDirectory)}.");
        return 0;
    }

    private static int FindLastStartEdge(ReadOnlySpan<ushort> inputs)
    {
        int last = -1;
        ushort previous = 0;
        for (int index = 0; index < inputs.Length; index++)
        {
            ushort input = inputs[index];
            if ((input & (ushort)SnesButton.Start) != 0 &&
                (previous & (ushort)SnesButton.Start) == 0)
            {
                last = index;
            }
            previous = input;
        }
        return last;
    }

    private static RasterTranslation FindBestTranslation(
        ReadOnlySpan<Rgba32> before,
        ReadOnlySpan<Rgba32> after,
        int sampleStride = 1)
    {
        if (before.Length != FrontendFrame.Width * FrontendFrame.Height ||
            after.Length != before.Length)
        {
            throw new InvalidDataException("Start audit requires two complete 256x224 frames.");
        }

        RasterTranslation best = new(0, 0, double.PositiveInfinity);
        for (int dy = -MaximumVerticalShift; dy <= MaximumVerticalShift; dy++)
        {
            for (int dx = -MaximumHorizontalShift; dx <= MaximumHorizontalShift; dx++)
            {
                long totalError = 0;
                int comparedChannels = 0;
                int firstY = Math.Max(0, -dy);
                int lastY = Math.Min(FrontendFrame.Height, FrontendFrame.Height - dy);
                int firstX = Math.Max(0, -dx);
                int lastX = Math.Min(FrontendFrame.Width, FrontendFrame.Width - dx);
                for (int y = firstY; y < lastY; y += sampleStride)
                {
                    int beforeRow = y * FrontendFrame.Width;
                    int afterRow = (y + dy) * FrontendFrame.Width;
                    for (int x = firstX; x < lastX; x += sampleStride)
                    {
                        Rgba32 source = before[beforeRow + x];
                        Rgba32 destination = after[afterRow + x + dx];
                        totalError += Math.Abs(source.R - destination.R);
                        totalError += Math.Abs(source.G - destination.G);
                        totalError += Math.Abs(source.B - destination.B);
                        comparedChannels += 3;
                    }
                }

                double meanError = (double)totalError / comparedChannels;
                if (meanError < best.MeanAbsoluteChannelError)
                    best = new RasterTranslation(dx, dy, meanError);
            }
        }
        return best;
    }

    private static string FormatRoom(SuperMetroidRuntime? runtime) =>
        runtime?.ActiveRoom is { } room ? $"${room.Pointer:X4}" : "----";

    private static string FormatWord(ushort? value) =>
        value is ushort word ? $"${word:X4}" : "----";

    private readonly record struct RasterTranslation(
        int X,
        int Y,
        double MeanAbsoluteChannelError);
}
