using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks that pause keeps the HUD gameplay latched, not gameplay VRAM (#1351, #1466).</summary>
internal static class PauseHudPlaneVerification
{
    /// <summary>
    /// The HUD gameplay latched reaches pause's four retained HUD rows at entry; a later latched HUD replaces only the
    /// three drawn rows, keeping the static top row.
    /// </summary>
    internal static void VerifyPauseHudFromLayer3Plane()
    {
        ShownHud entered = Hud(1);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry
        {
            Samus = new SamusState(), System = new SharedGameState(), AreaIndex = AreaId.Crateria,
            RoomMapPosition = new(28, 1), GameplayHud = entered,
        });
        AssertTrue(HudWords(pause).SequenceEqual(HudWords(pause, entered)), "pause entry keeps the HUD's four rows");

        ShownHud accepted = Hud(7);
        pause.SynchronizeAcceptedHud(accepted);
        ushort[] shown = HudWords(pause);
        AssertTrue(shown[..Layer3Plane.Columns].SequenceEqual(HudWords(pause, entered)[..Layer3Plane.Columns]),
            "the static top row stays as pause entered");
        AssertTrue(shown[Layer3Plane.Columns..].SequenceEqual(HudWords(pause, accepted)[Layer3Plane.Columns..]),
            "the three drawn rows mirror the latched HUD");
        Console.WriteLine("  Pause HUD: entry and accepted HUD rows come from the HUD gameplay latched.");
    }

    /// <summary>The four HUD rows' words.</summary>
    private const int HudWordCount = Layer3Plane.HudRows * Layer3Plane.Columns;

    /// <summary>
    /// A HUD with its top row, its template and energy digits <paramref name="numeral"/> placed in the drawn rows, each
    /// image where the stock HUD's layout places the energy count.
    /// </summary>
    private static ShownHud Hud(int numeral) => new([
        new PlacedHudImage(HudTopRowImage.Instance, new(0, 0)),
        new PlacedHudImage(HudTemplateImage.Instance, new(0, 1)),
        new PlacedHudImage(new HudEnergyDigitImage(numeral), new(4, 3)),
        new PlacedHudImage(new HudEnergyDigitImage(numeral), new(5, 3)),
    ]);

    /// <summary>The HUD rows of pause's captured BG3 tilemap.</summary>
    private static ushort[] HudWords(PauseMenuState pause)
    {
        ReadOnlySpan<byte> vram = pause.ShownVram();
        var words = new ushort[HudWordCount];
        for (int i = 0; i < words.Length; i++)
        {
            int at = (GameplayVramLayoutConstants.HudTilemapWord + i) * 2;
            words[i] = LittleEndianBytes.Word(vram[at], vram[at + 1]);
        }
        return words;
    }

    /// <summary>The words <paramref name="hud"/>'s images lay out over pause's HUD rows, over the cleared cell.</summary>
    private static ushort[] HudWords(PauseMenuState pause, ShownHud hud) =>
        BackgroundTileWordBytes.Words(GameplayHudArtwork.Of(pause.screen.Artwork.GameplayHud).RowsOver(hud, PauseHudPlaneDefinitions.ClearedCell));

    /// <summary>Runs the pause HUD plane check.</summary>
    [VerificationCommand("--pause-hud-plane")]
    internal static int PauseHudPlaneCommand()
    {
        Suite(nameof(VerifyPauseHudFromLayer3Plane), () => VerifyPauseHudFromLayer3Plane());
        return 0;
    }
}
