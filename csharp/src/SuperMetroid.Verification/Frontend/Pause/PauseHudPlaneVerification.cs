using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
    /// The HUD gameplay latched reaches pause's four HUD rows at entry; a later latched HUD replaces it whole.
    /// </summary>
    internal static void VerifyPauseHudFromLayer3Plane()
    {
        ShownHud entered = Hud(1);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry
        {
            Samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria,
            RoomMapPosition = new(28, 1), GameplayHud = entered,
        });
        AssertTrue(HudWords(pause).SequenceEqual(HudWords(entered)), "pause entry keeps the HUD's four rows");

        ShownHud accepted = Hud(7);
        pause.SynchronizeAcceptedHud(accepted);
        AssertTrue(ReferenceEquals(accepted, pause.screen.Hud), "pause shows the HUD gameplay latched since, whole (#1626)");
        AssertTrue(HudWords(pause).SequenceEqual(HudWords(accepted)), "pause's four HUD rows are the latched HUD's");
        Console.WriteLine("  Pause HUD: entry and accepted HUDs are the HUD gameplay latched.");
    }

    /// <summary>The four HUD rows' words.</summary>
    private const int HudWordCount = FxOverlayPlaneConstants.HudRows * FxOverlayPlaneConstants.Columns;

    /// <summary>
    /// A HUD with its top row, its template and energy digits <paramref name="numeral"/> placed in the drawn rows, each
    /// image where the stock HUD's layout places the energy count.
    /// </summary>
    private static ShownHud Hud(int numeral) => new([
        new PlacedHudImage(HudTopRowImage.Instance, new(0, 0)),
        new PlacedHudImage(HudTemplateImage.Instance, new(0, 8)),
        new PlacedHudImage(new HudEnergyDigitImage(numeral), new(32, 24)),
        new PlacedHudImage(new HudEnergyDigitImage(numeral), new(40, 24)),
    ]);

    /// <summary>The HUD rows of pause's captured BG3 tilemap.</summary>
    private static ushort[] HudWords(PauseMenuState pause)
    {
        ReadOnlySpan<byte> vram = pause.ShownVram(RetailPresentationFixture());
        var words = new ushort[HudWordCount];
        for (int i = 0; i < words.Length; i++)
        {
            int at = (GameplayVramLayoutConstants.HudTilemapWord + i) * 2;
            words[i] = LittleEndianBytes.Word(vram[at], vram[at + 1]);
        }
        return words;
    }

    /// <summary>
    /// The words <paramref name="hud"/>'s images lay out over pause's HUD rows, over the cleared cell, from the HUD artwork
    /// of the installation's map presentation, which the retail pause fixture binds.
    /// </summary>
    private static ushort[] HudWords(ShownHud hud) =>
        BackgroundTileWordBytes.Words(InstalledFixtures.ImportOf(RetailPresentationFixture()).GameplayHud.RowsOver(hud, PauseHudPlaneDefinitions.ClearedCell));

    /// <summary>Runs the pause HUD plane check.</summary>
    [VerificationCommand("--pause-hud-plane")]
    internal static int PauseHudPlaneCommand()
    {
        Suite(nameof(VerifyPauseHudFromLayer3Plane), () => VerifyPauseHudFromLayer3Plane());
        return 0;
    }
}
