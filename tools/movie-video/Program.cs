using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.MovieVideo;

/// <summary>
/// Renders one supplied movie's replay as a captioned MP4 for visual review, in one pass from its first frame to its last.
/// The replay compares every frame with the native recording as it goes, so a video only exists for a replay that
/// matched. <c>tools/render-movie-videos.ps1</c> runs one of these per movie side by side.
/// </summary>
/// <remarks>Usage: <c>SuperMetroid.MovieVideo &lt;movie&gt; &lt;output.mp4&gt;</c>, run from the repository root.</remarks>
internal static class Program
{
    /// <summary>Renders the movie behind the no-dialog process boundary and returns a failing exit code on error.</summary>
    private static int Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine($"Usage: SuperMetroid.MovieVideo <{string.Join('|', Enum.GetNames<ReplayMovieId>())}> <output.mp4>");
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

    private static ReplayMovieId Movie(string name) =>
        Enum.TryParse(name, ignoreCase: false, out ReplayMovieId movie) && Enum.IsDefined(movie)
            ? movie
            : throw new ArgumentException($"'{name}' is not a replay movie; the movies are {string.Join(", ", Enum.GetNames<ReplayMovieId>())}.", nameof(name));
}
