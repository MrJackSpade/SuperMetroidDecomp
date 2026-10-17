using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;

/// <summary>Checks a falling crawler's owned 16.16 gravity: half a pixel per update up to four whole pixels.</summary>
internal static class CrawlerFallingGravityVerification
{
    /// <summary>
    /// From rest, eight gravity updates reach exactly four pixels per update and further updates hold it there; an
    /// upward (negative) velocity still gains gravity because the whole pixels compare as a signed word.
    /// </summary>
    internal static void VerifyCrawlerFallingGravity()
    {
        var state = new CrawlerEnemyState();
        for (int update = 1; update <= 8; update++)
        {
            state.ApplyFallingGravity();
            AssertEqual(SnesSignedSixteenSixteen.FromRaw(update * SnesSixteenSixteen.FromWhole(1) / 2), state.FallingYVelocity,
                $"gravity update {update} adds half a pixel");
        }
        state.ApplyFallingGravity();
        AssertEqual(SnesSignedSixteenSixteen.FromParts(4, 0), state.FallingYVelocity, "gravity stops at four whole pixels");

        state.FallingYVelocity = SnesSignedSixteenSixteen.FromParts(-2, 0);
        state.ApplyFallingGravity();
        AssertEqual(SnesSignedSixteenSixteen.FromParts(-2, 0x8000), state.FallingYVelocity, "an upward velocity gains gravity");
        Console.WriteLine("Crawler falling gravity: half-pixel steps, the four-pixel cap and the signed comparison match.");
    }

    /// <summary>Runs the focused crawler falling-gravity check.</summary>
    [VerificationCommand("--crawler-falling-gravity")]
    internal static int CrawlerFallingGravityCommand()
    {
        VerifyCrawlerFallingGravity();
        return 0;
    }
}
