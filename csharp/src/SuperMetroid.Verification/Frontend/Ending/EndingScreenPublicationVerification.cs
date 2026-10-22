using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks that the ending records its transfers and publishes its screen from them (#1358).</summary>
internal static class EndingScreenPublicationVerification
{
    /// <summary>
    /// A transfer covering an earlier one's words replaces it and a full clear forgets every transfer, so the record
    /// stays bounded; and a running ending survives a debugger round trip at the credits and at the post-credits logo,
    /// showing the same image, palette and sprite entries once its artwork is rebound.
    /// </summary>
    internal static void VerifyEndingScreenPublication()
    {
        var screen = new EndingScreen();
        screen.Record(EndingUploadKind.CreditsTilemap, 0, new ushort[] { 1, 2 });
        screen.Record(EndingUploadKind.TextTilemap, 1, new ushort[] { 3 });
        screen.Record(EndingUploadKind.CreditsTilemap, 0, new ushort[] { 4, 5 });
        AssertEqual(2, screen.Uploads.Count, "a transfer covering an earlier one's words replaces it");
        AssertEqual(EndingUploadKind.CreditsTilemap, screen.Uploads[^1].Kind, "the replacing transfer is replayed last");
        AssertTrue(screen.Uploads[^1].Words!.SequenceEqual(new ushort[] { 4, 5 }), "the replacing transfer keeps the words it wrote");
        screen.Clear();
        AssertEqual(0, screen.Uploads.Count, "a full clear forgets every transfer");

        var audio = new CartridgeAudioState();
        EndingCreditsState ending = InstalledFixtures.CreateEnding(audio, 3, 0);
        foreach (EndingCreditsPhase target in new[] { EndingCreditsPhase.Credits, EndingCreditsPhase.PostCreditsLogo })
        {
            for (int frame = 0; frame < 40_000 && ending.Stage.Phase != target; frame++)
            {
                ending.Step();
                audio.AdvanceFrame(default);
            }
            AssertEqual(target, ending.Stage.Phase, $"the ending reaches {target}");
            AssertTrue(ending.screen.Uploads.Count < 64, $"the ending's transfer record stays bounded at {target}");
            PpuMemorySnapshot shown = EndingCreditsPresentation.CaptureRenderSnapshot(ending).Memory;
            EndingCreditsState restored = RoundTrip(ending);
            restored.Flyaway.BindFlightArtwork(InstalledFixtures.IntroArtwork.CeresFlight);
            restored.Stage.BindMode7Artwork(RepositoryInstallation.Installation.LoadEndingMode7Art());
            restored.InstalledArtwork.BindFixtureObjectArtwork(RepositoryInstallation.Installation.LoadEndingObjectArt());
            restored.InstalledArtwork.BindPaletteArtwork(RepositoryInstallation.Installation.LoadEndingPalettes());
            restored.InstalledArtwork.BindPaletteFxColors(InstalledFixtures.Maps.RoomPaletteFx);
            restored.Stage.BindEndingFont(InstalledFixtures.Maps.EndingFont);
            restored.InstalledArtwork.BindStaffCredits(InstalledFixtures.Maps.StaffCredits);
            restored.InstalledArtwork.BindEndingText(InstalledFixtures.Maps.EndingText);
            PpuMemorySnapshot restoredShown = EndingCreditsPresentation.CaptureRenderSnapshot(restored).Memory;
            AssertTrue(shown.Vram.SequenceEqual(restoredShown.Vram), $"a restored ending shows the same image at {target}");
            AssertTrue(shown.Cgram.SequenceEqual(restoredShown.Cgram), $"a restored ending shows the same palette at {target}");
            AssertTrue(PlacedSpritePart.Of(shown.SpriteDraws).SequenceEqual(PlacedSpritePart.Of(restoredShown.SpriteDraws)),
                $"a restored ending places the same sprite entries at {target}");
        }
        Console.WriteLine("  Ending screen: the transfer record replaces and clears, and the ending survives a debugger round trip at the credits and the logo.");
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
