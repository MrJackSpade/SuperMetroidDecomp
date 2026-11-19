using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;

namespace SuperMetroid.Verification.Hud;

/// <summary>
/// Checks the message box's pauses (#2055): which updates gameplay stays paused for before a box's window opens and after
/// it closes, the music and sound passes of each and the one that advances the liquid wave and the statues' descent, for
/// an item's box and for the accepted gunship save with its saving wait and its completion notice's shorter pause.
/// </summary>
internal static class MessageBoxPausesVerification
{
    /// <summary>The pause before an ordinary box opens, after the gameplay update that opened it.</summary>
    private static readonly MessageBoxFrameAudio[] BeforeOpening =
    [
        default,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
    ];

    /// <summary>The pause after a box's window has closed; gameplay resumes on its last update.</summary>
    private static readonly MessageBoxFrameAudio[] AfterClosing =
    [
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        default,
        MessageBoxFrameAudio.MusicAndSounds with { AdvancesLiquidWaveAndStatueDescent = true },
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds with { SoundQueuePasses = 2 },
    ];

    /// <summary>Checks an item box's pauses, then the accepted gunship save's through its completion notice.</summary>
    internal static void VerifyMessageBoxPauses()
    {
        AssertEqual(BeforeOpening.Length + 1, MessageBoxPausesConstants.UpdatesBeforeOpening,
            "the pause before opening counts the gameplay update that opened the box");
        GameplayMessageBoxState box = CreateGameplayMessageFixture();

        box.Begin(GameplayMessageId.EnergyTank, ControllerButton.None, ControllerBindings.Default);
        AssertPause(box, GameplayMessageBoxPhase.PausedBeforeOpening, BeforeOpening, GameplayMessageBoxPhase.Opening, "item box before opening");
        StepWhile(box, () => box.Phase != GameplayMessageBoxPhase.AwaitingInput, ControllerButton.None, 400, "item box opening and display");
        box.Step(ControllerButton.A);
        StepWhile(box, () => box.Phase == GameplayMessageBoxPhase.Closing, ControllerButton.None, 13, "item box closing");
        AssertPause(box, GameplayMessageBoxPhase.PausedAfterClosing, AfterClosing, GameplayMessageBoxPhase.Inactive, "item box after closing");

        box.Begin(GameplayMessageId.GunshipSaveConfirmation, ControllerButton.None, ControllerBindings.Default);
        AssertPause(box, GameplayMessageBoxPhase.PausedBeforeOpening, BeforeOpening, GameplayMessageBoxPhase.Opening, "gunship save before opening");
        StepWhile(box, () => box.Phase != GameplayMessageBoxPhase.AwaitingInput, ControllerButton.None, 20, "gunship save opening");
        StepWhile(box, () => box.Phase == GameplayMessageBoxPhase.AwaitingInput, ControllerButton.A, 4, "gunship save accepting YES");
        StepWhile(box, () => box.Phase == GameplayMessageBoxPhase.Closing, ControllerButton.None, 13, "gunship save closing");

        // One paused update after the closed box, then the 160-update saving wait; the notice begins on the last.
        int savingWait = MessageBoxPauses.UpdatesBeforeGunshipSavingWait + GameplayMessageBoxDefinitions.Timing.GunshipSavingSoundFrames;
        for (int update = 1; update <= savingWait; update++)
        {
            AssertEqual(GameplayMessageBoxPhase.GunshipSavingSound, box.Phase, $"gunship saving wait update {update}");
            box.Step(ControllerButton.None);
            AssertEqual(MessageBoxFrameAudio.MusicAndSounds, box.LastFrameAudio, $"gunship saving wait update {update} music and sound");
        }
        AssertEqual(GameplayMessageId.SaveCompleted, box.MessageId, "the completion notice begins as the saving wait ends");
        AssertPause(box, GameplayMessageBoxPhase.PausedBeforeOpening, [.. Enumerable.Repeat(MessageBoxFrameAudio.MusicAndSounds, 5)],
            GameplayMessageBoxPhase.Opening, "completion notice before opening");
        StepWhile(box, () => box.Phase != GameplayMessageBoxPhase.AwaitingInput, ControllerButton.None, 40, "completion notice opening and display");
        box.Step(ControllerButton.A);
        StepWhile(box, () => box.Phase == GameplayMessageBoxPhase.Closing, ControllerButton.None, 13, "completion notice closing");
        MessageBoxStep closing = AssertPause(box, GameplayMessageBoxPhase.PausedAfterClosing, AfterClosing, GameplayMessageBoxPhase.Inactive,
            "completion notice after closing");
        AssertEqual<MessageBoxStep>(new MessageBoxStep.Answered(SaveChoice.Yes), closing,
            "the accepted gunship save hands over YES as its notice closes");
        Console.WriteLine("Message box pauses: eight updates before an item box opens, five before the gunship's completion notice, one before its saving wait and six after a box closes, each with its music, sound and liquid advance.");
    }

    /// <summary>
    /// Requires <paramref name="box"/> to be in <paramref name="paused"/>, steps it once per entry of
    /// <paramref name="updates"/> requiring each update's music, sound and liquid advance and the box still paused until
    /// the last, after which it is in <paramref name="after"/>.
    /// </summary>
    /// <returns>What the pause's last update did.</returns>
    private static MessageBoxStep AssertPause(GameplayMessageBoxState box, GameplayMessageBoxPhase paused, MessageBoxFrameAudio[] updates,
        GameplayMessageBoxPhase after, string context)
    {
        MessageBoxStep? last = null;
        for (int update = 0; update < updates.Length; update++)
        {
            AssertEqual(paused, box.Phase, $"{context}: paused before update {update}");
            last = box.Step(ControllerButton.None);
            AssertEqual(updates[update], box.LastFrameAudio, $"{context}: update {update} music, sound and liquid advance");
        }
        AssertEqual(after, box.Phase, $"{context}: the pause ends after {updates.Length} updates");
        return last ?? throw new InvalidOperationException($"{context}: a pause has at least one update.");
    }

    /// <summary>Steps <paramref name="box"/> with <paramref name="input"/> held while <paramref name="condition"/> holds, at most <paramref name="limit"/> updates.</summary>
    private static void StepWhile(GameplayMessageBoxState box, Func<bool> condition, ControllerButton input, int limit, string context)
    {
        int updates = 0;
        while (condition())
        {
            AssertTrue(++updates <= limit, $"{context} ends within {limit} updates");
            box.Step(input);
        }
    }

    /// <summary>Runs the message box pause check.</summary>
    [VerificationCommand("--message-box-pauses")]
    internal static int MessageBoxPausesCommand()
    {
        VerifyMessageBoxPauses();
        return 0;
    }
}
