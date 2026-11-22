using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// The opening's Mother Brain flashback shows Samus in her opening colours (#2616): once the crossfade into the fight has
/// faded the narration out, the narration's level fades only the beam colours it shares with her, not her whole draw, so
/// her suit shows the opening's suit colours instead of black.
/// </summary>
internal static class IntroFlashbackSamusColoursVerification
{
    /// <summary>How far into the fight, past its crossfade, the check looks at her.</summary>
    private const int FightUpdates = 60;

    /// <summary>Checks Samus's draw in the fight: no level over the whole draw, her suit in the opening's colours.</summary>
    internal static void VerifyFlashbackSamusShowsHerOpeningColours()
    {
        var audio = AudioQueues.Standalone();
        IntroCinematicState intro = InstalledFixtures.CreateIntro(audio);
        int fight = 0;
        for (int update = 0; fight < FightUpdates; update++)
        {
            if (update >= 20_000)
                throw new InvalidOperationException("The intro did not reach the Mother Brain flashback.");
            intro.Step(update % 47 == 0 ? ControllerButton.A : ControllerButton.None);
            audio.AdvanceFrame(default);
            if (intro.Phase == IntroCinematicPhase.MotherBrainFlashback)
                fight++;
        }
        ImageSpriteDraw[] samus = [.. intro.ComposeScene().Layers.ToArray().OfType<SpritesLayer>()
            .SelectMany(layer => layer.Draws).Where(draw => draw.Source.Kind == SpriteSourceKind.Samus)];
        AssertEqual(1, samus.Length, "the fight draws Samus once");
        ImageSpriteDraw draw = samus[0];
        AssertTrue(draw.Shading.Effects.IsEmpty, "no colour effect covers Samus's whole draw");
        AssertTrue(draw.Shading.Shades.Contains(IntroShades.Faded(IntroCrossfadeScene.Narration, 0)),
            "the narration's faded-out level shows over the beam colours it shares with her");
        SpriteImage sheet = draw.Image.Sheet;
        FrozenDictionary<ColourKey, RgbColor> drawn = draw.Shading.ColoursOf(sheet, ColourBlendOracle.Instance);
        foreach (KeyedSubstitution suit in sheet.Shade(SamusShades.IntroSuit).Substitutions)
            AssertEqual(suit.Shown, drawn[suit.Replaced], $"Samus's suit colour {suit.Replaced} shows the opening's suit colour");
    }

    /// <summary>Runs the flashback Samus colour check.</summary>
    [VerificationCommand("--intro-flashback-samus-colours")]
    internal static int IntroFlashbackSamusColoursCommand()
    {
        Suite(nameof(VerifyFlashbackSamusShowsHerOpeningColours), VerifyFlashbackSamusShowsHerOpeningColours);
        return 0;
    }
}
