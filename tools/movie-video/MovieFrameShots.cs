using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.MovieVideo;

/// <summary>
/// Writes the port's picture of chosen movie frames as PNGs, at the console's own size, without rendering the rest of the
/// movie: every other frame is stepped and not drawn, and the replay stops after the last chosen frame. Each picture is
/// the first update the replay runs for its movie frame, or for the first frame after it a lagging movie frame folds into.
/// </summary>
internal sealed class MovieFrameShots : IMovieRecorder
{
    /// <summary>
    /// The synthesized audio a recorded game mixes, built in before its first update: a session records with a recorder,
    /// whose games render their gameplay frames.
    /// </summary>
    public SuperMetroid.Core.Audio.ClipAudioLibrary Audio { get; } =
        SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadAudioClips().Clips;

    private readonly string directory;
    private readonly SortedSet<long> pending;

    /// <summary>Prepares pictures of <paramref name="frames"/> in <paramref name="directory"/>.</summary>
    internal MovieFrameShots(string directory, IEnumerable<long> frames)
    {
        this.directory = Path.GetFullPath(directory);
        Directory.CreateDirectory(this.directory);
        pending = [.. frames];
        if (pending.Count == 0) throw new ArgumentException("No frames were chosen.", nameof(frames));
    }

    /// <summary>Whether every chosen frame has its picture.</summary>
    internal bool Done => pending.Count == 0;

    /// <summary>Runs one production frame, writing its picture when it is the next chosen frame or follows it.</summary>
    internal void Step(SuperMetroidGame game, ControllerButton input, MovieReplayPosition position)
    {
        long source = position.SourceFrame;
        if (pending.Count == 0 || source < pending.Min)
        {
            _ = game.StepCaptured(input);
            return;
        }
        long chosen = pending.Min;
        while (pending.Count > 0 && pending.Min <= source)
            pending.Remove(pending.Min);
        DisplayPacket snapshot = game.StepCaptured(input).Snapshot;
        string path = Path.Combine(directory, string.Create(CultureInfo.InvariantCulture, $"port-{chosen}.png"));
        WritePng(path, DisplayRendering.Render(snapshot), snapshot.Size.X, snapshot.Size.Y);
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Wrote movie frame {chosen} (update {position.Update}, movie frame {source}) to {path}"));
    }

    /// <summary>Parses a frame list: numbers and ranges FIRST-LAST, a range taking every STEPth with /STEP, comma separated.</summary>
    internal static IEnumerable<long> Parse(string spec)
    {
        foreach (string item in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] stepped = item.Split('/');
            long step = stepped.Length == 2 ? long.Parse(stepped[1], CultureInfo.InvariantCulture) : 1;
            if (step <= 0) throw new ArgumentException($"A range's step must be positive: {item}.", nameof(spec));
            string[] range = stepped[0].Split('-');
            long first = long.Parse(range[0], CultureInfo.InvariantCulture);
            long last = range.Length == 2 ? long.Parse(range[1], CultureInfo.InvariantCulture) : first;
            if (last < first) throw new ArgumentException($"A range must not run backwards: {item}.", nameof(spec));
            for (long frame = first; frame <= last; frame += step)
                yield return frame;
        }
    }

    /// <summary>Writes <paramref name="pixels"/>, <paramref name="width"/> by <paramref name="height"/>, losslessly as a PNG through ffmpeg.</summary>
    private static void WritePng(string path, RgbaPixel[] pixels, int width, int height)
    {
        var start = new ProcessStartInfo("ffmpeg",
            string.Create(CultureInfo.InvariantCulture, $"-y -hide_banner -loglevel error -f rawvideo -pix_fmt rgba -s {width}x{height} -i - -frames:v 1 \"{path}\""))
        {
            RedirectStandardInput = true,
            UseShellExecute = false,
        };
        using Process ffmpeg = Process.Start(start) ?? throw new InvalidOperationException("ffmpeg did not start.");
        ffmpeg.StandardInput.BaseStream.Write(MemoryMarshal.AsBytes(pixels.AsSpan()));
        ffmpeg.StandardInput.Close();
        ffmpeg.WaitForExit();
        if (ffmpeg.ExitCode != 0) throw new InvalidOperationException($"ffmpeg failed writing {path} with exit code {ffmpeg.ExitCode}.");
    }
}
