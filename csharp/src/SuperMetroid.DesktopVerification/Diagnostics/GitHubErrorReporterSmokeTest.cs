using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Desktop.Audio;
using SuperMetroid.Desktop.Diagnostics;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.DesktopVerification.Diagnostics;

/// <summary>Deterministic audit of stable fingerprints and both deduplication layers.</summary>
public static class GitHubErrorReporterSmokeTest
{
    /// <summary>Checks stable error fingerprints, session/remote deduplication, captured failure context, and audio recovery using an in-memory issue client without contacting GitHub.</summary>
    public static GitHubErrorReporterSmokeTestResult Run()
    {
        VerifyAudioRecovery();
        string frameDiagnostics = VerifyFrameFailureSnapshot();
        var client = new RecordingIssueClient();
        Exception repeated = CaptureFixtureException("door callback $8F:B9A2 is untranslated");
        Exception existing = CaptureFixtureException("PLM instruction $84:BA6F is untranslated");

        string repeatedFingerprint;
        string duplicateFingerprint;
        using (var reporter = new GitHubErrorReporter("owner/private-repository", client))
        {
            var context = new GitHubErrorContext(
                "smoke-test frame boundary",
                FrameNumber: 42,
                GameState: "$08 MainGameplay",
                Phase: "Gameplay",
                ControllerInput: SnesButton.A,
                Room: "Climb",
                Door: "Flyway0",
                InputRecordingPath: "fixture.smrec",
                FrameDiagnostics: frameDiagnostics);
            Require(GitHubErrorReporter.CreateFingerprint(repeated, context) ==
                GitHubErrorReporter.CreateFingerprint(repeated, context with { FrameDiagnostics = "different frame and handler" }),
                "Diagnostic state changes must not split error fingerprints.");
            client.ExistingFingerprints.Add(GitHubErrorReporter.CreateFingerprint(existing, context));
            repeatedFingerprint = reporter.Report(repeated, context);
            duplicateFingerprint = reporter.Report(repeated, context);
            _ = reporter.Report(existing, context);
            var firstDispatch = new SuperMetroid.Core.UnmodeledBehaviorException(
                "grapple/type-C/bts-45", "block 12 (2,3)");
            var movedDispatch = new SuperMetroid.Core.UnmodeledBehaviorException(
                "grapple/type-C/bts-45", "block 99 (4,5)");
            var otherDispatch = new SuperMetroid.Core.UnmodeledBehaviorException(
                "grapple/type-C/bts-4F", "block 12 (2,3)");
            var otherRoom = context with { Room = "Draygon", InputRecordingPath = "new-run.smrec" };
            Require(GitHubErrorReporter.CreateFingerprint(firstDispatch, context) == GitHubErrorReporter.CreateFingerprint(movedDispatch, otherRoom),
                "Room-independent dispatch split by block/room location.");
            Require(GitHubErrorReporter.CreateFingerprint(firstDispatch, context) != GitHubErrorReporter.CreateFingerprint(otherDispatch, context),
                "Different BTS failures merged.");
            Require(GitHubErrorReporter.CreateFingerprint(existing, context) != GitHubErrorReporter.CreateFingerprint(existing, otherRoom),
                "Unclassified room-sensitive failure merged across rooms.");
            client.ExistingFingerprints.Add(GitHubErrorReporter.CreateFingerprint(firstDispatch, context));
            reporter.Report(firstDispatch, context);
            reporter.Report(firstDispatch, context with { FrameNumber = 43 });
            reporter.Report(movedDispatch, otherRoom);
            reporter.FlushAsync().GetAwaiter().GetResult();
        }

        Require(repeatedFingerprint == duplicateFingerprint,
            "Repeated exception did not retain one stable fingerprint.");
        Require(client.FindCalls.Count == 4,
            $"Expected four distinct occurrence lookups, got {client.FindCalls.Count}.");
        Require(client.Comments.Count == 3 && client.Comments.Any(body => body.Contains("new-run.smrec")),
            "Existing ticket did not receive new occurrence context, or repeated frames spammed comments.");
        Require(client.CreatedIssues.Count == 1,
            $"Expected one created issue after remote deduplication, got {client.CreatedIssues.Count}.");
        CreatedIssue created = client.CreatedIssues[0];
        Require(created.Title.Contains(repeatedFingerprint, StringComparison.Ordinal),
            "Created issue title omitted its stable fingerprint.");
        Require(created.Body.Contains($"supermetroid-error-id:{repeatedFingerprint}",
                StringComparison.Ordinal),
            "Created issue body omitted its machine-readable fingerprint marker.");
        Require(created.Body.Contains("- Room: Climb", StringComparison.Ordinal) &&
                created.Body.Contains("- Door: Flyway0", StringComparison.Ordinal) &&
                created.Body.Contains(repeated.ToString(), StringComparison.Ordinal),
            "Created issue omitted frame context or the complete exception.");
        Require(created.Body.Contains(frameDiagnostics, StringComparison.Ordinal),
            "Created issue omitted the exact before/failed-frame diagnostic payload.");

        return new GitHubErrorReporterSmokeTestResult(
            repeatedFingerprint,
            client.FindCalls.Count,
            client.CreatedIssues.Count);
    }

