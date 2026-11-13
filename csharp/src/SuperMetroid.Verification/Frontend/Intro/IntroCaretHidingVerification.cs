using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks that the narration's caret is hidden, not moved below the screen, while an illustration owns it (#1625).</summary>
internal static class IntroCaretHidingVerification
{
    /// <summary>
    /// Takes the shown caret off the screen as an illustration's setup does: it shows no pose and the illustrated page draws
    /// nothing for it while its program runs on; the next narration page shows it again at the first text line.
    /// </summary>
    internal static void VerifyIntroCaretHiding()
    {
        IntroCinematicState intro = InstalledFixtures.CreateIntro();
        intro.SetupFirstIllustratedPage();
        IntroCinematicObjectSystem objects = intro.Objects;
        objects.Step(0);
        AssertTrue(objects.CaretPose is not null, "the narration's caret shows its program's pose");
        int shownDraws = intro.PrepareIllustratedPageSprites().Draws.Length;
        AssertTrue(shownDraws > 0, "the shown caret is drawn");

        objects.PlaceCaretOffScreen();
        for (int frame = 0; frame < 3; frame++)
        {
            AssertTrue(objects.CaretPose is null, $"the hidden caret shows no pose on hidden frame {frame}");
            AssertEqual(0, intro.PrepareIllustratedPageSprites().Draws.Length, $"the hidden caret is not drawn on hidden frame {frame}");
            objects.Step(0);
        }

        objects.StartEnglishPageTwo();
        AssertEqual(new Position16<ScreenPixels>(IntroCinematicDefinitions.ObjectSystem.CaretLeftX, IntroCinematicDefinitions.ObjectSystem.CaretFirstTextY),
            objects.Caret, "the next page restores the caret to the first text line");
        objects.Step(0);
        AssertTrue(objects.CaretPose is not null, "the next page shows the caret again");
        AssertEqual(shownDraws, intro.PrepareIllustratedPageSprites().Draws.Length, "the restored caret is drawn again");
        Console.WriteLine("Intro caret: hidden while an illustration owns the screen, drawn again on the next page.");
    }
}
