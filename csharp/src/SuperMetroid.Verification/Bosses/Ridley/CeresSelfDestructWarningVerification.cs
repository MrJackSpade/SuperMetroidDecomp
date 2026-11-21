using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks the waits of the Ceres self-destruct warning's phases (#2368).</summary>
internal static class CeresSelfDestructWarningVerification
{
    /// <summary>
    /// Checks the Japanese subtitle shows on the 32nd update after the title's hold, as the cartridge's own phase countdown
    /// shows it, while the typewriter types on its own schedule: the subtitle once waited on the typewriter's counter and
    /// showed on the fourth update.
    /// </summary>
    internal static void VerifyCeresJapaneseSubtitleWait()
    {
        RoomEnemySystem enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        enemies.Room.Session.Language = TextLanguage.Japanese;
        CeresEscape escape = enemies.CeresEscape;
        escape.BeginSelfDestruct();
        // Every character the typewriter types, a space or a line's first among them, moves the corner the next is typed at.
        int typedCharacters = 0;
        // Before the typewriter exists, and on a new one, the corner is the origin, which no typed character leaves it at.
        Position16<PlanePixels> corner = escape.Warning.Typewriter?.Corner ?? default;
        void CountTypedCharacter()
        {
            Position16<PlanePixels> next = escape.Warning.Typewriter?.Corner ?? default;
            if (next != corner)
                typedCharacters++;
            corner = next;
        }
        for (int update = 0; escape.Warning.Phase != CeresSelfDestructPhase.JapaneseSubtitle; update++)
        {
            if (update == 4096)
                throw new InvalidOperationException("The Japanese warning never reached its subtitle phase.");
            escape.RunRidleysStep(0);
            CountTypedCharacter();
        }

        for (int update = 1; update <= CeresSelfDestructWarningDefinitions.JapaneseSubtitleWaitUpdates; update++)
        {
            escape.RunRidleysStep(0);
            CountTypedCharacter();
            bool last = update == CeresSelfDestructWarningDefinitions.JapaneseSubtitleWaitUpdates;
            AssertEqual(last, ShowsSubtitle(escape.Warning),
                $"the Japanese subtitle shows on the 32nd update of its wait only, not on update {update}");
            AssertEqual(last ? CeresSelfDestructPhase.TypingWarning : CeresSelfDestructPhase.JapaneseSubtitle, escape.Warning.Phase,
                $"the warning types on after the subtitle shows, on update {update}");
        }
        // Over the 32 updates the typewriter typed its first two characters on consecutive updates and every third after.
        AssertTrue(escape.Warning.Typewriter is not null, "The Japanese warning types its text.");
        AssertEqual(2 + (CeresSelfDestructWarningDefinitions.JapaneseSubtitleWaitUpdates - 2) / 3, typedCharacters,
            "the typewriter keeps its own schedule through the subtitle's wait");
        Console.WriteLine("Ceres Japanese subtitle: it shows on the 32nd update after the title's hold while the warning types.");
    }

    /// <summary>Whether <paramref name="warning"/> shows any row of the Japanese subtitle.</summary>
    private static bool ShowsSubtitle(CeresSelfDestructWarning warning) =>
        warning.Overlay.Pictures.Any(written => CeresEscapeWarningPictures.JapaneseSubtitle.Any(line => line.Picture == written.Picture));

    /// <summary>Runs the Ceres self-destruct warning's wait check.</summary>
    [VerificationCommand("--ceres-self-destruct-warning")]
    internal static int CeresSelfDestructWarningCommand()
    {
        Suite(nameof(VerifyCeresJapaneseSubtitleWait), VerifyCeresJapaneseSubtitleWait);
        return 0;
    }
}