    /// <summary>Captures before and failed-frame Samus state and checks replay, build, and input details in the report.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static string VerifyFrameFailureSnapshot()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var colors = GameplayBasePaletteFiles.Read(new MemoryStream(GameplayBasePaletteFiles.Write(
            new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
                Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray(),
                Enumerable.Range(0, GameplayBasePaletteFormat.SpriteColorCount).Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray()))));
        var random = new CartridgeRandom();
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: colors, random: random);
        var samus = new SamusState { Pose = SamusPoseId.MovingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)) };
        runtime.Player.Samus = samus;
        samus.Drained.GetUpHandler = DrainedGetUpHandler.UnableToStand;
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 8, 1);
        samus.LiquidPhysics.FxType = RoomFxType.Acid;
        samus.LiquidPhysics.LavaAcidYPosition = 100;
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        game.AdoptRuntime(runtime);
        string before = game.CaptureGameplayFailureContext();
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 18, 1);
        string after = game.CaptureGameplayFailureContext();
        Require(before.Contains("animationFrame=8;") && !before.Contains("animationFrame=18;"),
            "Before-frame snapshot changed with mutable Samus state.");
        Require(after.Contains("animationFrame=18;") && after.Contains("animationTimer=1;") &&
            after.Contains("drainedHandler=UnableToStand;") && after.Contains("pose=$09;") && after.Contains("fx=Acid;"),
            "Failed-frame snapshot omitted the reported invalid animation and its installed handler.");
        string diagnostics = GameplayFrameFailureDiagnostics.Format(before, after, SnesButton.Up | SnesButton.A, "fixture.smrec", 70000);
        Require(diagnostics.Contains("Build:") && diagnostics.Contains("Core build ID:") &&
            diagnostics.Contains("Host build ID:") && diagnostics.Contains("Attempted input: $0880") &&
            diagnostics.Contains("zero-based): 70000") && diagnostics.Contains("fixture.smrec") &&
            diagnostics.Contains("Before frame:") && diagnostics.Contains("may be partially mutated"),
            "Failure diagnostic omitted build/replay provenance or snapshot ordering.");
        return diagnostics;
    }

    /// <summary>Checks that repeated audio failures preserve gameplay frames and do not prevent later audio attempts.</summary>
    private static void VerifyAudioRecovery()
    {
        var recovery = new AudioFrameRecovery();
        var client = new RecordingIssueClient();
        using var reporter = new GitHubErrorReporter("owner/private-repository", client);
        var context = new GitHubErrorContext("audio rendering/submission after completed gameplay frame");
        int completedFrames = 0;
        for (int frame = 0; frame < 3; frame++)
        {
            recovery.Run(() => throw new InvalidDataException("fixture missing PCM source"),
                error => reporter.Report(error, context));
            completedFrames++;
        }
        reporter.FlushAsync().GetAwaiter().GetResult();
        Require(completedFrames == 3 && client.CreatedIssues.Count == 1 && client.FindCalls.Count == 1,
            "Audio errors must preserve frame completion and deduplicate GitHub reports.");
        recovery.Run(() => throw new InvalidDataException("reporting disabled"), null);
        recovery.Run(() => throw new InvalidDataException("transport failure"),
            _ => throw new InvalidOperationException("fixture reporter failure"));
        bool resumed = false;
        recovery.Run(() => resumed = true, null);
        Require(resumed, "Audio must resume attempts after a failed frame or report.");
        Console.WriteLine("PASS audio recovery: repeated failure, reporting disabled, failed reporter, deduplication and subsequent successful frame.");
    }

    /// <summary>Creates a caught exception with the requested message for stable fingerprint comparisons.</summary>
    private static Exception CaptureFixtureException(string message)
    {
        try
        {
            throw new InvalidDataException(message);
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    /// <summary>Raises a fixture failure when an expected reporter property is absent.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    /// <summary>Records issue lookups, creations, and comments locally for reporter deduplication checks.</summary>
    private sealed class RecordingIssueClient : IGitHubIssueClient
    {
        /// <summary>Comment bodies sent for distinct occurrences of an existing issue.</summary>
        public List<string> Comments { get; } = [];
        /// <summary>Records a reporter comment without contacting GitHub.</summary>
        public Task CommentAsync(string repository, string issueUrl, string body)
        {
            Comments.Add(body);
            return Task.CompletedTask;
        }
        /// <summary>Fingerprints the fake remote repository reports as already present.</summary>
        public HashSet<string> ExistingFingerprints { get; } = new(StringComparer.Ordinal);

        /// <summary>Fingerprint lookup calls made by the reporter worker.</summary>
        public List<string> FindCalls { get; } = [];

        /// <summary>Issue titles and bodies created for previously unseen fingerprints.</summary>
        public List<CreatedIssue> CreatedIssues { get; } = [];

        /// <summary>Records a lookup and returns a fixture URL for an existing fingerprint.</summary>
        public Task<string?> FindByFingerprintAsync(string repository, string fingerprint)
        {
            FindCalls.Add(fingerprint);
            string? result = ExistingFingerprints.Contains(fingerprint)
                ? $"https://github.example/{repository}/issues/1"
                : null;
            return Task.FromResult(result);
        }

        /// <summary>Records a new issue and returns its fixture URL.</summary>
        public Task<string> CreateAsync(string repository, string title, string body)
        {
            CreatedIssues.Add(new CreatedIssue(title, body));
            return Task.FromResult($"https://github.example/{repository}/issues/2");
        }
    }

    /// <summary>Title and body captured from one fake issue creation.</summary>
    private sealed record CreatedIssue(string Title, string Body);
}
