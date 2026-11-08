using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// Holds every installed boss body sheet to the sheet pixel rule (#1513): no frame is empty, as
/// <see cref="SheetInkRules"/> defines it.
/// </summary>
internal static class BossBodySheetFrameRulesVerification
{
    /// <summary>Checks every installed boss body sheet's frames against the sheet pixel rules.</summary>
    internal static void VerifyBossBodySheetInks() =>
        SheetInkRules.Assert("boss body", RepositoryInstallation.Installation.BossBodySheetDirectory,
            [.. Enum.GetValues<BossBodyOwner>().Select(owner => new LabelledSheet(owner.ToString(), InstalledFixtures.BossBodySheets.Of(owner).Sheet))]);

    /// <summary>Runs the installed boss body sheets' pixel rules.</summary>
    [VerificationCommand("--boss-body-sheet-frame-rules")]
    internal static int BossBodySheetFrameRulesCommand()
    {
        Suite(nameof(VerifyBossBodySheetInks), VerifyBossBodySheetInks);
        return 0;
    }
}
