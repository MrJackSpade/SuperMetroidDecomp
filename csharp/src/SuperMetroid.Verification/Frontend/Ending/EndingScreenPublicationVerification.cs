using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks that the ending records its transfers and publishes its screen from them (#1358).</summary>
internal static class EndingScreenPublicationVerification
{
    /// <summary>
    /// A running ending survives a debugger round trip at the credits and at the post-credits logo, showing the same image,
    /// palette and sprite entries once its artwork is rebound.
    /// </summary>
    internal static void VerifyEndingScreenPublication()
    {
        var audio = AudioQueues.Standalone();
        EndingCreditsState ending = InstalledFixtures.CreateEnding(audio, 3, 0);
        foreach (EndingCreditsPhase target in new[] { EndingCreditsPhase.Credits, EndingCreditsPhase.PostCreditsLogo })
        {
            for (int frame = 0; frame < 40_000 && ending.Stage.Phase != target; frame++)
            {
                ending.Step();
                audio.AdvanceFrame(default);
            }
            AssertEqual(target, ending.Stage.Phase, $"the ending reaches {target}");
            FramePaint shown = EndingCreditsPresentation.CaptureRenderSnapshot(ending).Paint;
            byte[] shownVram = ending.ShownVram();
            EndingCreditsState restored = RoundTrip(ending);
            restored.InstalledArtwork.BindFixtureObjectArtwork(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingObjects);
            restored.InstalledArtwork.BindBackdrops(InstalledFixtures.EndingBackdrops);
            restored.InstalledArtwork.BindEndingText(InstalledFixtures.Maps.EndingText);
            FramePaint restoredShown = EndingCreditsPresentation.CaptureRenderSnapshot(restored).Paint;
            var shownRestored = restored.ShownVram();
            AssertTrue(shownVram.SequenceEqual(shownRestored), $"a restored ending shows the same image at {target}");
            AssertTrue(ending.screen.Colours.Shading == restored.screen.Colours.Shading && shown.Backdrop == restoredShown.Backdrop, $"a restored ending shows the same shading and backdrop at {target}");
            AssertTrue(PlacedSpritePart.Of(shown.SpriteDraws).SequenceEqual(PlacedSpritePart.Of(restoredShown.SpriteDraws)),
                $"a restored ending places the same sprite entries at {target}");
        }
        Console.WriteLine("  Ending screen: the ending survives a debugger round trip at the credits and the logo.");
    }

    /// <summary>A copy of <paramref name="ending"/> through the debugger state serializer.</summary>
    private static EndingCreditsState RoundTrip(EndingCreditsState ending)
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, ending);
        stream.Position = 0;
        return DebuggerObjectGraphSerializer.Deserialize<EndingCreditsState>(stream);
    }

    /// <summary>Runs the ending screen publication check.</summary>
    [VerificationCommand("--ending-screen")]
    internal static int EndingScreenCommand()
    {
        Suite(nameof(VerifyEndingScreenPublication), () => VerifyEndingScreenPublication());
        return 0;
    }
}
