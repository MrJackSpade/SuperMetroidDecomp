using System.Globalization;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.MovieReplay.Verification;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Records the supplied movies' replays as captioned video for visual review. A movie is recorded in segments, each in a
/// run of its own, and <c>tools/render-movie-videos.ps1</c> plans the segments, runs them side by side and joins them.
/// Every recorded frame is still compared with the native recording, so a video only exists for a replay that matched.
/// </summary>
internal static class MovieVideoExportVerification
{
    /// <summary>
    /// Records frames <paramref name="firstFrame"/> on, for <paramref name="frameCount"/> frames, of
    /// <paramref name="movie"/>'s replay into <paramref name="videoPath"/> and its sound beside it.
    /// </summary>
    [VerificationCommand("--export-movie-video", "movie", "videoPath", "firstFrame", "frameCount")]
    internal static int ExportMovieVideoCommand(string movie, string videoPath, string firstFrame, string frameCount)
    {
        using var video = new MovieReplayVideo(videoPath,
            long.Parse(firstFrame, CultureInfo.InvariantCulture), long.Parse(frameCount, CultureInfo.InvariantCulture));
        HeadlessMovieReplay.Record(Movie(movie), video);
        return 0;
    }

    /// <summary>
    /// Prints how many frames <paramref name="movie"/>'s replay runs, which is its video's length, and the sample rate of
    /// the stereo 16-bit sound each recorded segment writes beside its picture.
    /// </summary>
    [VerificationCommand("--count-movie-video-frames", "movie")]
    internal static int CountMovieVideoFramesCommand(string movie)
    {
        long frames = HeadlessMovieReplay.FrameCount(Movie(movie));
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Video frames: {frames}"));
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Sound sample rate: {SuperMetroid.Core.Audio.AudioSession.SampleRate}"));
        return 0;
    }

    private static ReplayMovieId Movie(string name) =>
        Enum.TryParse(name, ignoreCase: false, out ReplayMovieId movie) && Enum.IsDefined(movie)
            ? movie
            : throw new ArgumentException($"'{name}' is not a replay movie; the movies are {string.Join(", ", Enum.GetNames<ReplayMovieId>())}.", nameof(name));
}
