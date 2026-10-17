using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Checks the opening cinematic's frame-counter word: the cinematic counts its updates as an <see cref="int"/>, and the
/// word it hands the shared Samus systems must wrap to zero every 65536 updates, as the cartridge's sixteen-bit counter
/// does.
/// </summary>
internal static class IntroNmiFrameWordVerification
{
    /// <summary>Checks the word at the start, across the sixteen-bit wrap, and across the counter's own overflow.</summary>
    internal static void VerifyIntroNmiFrameWord()
    {
        AssertEqual(0, IntroCinematicState.NmiFrameWord(0), "the word starts at zero");
        AssertEqual(1, IntroCinematicState.NmiFrameWord(1), "the first update's word is one");
        AssertEqual(ushort.MaxValue, IntroCinematicState.NmiFrameWord(ushort.MaxValue), "the word's last value before it wraps");
        AssertEqual(0, IntroCinematicState.NmiFrameWord(ushort.MaxValue + 1), "the word wraps to zero after 65536 updates");
        AssertEqual(1, IntroCinematicState.NmiFrameWord(ushort.MaxValue + 2), "the word counts on after its wrap");
        AssertEqual(ushort.MaxValue, IntroCinematicState.NmiFrameWord(int.MaxValue), "the word at the update counter's largest value");
        AssertEqual(0, IntroCinematicState.NmiFrameWord(unchecked(int.MaxValue + 1)),
            "the word stays continuous when the update counter itself wraps");
        Console.WriteLine("  Intro frame-counter word: wraps every 65536 updates, including across the update counter's overflow.");
    }

    /// <summary>Runs the opening cinematic frame-counter word check.</summary>
    [VerificationCommand("--intro-nmi-frame-word")]
    internal static int IntroNmiFrameWordCommand()
    {
        Suite(nameof(VerifyIntroNmiFrameWord), () => VerifyIntroNmiFrameWord());
        return 0;
    }
}
