using System.Diagnostics;
using System.Globalization;
using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.MovieVideo;

/// <summary>
/// Reports a render's progress every <see cref="Interval"/> frames: the frames rendered, the movie frame reached, and the
/// frame rate over the last interval and since the first frame. tools/render-movie-videos.ps1 shows each movie's latest
/// report.
/// </summary>
internal sealed class RenderProgress
{
    /// <summary>The frames between reports: ten seconds of video.</summary>
    internal const int Interval = 600;

    /// <summary>Time since the first frame started.</summary>
    private readonly Stopwatch total = new();

    /// <summary>Time since the last report.</summary>
    private readonly Stopwatch interval = new();

    /// <summary>The frames started so far.</summary>
    private long frames;

    /// <summary>Counts a frame about to run, and reports after every <see cref="Interval"/> frames.</summary>
    /// <param name="position">Where the frame comes from in the movie.</param>
    internal void Frame(MovieReplayPosition? position)
    {
        if (frames == 0)
        {
            total.Start();
            interval.Start();
        }
        else if (frames % Interval == 0)
        {
            double recent = Interval / interval.Elapsed.TotalSeconds;
            double overall = frames / total.Elapsed.TotalSeconds;
            Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"progress: frame {frames}, movie frame {position?.SourceFrame}, {recent:F0} fps now, {overall:F0} fps average"));
            interval.Restart();
        }
        frames++;
    }
}
