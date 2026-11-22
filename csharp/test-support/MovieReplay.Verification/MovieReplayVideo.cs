using SuperMetroid.Core.Assets;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// Records one segment of a movie replay's port output: the frames from <c>firstFrame</c> on, for <c>frameCount</c>
/// frames, each rasterized as the desktop host presents it with a caption band below it naming the frame, its update,
/// its movie frame and the room, and each frame's mixed sound. The picture is encoded with ffmpeg into an MP4 with no
/// sound track and the sound is kept as raw PCM beside it, so a movie's segments join without re-encoding the picture
/// and with no gap or overlap in the sound.
/// </summary>
/// <remarks>
/// Each update yields one stereo frame at the session's PCM rate (<see cref="AudioSession.SampleRate"/>), so the video runs at exactly
/// 60 frames per second to stay aligned with the audio. Pixels are upscaled 4x with
/// nearest-neighbour sampling; the native image is never reduced. Frames before the segment still run, mix their sound
/// and are discarded, so the game reaches the segment exactly as the whole replay does.
/// </remarks>
internal sealed class MovieReplayVideo : IMovieRecorder, IDisposable
{
    private const int Scale = 4;
    private readonly string videoPath;
    private readonly string audioPath;
    private readonly long firstFrame;
    private readonly long endFrame;
    private readonly short[] samples = new short[AudioSession.PcmSamplesPerFrame];
    private FileStream? audio;
    private Process? encoder;
    private Extent16<ScreenPixels> size;

    /// <summary>Prepares the segment's picture at <paramref name="videoPath"/> and its sound beside it.</summary>
    /// <param name="videoPath">The MP4 the segment's picture is written to; its sound goes to the same path plus <c>.pcm</c>.</param>
    /// <param name="firstFrame">The first frame recorded, counting the movie's frames from zero.</param>
    /// <param name="frameCount">How many frames the segment records at most.</param>
    internal MovieReplayVideo(string videoPath, long firstFrame, long frameCount)
    {
        if (firstFrame < 0) throw new ArgumentOutOfRangeException(nameof(firstFrame), firstFrame, "A segment starts at frame zero or later.");
        if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount), frameCount, "A segment records at least one frame.");
        this.videoPath = Path.GetFullPath(videoPath);
        audioPath = this.videoPath + ".pcm";
        this.firstFrame = firstFrame;
        endFrame = firstFrame + frameCount;
        Directory.CreateDirectory(Path.GetDirectoryName(this.videoPath)!);
    }

    /// <summary>The synthesized audio a recorded game mixes, built in before its first update.</summary>
    public SuperMetroid.Core.Audio.ClipAudioLibrary Audio { get; } =
        SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadAudioClips().Clips;

    /// <summary>Whether every frame of the segment has been recorded.</summary>
    internal bool Done => FramesRun >= endFrame;

    /// <summary>How many frames have run, recorded or not.</summary>
    internal long FramesRun { get; private set; }

    /// <summary>Runs one production frame, and records its picture and sound when it lies in the segment.</summary>
    /// <param name="game">The game the replay steps.</param>
    /// <param name="input">The frame's controller input.</param>
    /// <param name="position">Where the frame comes from in the movie.</param>
    internal FrontendFrame Step(SuperMetroidGame game, ControllerButton input, MovieReplayPosition position)
    {
        long frame = FramesRun++;
        CapturedFrontendFrame captured = game.StepCaptured(input);
        game.CopyAudioFrame(samples);
        if (frame < firstFrame)
            return captured.Frame;
        DisplayPacket snapshot = captured.Snapshot;
        if (encoder is null)
        {
            size = snapshot.Size;
            encoder = StartEncoder(size);
            audio = File.Create(audioPath);
        }
        else if (snapshot.Size != size)
            throw new InvalidDataException($"Frame {frame} produced a {snapshot.Size.X}x{snapshot.Size.Y} picture; the video is {size.X}x{size.Y}.");
        RgbaPixel[] pixels = MovieReplayOverlay.Captioned(DisplayRendering.Render(snapshot), size.X, size.Y,
            FormattableString.Invariant($"FRAME {frame}  UPDATE {position.Update}  MOVIE {position.SourceFrame}"),
            Place(game));
        encoder.StandardInput.BaseStream.Write(MemoryMarshal.AsBytes(pixels.AsSpan()));
        audio!.Write(MemoryMarshal.AsBytes(samples.AsSpan()));
        return captured.Frame;
    }

    /// <summary>Finishes the segment's picture and sound.</summary>
    internal void Complete()
    {
        if (encoder is null)
            throw new InvalidOperationException($"The movie has {FramesRun} frames, so the segment from frame {firstFrame} recorded none.");
        encoder.StandardInput.Close();
        encoder.WaitForExit();
        if (encoder.ExitCode != 0) throw new InvalidOperationException($"ffmpeg video encoding failed with exit code {encoder.ExitCode}.");
        audio!.Flush();
        audio.Close();
        long recorded = FramesRun - firstFrame;
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"Wrote frames {firstFrame}-{FramesRun - 1} ({recorded} frames, {recorded / 60.0:F1} s) to {videoPath} and {audioPath}"));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        audio?.Dispose();
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
        int height = frameSize.Y + MovieReplayOverlay.Height;
        return Process.Start(new ProcessStartInfo("ffmpeg",
            $"-y -hide_banner -loglevel error -f rawvideo -pix_fmt rgba -s {frameSize.X}x{height} -framerate 60 -i - " +
            $"-vf scale={frameSize.X * Scale}:{height * Scale}:flags=neighbor -c:v libx264 -preset veryfast -crf 14 " +
            $"-pix_fmt yuv420p -an \"{videoPath}\"")
        { RedirectStandardInput = true, UseShellExecute = false })
            ?? throw new InvalidOperationException("ffmpeg did not start.");
    }
}
