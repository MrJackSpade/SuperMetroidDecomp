using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks that pause keeps the gameplay HUD from the latched BG3 plane, not from gameplay VRAM (#1351).</summary>
internal static class PauseHudPlaneVerification
{
    /// <summary>
    /// A constructed HUD plane reaches pause's four retained HUD rows at entry; a later accepted plane replaces only the
    /// three mutable rows, keeping the static top row.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks the pause image's retained BG3 tilemap words")]
    internal static void VerifyPauseHudFromLayer3Plane()
    {
        var entered = Hud(0x2100);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry
        {
            Samus = new SamusState(), System = new SharedGameState(), AreaIndex = AreaId.Crateria,
            RoomMapPosition = new(28, 1), GameplayLayer3 = entered,
        });
        AssertTrue(HudWords(pause).SequenceEqual(entered.Words[..HudWordCount]), "pause entry keeps the plane's four HUD rows");

        var accepted = Hud(0x2200);
        pause.SynchronizeAcceptedHud(accepted);
        ReadOnlySpan<ushort> shown = HudWords(pause);
        AssertTrue(shown[..Layer3Plane.Columns].SequenceEqual(entered.Words[..Layer3Plane.Columns]),
            "the static top row stays as pause entered");
        AssertTrue(shown[Layer3Plane.Columns..].SequenceEqual(accepted.Words[Layer3Plane.Columns..HudWordCount]),
            "the three mutable rows mirror the accepted plane");
        Console.WriteLine("  Pause HUD: entry and accepted HUD rows come from the gameplay BG3 plane.");
    }

    /// <summary>The four HUD rows' words.</summary>
    private const int HudWordCount = Layer3Plane.HudRows * Layer3Plane.Columns;

    /// <summary>A BG3 plane whose HUD rows hold distinct words from <paramref name="first"/>.</summary>
    private static Layer3Plane Hud(ushort first)
    {
        var rows = new ushort[HudWordCount];
        for (int i = 0; i < rows.Length; i++) rows[i] = (ushort)(first + i);
        return Layer3Plane.Compose(rows, []);
    }

    /// <summary>The HUD rows of pause's captured BG3 tilemap.</summary>
    private static ushort[] HudWords(PauseMenuState pause)
    {
        ReadOnlySpan<byte> vram = pause.ShownVram();
        var words = new ushort[HudWordCount];
        for (int i = 0; i < words.Length; i++)
        {
            int at = (GameplayVramLayoutConstants.HudTilemapWord + i) * 2;
            words[i] = SnesLittleEndian.Word(vram[at], vram[at + 1]);
        }
        return words;
    }

    /// <summary>Runs the pause HUD plane check.</summary>
    [VerificationCommand("--pause-hud-plane")]
    internal static int PauseHudPlaneCommand()
    {
        Suite(nameof(VerifyPauseHudFromLayer3Plane), () => VerifyPauseHudFromLayer3Plane());
        return 0;
    }
}
