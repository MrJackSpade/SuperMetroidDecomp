using SuperMetroid.Core.Assets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Records a movie replay's port output: every production update is captured, rasterized and
/// mixed exactly as the desktop host presents it, then encoded with ffmpeg into one MP4.
/// </summary>
/// <remarks>
/// Each update yields one 48-kHz stereo frame of 800 samples, so the video runs at exactly
/// 60 frames per second to stay aligned with the audio. Pixels are upscaled 4x with
/// nearest-neighbour sampling; the native image is never reduced.
/// </remarks>
internal sealed class MovieReplayVideo : IDisposable
{
    private const int Scale = 4;
    private readonly string outputPath;
    private readonly string videoPath;
    private readonly string audioPath;
    private readonly FileStream audio;
    private readonly short[] samples = new short[AudioSession.PcmSamplesPerFrame];
    private readonly ClipAudioLibrary clips = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadAudioClips();
    private Process? encoder;
    private Extent16<ScreenPixels> size;
    private long sequence;

    /// <summary>Prepares temporary streams beside <paramref name="outputPath"/>.</summary>
    internal MovieReplayVideo(string outputPath)
    {
        this.outputPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(this.outputPath)!);
        videoPath = this.outputPath + ".video.mp4";
        audioPath = this.outputPath + ".audio.pcm";
        audio = File.Create(audioPath);
    }

    /// <summary>Binds synthesized audio to a freshly created game before its first update.</summary>
    internal void Attach(SuperMetroidGame game) => game.BindAudio(clips);

    /// <summary>Runs one production update and appends its picture and sound.</summary>
    internal FrontendFrame Step(SuperMetroidGame game, ControllerButton input)
    {
        CapturedFrontendFrame captured = game.StepCaptured(input, ++sequence, 1);
        DisplayPacket snapshot = captured.Snapshot;
        RgbaPixel[] pixels = DisplayRendering.Render(snapshot);
        if (encoder is null)
        {
            size = snapshot.Size;
            encoder = StartEncoder(size);
        }
        else if (snapshot.Size != size)
            throw new InvalidDataException($"Update {sequence} produced a {snapshot.Size.X}x{snapshot.Size.Y} frame; the video is {size.X}x{size.Y}.");
        encoder.StandardInput.BaseStream.Write(MemoryMarshal.AsBytes(pixels.AsSpan()));
        game.CopyAudioFrame(samples);
        audio.Write(MemoryMarshal.AsBytes(samples.AsSpan()));
        return captured.Frame;
    }

    /// <summary>Finishes the video stream and muxes it with the recorded audio into the output file.</summary>
    internal void Complete()
    {
        if (encoder is null) throw new InvalidOperationException("The replay produced no frames.");
        encoder.StandardInput.Close();
        encoder.WaitForExit();
        if (encoder.ExitCode != 0) throw new InvalidOperationException($"ffmpeg video encoding failed with exit code {encoder.ExitCode}.");
        audio.Flush();
        audio.Close();
        RunFfmpeg($"-y -hide_banner -loglevel error -i \"{videoPath}\" -f s16le -ar 48000 -ac 2 -i \"{audioPath}\" " +
            $"-map 0:v -map 1:a -c:v copy -c:a aac -b:a 256k -movflags +faststart \"{outputPath}\"");
        File.Delete(videoPath);
        File.Delete(audioPath);
        Console.WriteLine($"Wrote {sequence} frames ({sequence / 60.0:F1} s) to {outputPath}");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        audio.Dispose();
        encoder?.Dispose();
    }

    private Process StartEncoder(Extent16<ScreenPixels> frameSize) =>
        Process.Start(new ProcessStartInfo("ffmpeg",
            $"-y -hide_banner -loglevel error -f rawvideo -pix_fmt rgba -s {frameSize.X}x{frameSize.Y} -framerate 60 -i - " +
            $"-vf scale={frameSize.X * Scale}:{frameSize.Y * Scale}:flags=neighbor -c:v libx264 -preset medium -crf 14 " +
            $"-pix_fmt yuv420p \"{videoPath}\"")
        { RedirectStandardInput = true, UseShellExecute = false })
        ?? throw new InvalidOperationException("ffmpeg did not start.");

    private static void RunFfmpeg(string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo("ffmpeg", arguments) { UseShellExecute = false })
            ?? throw new InvalidOperationException("ffmpeg did not start.");
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException($"ffmpeg mux failed with exit code {process.ExitCode}.");
    }
}
