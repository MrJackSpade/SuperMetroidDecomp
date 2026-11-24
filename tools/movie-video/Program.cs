using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.MovieVideo;

/// <summary>
/// Renders one supplied movie's replay as a captioned MP4 for visual review, in one pass from its first frame to its last.
/// The replay compares every frame with the native recording as it goes, so a video only exists for a replay that
/// matched. <c>tools/render-movie-videos.ps1</c> runs one of these per movie side by side.
/// </summary>
/// <remarks>
/// Usage: <c>SuperMetroid.MovieVideo &lt;movie&gt; &lt;output.mp4&gt;</c>, or <c>SuperMetroid.MovieVideo &lt;movie&gt;
/// &lt;output directory&gt; --frames &lt;frames&gt;</c> for PNGs of only the chosen movie frames, run from the repository root.
/// </remarks>
internal static class Program
{
    /// <summary>Renders the movie behind the no-dialog process boundary and returns a failing exit code on error.</summary>
    private static int Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            if (args is [var movie, var directory, "--frames", var frames])
            {
                Shoot(Movie(movie), new MovieFrameShots(directory, MovieFrameShots.Parse(frames)));
                return 0;
            }
            if (args.Length != 2)
            {
                Console.Error.WriteLine($"Usage: SuperMetroid.MovieVideo <{string.Join('|', Enum.GetNames<ReplayMovieId>())}> <output.mp4>");
                Console.Error.WriteLine("   or: SuperMetroid.MovieVideo <movie> <output directory> --frames <FIRST-LAST/STEP,FRAME,...>");
                return 2;
            }
            Render(Movie(args[0]), args[1]);
            return 0;
        }
        catch (Exception exception)
        {
            // A failing exit code keeps the failure in the console instead of the Windows error reporter's dialog.
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    /// <summary>Replays <paramref name="movie"/> from its first frame to its last, recording every frame to <paramref name="outputPath"/>.</summary>
    private static void Render(ReplayMovieId movie, string outputPath)
    {
        using var video = new MovieVideoRecorder(outputPath);
        using var session = ConvertedMovieSession.Open(movie, ReplayMovieFixture.Of(movie).TraceDirectory, video: video);
        var progress = new RenderProgress();
        while (session.NextInput() is { } input)
        {
            progress.Frame(session.Position);
            MovieReplayPosition position = session.Position
                ?? throw new InvalidOperationException("The replay handed a frame without saying where it comes from.");
            try
            {
                video.Step(session.Game, input, position);
            }
            catch (Exception exception) when (session.NamedProductionFailure(exception) is { } named)
            {
                throw named;
            }
            session.FrameCompleted();
        }
        Console.WriteLine(session.Finish());
        video.Complete();
    }

    /// <summary>Replays <paramref name="movie"/> until <paramref name="shots"/> has every chosen frame's picture.</summary>
    private static void Shoot(ReplayMovieId movie, MovieFrameShots shots)
    {
        using var session = ConvertedMovieSession.Open(movie, ReplayMovieFixture.Of(movie).TraceDirectory, video: shots);
        while (!shots.Done && session.NextInput() is { } input)
        {
            MovieReplayPosition position = session.Position
                ?? throw new InvalidOperationException("The replay handed a frame without saying where it comes from.");
            try
            {
                shots.Step(session.Game, input, position);
            }
            catch (Exception exception) when (session.NamedProductionFailure(exception) is { } named)
            {
                throw named;
            }
            session.FrameCompleted();
        }
        if (!shots.Done)
            throw new InvalidOperationException("The movie ended before every chosen frame was pictured.");
    }

    private static ReplayMovieId Movie(string name) =>
        Enum.TryParse(name, ignoreCase: false, out ReplayMovieId movie) && Enum.IsDefined(movie)
            ? movie
            : throw new ArgumentException($"'{name}' is not a replay movie; the movies are {string.Join(", ", Enum.GetNames<ReplayMovieId>())}.", nameof(name));
}
