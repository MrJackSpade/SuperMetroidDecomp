using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

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
    /// <summary>Fails, naming every typed step the title or ending explosion sheet pictures as a whole frame.</summary>
    internal static void VerifyTypedStepsNotPicturedWhole()
    {
        var findings = new List<string>();
        var typed = new List<TypedStepFrame>();
        // The title sheet holds the logo and the copyright whole and each card's letters (#1979); any other frame would picture
        // a card's typing step whole.
        var titleFrames = new HashSet<string>(StringComparer.Ordinal)
        {
            TitleSpriteDefinitions.FrameName(TitleSprite.SuperMetroidLogo), TitleSpriteDefinitions.FrameName(TitleSprite.NintendoCopyright),
        };
        foreach (TitleCardDefinition card in TitleCards.All)
        {
            foreach (TitleCardTyping step in TitleCards.Typings(card.Card))
                titleFrames.Add(TitleSpriteDefinitions.LetterFrameName(step));
        }
        SpriteSheet titleSheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.Title);
        foreach (string frame in titleSheet.frames.Keys.Concat(titleSheet.blanks).Where(frame => !titleFrames.Contains(frame)))
            findings.Add($"{SpriteSheetId.Title} pictures {frame}, which is no whole composition or letter");
        // The ending's result text family holds its digits and colon whole and each typed text's letters (#2195); any other
        // frame of the family would picture a typing step whole.
        var resultFrames = new HashSet<string>(StringComparer.Ordinal);
        foreach (CinematicSpriteFrameDefinition whole in EndingCompletionTextSpriteDefinitions.Frames)
            resultFrames.Add(CinematicSpriteSheet.FrameName(EndingTextAnimations.Family, whole.Pose));
        foreach (EndingTypedText text in Enum.GetValues<EndingTypedText>())
        {
            foreach (EndingTextTyping step in EndingTextAnimations.Typings(text))
            {
                resultFrames.Add(EndingTypedTextDrawing.LetterFrameName(step));
                typed.Add(new(SpriteSheetId.EndingExplosionSprites, EndingTypedTextDrawing.LetterFrameName(step)));
            }
        }
        SpriteSheet explosionSheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.EndingExplosionSprites);
        foreach (string frame in explosionSheet.frames.Keys.Concat(explosionSheet.blanks)
            .Where(frame => frame.StartsWith($"{EndingTextAnimations.Family}.", StringComparison.Ordinal) && !resultFrames.Contains(frame)))
            findings.Add($"{SpriteSheetId.EndingExplosionSprites} pictures {frame}, which is no whole digit, colon or letter");
        // Each glyph a typed step adds is held, as a picture or as one that shows nothing (a space).
        var glyphs = new List<TypedStepFrame>();
        foreach (TitleCardDefinition card in TitleCards.All)
        {
            foreach (TitleCardTyping step in TitleCards.Typings(card.Card))
                glyphs.Add(new(SpriteSheetId.Title, TitleSpriteDefinitions.LetterFrameName(step)));
        }
        glyphs.AddRange(typed);
        foreach (TypedStepFrame glyph in glyphs)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(glyph.Sheet);
            if (!sheet.frames.ContainsKey(glyph.Frame) && !sheet.blanks.Contains(glyph.Frame))
                findings.Add($"{glyph.Sheet} holds no glyph {glyph.Frame}");
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} typed text steps are pictured whole:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Typed text: no typed step of the title's cards or the ending's {typed.Count} letters is pictured whole, and each of their {glyphs.Count} letters is held.");
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
