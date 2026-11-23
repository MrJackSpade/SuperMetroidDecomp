using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.MovieVideo;

/// <summary>
/// Records a movie replay's port output into one MP4: every frame rasterized as the desktop host presents it, with a
/// caption band below it naming the frame, its update, its movie frame and the room, and every frame's mixed sound.
/// </summary>
/// <remarks>
/// Each frame yields one stereo frame of sound at the session's PCM rate (<see cref="AudioSession.SampleRate"/>), so the
/// video runs at exactly 60 frames per second to stay aligned with it. Pixels are upscaled 4x with nearest-neighbour
/// sampling; the native image is never reduced. The picture streams to ffmpeg as it is made, and the sound collects
/// beside the output until the replay ends, when the two are muxed.
/// </remarks>
internal sealed class MovieVideoRecorder : IMovieRecorder, IDisposable
{
    private const int Scale = 4;
    private readonly string outputPath;
    private readonly string videoPath;
    private readonly string audioPath;
    private readonly FileStream audio;
    private readonly short[] samples = new short[AudioSession.PcmSamplesPerFrame];
    private Process? encoder;
    private Extent16<ScreenPixels> size;
    private long frames;

    /// <summary>Prepares the recording of <paramref name="outputPath"/>, with its picture and sound collected beside it.</summary>
    /// <param name="outputPath">The MP4 to write.</param>
    internal MovieVideoRecorder(string outputPath)
    {
        this.outputPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(this.outputPath)!);
        videoPath = this.outputPath + ".video.mp4";
        audioPath = this.outputPath + ".audio.pcm";
        audio = File.Create(audioPath);
    }

    /// <summary>The synthesized audio a recorded game mixes, built in before its first update.</summary>
    public SuperMetroid.Core.Audio.ClipAudioLibrary Audio { get; } =
        SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadAudioClips().Clips;

    /// <summary>Runs one production frame and records its captioned picture and its sound.</summary>
    /// <param name="game">The game the replay steps.</param>
    /// <param name="input">The frame's controller input.</param>
    /// <param name="position">Where the frame comes from in the movie.</param>
    internal void Step(SuperMetroidGame game, ControllerButton input, MovieReplayPosition position)
    {
        long frame = frames++;
        CapturedFrontendFrame captured = game.StepCaptured(input);
        DisplayPacket snapshot = captured.Snapshot;
        if (encoder is null)
        {
            size = snapshot.Size;
            encoder = StartEncoder(size);
        }
        else if (snapshot.Size != size)
            throw new InvalidDataException($"Frame {frame} produced a {snapshot.Size.X}x{snapshot.Size.Y} picture; the video is {size.X}x{size.Y}.");
        RgbaPixel[] pixels = MovieVideoCaption.Captioned(DisplayRendering.Render(snapshot), size.X, size.Y,
            FormattableString.Invariant($"FRAME {frame}  UPDATE {position.Update}  MOVIE {position.SourceFrame}"),
            Place(game));
        encoder.StandardInput.BaseStream.Write(MemoryMarshal.AsBytes(pixels.AsSpan()));
        game.CopyAudioFrame(samples);
        audio.Write(MemoryMarshal.AsBytes(samples.AsSpan()));
    }

    /// <summary>Finishes the picture and muxes it with the recorded sound into the output file.</summary>
    internal void Complete()
    {
        if (encoder is null) throw new InvalidOperationException("The replay produced no frames.");
        encoder.StandardInput.Close();
        encoder.WaitForExit();
        if (encoder.ExitCode != 0) throw new InvalidOperationException($"ffmpeg video encoding failed with exit code {encoder.ExitCode}.");
        audio.Flush();
        audio.Close();
        RunFfmpeg($"-y -hide_banner -loglevel error -i \"{videoPath}\" -f s16le -ar {AudioSession.SampleRate} -ac 2 -i \"{audioPath}\" " +
            $"-map 0:v -map 1:a -c:v copy -c:a aac -b:a 256k -movflags +faststart \"{outputPath}\"");
        File.Delete(videoPath);
        File.Delete(audioPath);
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Wrote {frames} frames ({frames / 3600.0:F1} min) to {outputPath}"));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        audio.Dispose();
        encoder?.Dispose();
    }

    /// <summary>The caption's second line: the area and room Samus is in, and the game's state when it is not plain gameplay.</summary>
    private static string Place(SuperMetroidGame game)
    {
        SuperMetroidGameState state = game.GameState;
        string screen = state == SuperMetroidGameState.MainGameplay ? "" : Words(state.ToString());
        return game.GameplayActiveRoom is { } room
            ? $"{Words(room.Area.ToString())}: {Words(room.Name)}" + (screen.Length == 0 ? "" : $" - {screen}")
            : screen;
    }

    /// <summary><paramref name="name"/> with a space before each capital and digit run that starts a word.</summary>
    private static string Words(string name)
    {
        var words = new StringBuilder(name.Length + 8);
        for (int index = 0; index < name.Length; index++)
        {
            char c = name[index];
            if (c == '_')
            {
                words.Append(' ');
                continue;
            }
            bool startsWord = index > 0 && (char.IsUpper(c) && !char.IsUpper(name[index - 1])
                || char.IsDigit(c) && !char.IsDigit(name[index - 1]));
            if (startsWord)
                words.Append(' ');
            words.Append(c);
        }
        return words.ToString();
    }

    private Process StartEncoder(Extent16<ScreenPixels> frameSize)
    {
        int height = frameSize.Y + MovieVideoCaption.Height;
        return Process.Start(new ProcessStartInfo("ffmpeg",
            $"-y -hide_banner -loglevel error -f rawvideo -pix_fmt rgba -s {frameSize.X}x{height} -framerate 60 -i - " +
            $"-vf scale={frameSize.X * Scale}:{height * Scale}:flags=neighbor -c:v libx264 -preset veryfast -crf 14 " +
            $"-pix_fmt yuv420p -an \"{videoPath}\"")
        { RedirectStandardInput = true, UseShellExecute = false })
            ?? throw new InvalidOperationException("ffmpeg did not start.");
    }

    private static void RunFfmpeg(string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo("ffmpeg", arguments) { UseShellExecute = false })
            ?? throw new InvalidOperationException("ffmpeg did not start.");
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException($"ffmpeg mux failed with exit code {process.ExitCode}.");
    }
}
