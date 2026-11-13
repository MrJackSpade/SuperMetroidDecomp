using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Frontend;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Holds the title's and the ending explosion's sprite sheets to the rule that typed text is pictured one glyph at a time
/// (#1513). The title's cards and the ending's completion text and clear-time label are typed one letter per step: each
/// step of their programs shows the step before it and one more letter. A sheet that pictures every step as a whole frame
/// holds each letter once per later step, a repeating texture of ever longer prefixes ("THE OP", "THE OPE", ...). Each
/// glyph is held once and the typing places the glyphs it has typed, so no typed step is a frame of its own.
/// </summary>
internal static class TypedTextGlyphsVerification
{
    /// <summary>The ending's typed texts: the programs that type THE OPERATION WAS, COMPLETED SUCCESSFULLY and CLEAR TIME.</summary>
    private static readonly CinematicSpriteSegment[] EndingTypedPrograms =
    [
        EndingActorDefinitions.OperationWasText.Animation, EndingActorDefinitions.CompletedSuccessfullyText.Animation,
        EndingActorDefinitions.ClearTimeText.Animation,
    ];

    /// <summary>Fails, naming every typed step the title or ending explosion sheet pictures as a whole frame.</summary>
    internal static void VerifyTypedStepsNotPicturedWhole()
    {
        var findings = new List<string>();
        var typed = new List<TypedStepFrame>();
        foreach (TitleTextSequenceDefinition card in (TitleTextSequenceDefinition[])[TitleSequenceDefinitions.TextSequences.Year,
            TitleSequenceDefinitions.TextSequences.Nintendo, TitleSequenceDefinitions.TextSequences.Presents,
            TitleSequenceDefinitions.TextSequences.MetroidThree])
        {
            foreach (TitleCardStep step in TitleCards.Steps(card.Phase))
            {
                if (step.Sprite is { } sprite)
                    typed.Add(new(SpriteSheetId.Title, TitleSpriteDefinitions.FrameName(sprite)));
            }
        }
        foreach (CinematicSpriteSegment program in EndingTypedPrograms)
        {
            foreach (CinematicSpritePose pose in CinematicSpriteAnimations.Segments.FramesOf(CinematicSpriteAnimations.StartingWith(program)))
            {
                if (pose != CinematicSpritePose.Blank)
                    typed.Add(new(SpriteSheetId.EndingExplosionSprites, $"{CinematicSpriteFamily.EndingCompletionText}.{pose.Index}"));
            }
        }
        foreach (TypedStepFrame step in typed.Distinct())
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(step.Sheet);
            if (sheet.frames.ContainsKey(step.Frame) || sheet.blanks.Contains(step.Frame))
                findings.Add($"{step.Sheet} pictures typed step {step.Frame} as a whole frame");
        }
        // Each glyph a typed step adds is held, as a picture or as one that shows nothing (a space).
        var glyphs = new List<TypedStepFrame>();
        foreach (TitleSequencePhase card in TitleTypedCards.Cards)
        {
            for (int glyph = 0; glyph < TitleTypedCardSteps.Of(card).Count; glyph++)
                glyphs.Add(new(SpriteSheetId.Title, TitleSpriteDefinitions.GlyphFrameName(card, glyph)));
        }
        foreach (CinematicSpriteSegment text in EndingTypedTexts.Texts.Texts)
        {
            for (int glyph = 0; glyph < EndingTypedTexts.Texts.Steps(text).Count; glyph++)
                glyphs.Add(new(SpriteSheetId.EndingExplosionSprites, TypedTextSpriteSheet.GlyphFrameName(EndingTypedTexts.Family, text, glyph)));
        }
        foreach (TypedStepFrame glyph in glyphs)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(glyph.Sheet);
            if (!sheet.frames.ContainsKey(glyph.Frame) && !sheet.blanks.Contains(glyph.Frame))
                findings.Add($"{glyph.Sheet} holds no glyph {glyph.Frame}");
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} typed text steps are pictured whole:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Typed text: none of {typed.Distinct().Count()} typed steps of the title and the ending is pictured whole, and each of their {glyphs.Count} glyphs is held.");
    }

    /// <summary>A typed step's whole frame on a sheet.</summary>
    /// <param name="Sheet">The sheet the typing draws from.</param>
    /// <param name="Frame">The name the step's whole frame would hold.</param>
    private readonly record struct TypedStepFrame(SpriteSheetId Sheet, string Frame);

    /// <summary>Runs the typed text rule.</summary>
    [VerificationCommand("--typed-text-glyphs")]
    internal static int TypedTextGlyphsCommand()
    {
        Suite(nameof(VerifyTypedStepsNotPicturedWhole), VerifyTypedStepsNotPicturedWhole);
        return 0;
    }
}
