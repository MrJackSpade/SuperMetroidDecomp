using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>Runs the crawler animations, the orange Zoomer's among them, with the installed artwork.</summary>
internal static class HZoomerArtworkVerification
{
    /// <summary>Every crawler pose, the orange Zoomer's included, has installed artwork and runs through the real behavior.</summary>
    internal static void VerifyInstalledHZoomerInstructionFrames(
        InstalledEnemyArtwork stock) =>
        Suite(nameof(CrawlerBehaviorVerification.VerifyCrawlerBehavior), () => CrawlerBehaviorVerification.VerifyCrawlerBehavior(stock));
}
