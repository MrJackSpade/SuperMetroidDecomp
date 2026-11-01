using SuperMetroid.TestSupport.MovieReplay;
using System.Globalization;
using SuperMetroid.TestSupport.MovieReplay.Verification;
namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Runs complete converted controller timelines against native checkpoints without importing later gameplay state.
/// Each replay is a <see cref="ConvertedMovieSession"/> the verifier steps headlessly, the same session the Direct3D 11
/// host drives from its own frame loop.
/// </summary>
internal static class FullMovieReplayVerification
{
    /// <summary>Checks the 100% power-on playthrough against its converted input trace and WRAM checkpoints.</summary>
    internal static void VerifyFullPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null, MovieReplayVideo? video = null) =>
        HeadlessMovieReplay.Run(ReplayMovieId.Full100, traceDirectory, traceFromUpdate, video);

    /// <summary>
    /// The 13% Speed Booster lsnes TAS, captured on bsnes v085. Its intro updates also check the
    /// phase handoffs and Samus milestones of <see cref="LowPercentIntroReplayChecks"/>.
    /// </summary>
    internal static void VerifyLowPercentPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null, MovieReplayVideo? video = null) =>
        HeadlessMovieReplay.Run(ReplayMovieId.LowPercent13, traceDirectory, traceFromUpdate, video);

    /// <summary>The #1275 "Samus drunk" Snes9x 1.60 power-on playthrough.</summary>
    internal static void VerifyDrunkPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null) =>
        HeadlessMovieReplay.Run(ReplayMovieId.Drunk, traceDirectory, traceFromUpdate);

    /// <summary>The contributed "lowc" Snes9x 1.60 power-on playthrough (208,183 frames, SRAM start).</summary>
    internal static void VerifyLowcPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null) =>
        HeadlessMovieReplay.Run(ReplayMovieId.Lowc, traceDirectory, traceFromUpdate);

    /// <summary>
    /// The #1275 Mother Brain glitch movie, which starts from a snapshot paused in her room. It also counts her
    /// library-two cries (<see cref="MotherBrainCryChecks"/>): every cry the cartridge starts must be one the game
    /// publishes (#1391).
    /// </summary>
    internal static void VerifyMotherBrainGlitchMovie(string traceDirectory, int? traceFromUpdate = null) =>
        HeadlessMovieReplay.Run(ReplayMovieId.MotherBrainGlitch, traceDirectory, traceFromUpdate);

    /// <summary>Runs the 13% lsnes replay command with the supplied native trace directory.</summary>
    [VerificationCommand("--lsmv-playthrough-movie", "lowTraceDirectory")]
    internal static int LsmvPlaythroughMovieCommand(string lowTraceDirectory)
    {
     VerifyLowPercentPlaythroughMovie(lowTraceDirectory); return 0;
    }

    /// <summary>Runs the 100% playthrough replay command with the supplied native trace directory.</summary>
    [VerificationCommand("--full-playthrough-movie", "fullTraceDirectory")]
    internal static int FullPlaythroughMovieCommand(string fullTraceDirectory)
    {
     VerifyFullPlaythroughMovie(fullTraceDirectory); return 0;
    }

    /// <summary>Replays and verifies the 13% movie while recording the port's picture and sound to an MP4.</summary>
    [VerificationCommand("--export-lsmv-playthrough-video", "lowTraceDirectory", "outputPath")]
    internal static int LsmvPlaythroughVideoCommand(string lowTraceDirectory, string outputPath)
    {
        using var video = new MovieReplayVideo(outputPath);
        VerifyLowPercentPlaythroughMovie(lowTraceDirectory, video: video);
        video.Complete();
        return 0;
    }

    /// <summary>Replays and verifies the 100% movie while recording the port's picture and sound to an MP4.</summary>
    [VerificationCommand("--export-full-playthrough-video", "fullTraceDirectory", "outputPath")]
    internal static int FullPlaythroughVideoCommand(string fullTraceDirectory, string outputPath)
    {
        using var video = new MovieReplayVideo(outputPath);
        VerifyFullPlaythroughMovie(fullTraceDirectory, video: video);
        video.Complete();
        return 0;
    }

    /// <summary>Runs the issue 1275 power-on Samus recording against its native trace.</summary>
    [VerificationCommand("--drunk-playthrough-movie", "drunkTraceDirectory")]
    internal static int DrunkPlaythroughMovieCommand(string drunkTraceDirectory)
    {
     VerifyDrunkPlaythroughMovie(drunkTraceDirectory); return 0;
    }

    /// <summary>Runs the contributed lowc power-on recording against its native trace.</summary>
    [VerificationCommand("--lowc-playthrough-movie", "lowcTraceDirectory")]
    internal static int LowcPlaythroughMovieCommand(string lowcTraceDirectory)
    {
        VerifyLowcPlaythroughMovie(lowcTraceDirectory); return 0;
    }

    /// <summary>Runs the lowc replay while printing port and native state from a chosen update.</summary>
    [VerificationCommand("--lowc-playthrough-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int LowcPlaythroughMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyLowcPlaythroughMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }

    /// <summary>Runs the issue 1275 snapshot-start Mother Brain recording against its native trace.</summary>
    [VerificationCommand("--mother-brain-glitch-movie", "glitchTraceDirectory")]
    internal static int MotherBrainGlitchMovieCommand(string glitchTraceDirectory)
    {
     VerifyMotherBrainGlitchMovie(glitchTraceDirectory); return 0;
    }

    /// <summary>Runs the 13% replay while printing port and native Samus state from a chosen update.</summary>
    [VerificationCommand("--lsmv-playthrough-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int LsmvPlaythroughMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyLowPercentPlaythroughMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }

    /// <summary>Runs the 100% replay while printing port and native Samus state from a chosen update.</summary>
    [VerificationCommand("--full-playthrough-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int FullPlaythroughMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyFullPlaythroughMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }

    /// <summary>Runs the Samus movement replay while printing port and native state from a chosen update.</summary>
    [VerificationCommand("--drunk-playthrough-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int DrunkPlaythroughMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyDrunkPlaythroughMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }

    /// <summary>Runs the Mother Brain replay while printing port and native state from a chosen update.</summary>
    [VerificationCommand("--mother-brain-glitch-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int MotherBrainGlitchMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyMotherBrainGlitchMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }
}
