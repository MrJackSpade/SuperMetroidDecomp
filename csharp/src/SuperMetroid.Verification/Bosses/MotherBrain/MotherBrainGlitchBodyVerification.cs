using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// The Mother Brain glitch movie's snapshot, taken mid-fight, shows her body where she stands (#2632): its import reveals
/// her encounter's own body, placed with her, as her phase-two transition does, instead of a still image placed where the
/// room's background scroll puts it, off the screen.
/// </summary>
internal static class MotherBrainGlitchBodyVerification
{
    /// <summary>How many frames into the movie the check looks at her body: well into the fight.</summary>
    private const int Frames = 1000;

    /// <summary>Checks that the fight's frame shows her body's parts placed with her, not at the room's background scroll.</summary>
    internal static void VerifyGlitchMovieShowsMotherBrainsBody()
    {
        using var session = ConvertedMovieSession.Open(ReplayMovieId.MotherBrainGlitch, ReplayMovieFixture.Of(ReplayMovieId.MotherBrainGlitch).TraceDirectory,
            video: new Recording());
        CapturedFrontendFrame? last = null;
        for (int frame = 0; frame < Frames && session.NextInput() is { } input; frame++)
        {
            last = session.Game.StepCaptured(input);
            session.FrameCompleted();
        }
        var scene = (last ?? throw new InvalidOperationException("The movie ran no frame.")).Snapshot.Scene as LayeredScene
            ?? throw new InvalidOperationException("The fight's frame is not a layered scene.");
        GameplaySceneLayer gameplay = scene.Layers.ToArray().OfType<GameplaySceneLayer>().Single();
        var body = gameplay.BossBody ?? throw new InvalidOperationException("The fight's frame shows no boss body.");
        AssertTrue(body.Body.Parts.Length > 0, "her body shows a part");
        AssertTrue(body.Scroll != gameplay.View.BackgroundScroll, "her body is placed with her, not where the room's background scroll puts it");
    }

    /// <summary>Records nothing but builds the games with the installed audio, as a recorded replay's games are built.</summary>
    private sealed class Recording : IMovieRecorder
    {
        /// <inheritdoc/>
        public ClipAudioLibrary Audio { get; } =
            SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadAudioClips().Clips;
    }

    /// <summary>Runs the glitch movie's Mother Brain body check.</summary>
    [VerificationCommand("--mother-brain-glitch-body")]
    internal static int MotherBrainGlitchBodyCommand()
    {
        Suite(nameof(VerifyGlitchMovieShowsMotherBrainsBody), VerifyGlitchMovieShowsMotherBrainsBody);
        return 0;
    }
}
