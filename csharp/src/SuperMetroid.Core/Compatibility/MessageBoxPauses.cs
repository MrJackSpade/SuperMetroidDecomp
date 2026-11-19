using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The updates gameplay stays paused around a message box while nothing of the box shows (#2055): before its window opens
/// and after it has closed, and what each of those updates runs. Gameplay stays paused for eight updates before an
/// ordinary box opens, five before the gunship's completion notice opens and six after a box closes, and one update
/// separates the accepted gunship save's closed box from its saving sound's wait. On each paused update the music queue
/// and the sound queues run a set number of times, and one update after the box closes also advances the room liquid's
/// wave, the Tourian statues' descent and a power bomb's explosion. The count is gameplay-observable: it decides on which
/// update gameplay resumes and when queued sounds are accepted, and the liquid's advance swaps the halves of the random
/// number (a lava or acid wave) and moves the statues' descent, which every later random draw and the statues' rumble
/// follow.
/// </summary>
/// <remarks>
/// <para>
/// Started by the message box when it begins (<see cref="BeginBeforeOpening"/>), when the gunship's completion notice
/// begins after the saving sound (<see cref="BeginBeforeCompletionNotice"/>) and when its window has closed
/// (<see cref="BeginAfterClosing"/>); advanced once per update of the box while it is paused, before the update's audio
/// runs. Created once per session at its composition root (<c>FrontendSession</c>) and handed to every gameplay runtime's
/// message box.
/// </para>
/// <para>
/// The pause before an ordinary box opens: its first update is the gameplay update that opened the box, whose own sound
/// pass runs only once the box has closed; the second runs no music or sound; the other six run music and sound once
/// each. The pause before the gunship's completion notice: five updates of music and sound. The pause after the window
/// has closed: two updates of music and sound, one with neither, one of music and sound that also advances the liquid
/// wave, the statues' descent and a power bomb's explosion, then two of music and sound, gameplay resuming on the last,
/// whose own sound pass follows the box's.
/// </para>
/// <para>
/// Evidence: the original message-box routine (<c>$85:8080</c>) prepares the screen over two displayed frames, the
/// first silent, clears the box over one, writes the message over one, sets up the box and its window over two and
/// plays two frames of music and sound (<c>$85:808D</c>-<c>$85:8096</c>) before the window opens; the gunship's
/// completion notice skips the screen's preparation and the clearing. After the window closes it clears the box, restores
/// the screen over two frames, the first silent and the second running the room's liquid and statue effects
/// (<c>$88:84B9</c>), and plays two frames of music and sound (<c>$85:80AA</c>-<c>$85:80B4</c>) before it returns
/// (<c>$85:80BA</c>) into the gameplay update that opened it. The SMV replays compare the box's first and last frames
/// with the native capture's message-box entry and return and the random number on every update.
/// </para>
/// </remarks>
[CompatibilityService("gameplay stays paused eight updates before a message box opens, five before the gunship's completion notice and six after a box closes, each running the music and sound queues a set number of times, one of them after closing also advancing the liquid wave, the statues' descent and a power bomb's explosion")]
public sealed class MessageBoxPauses
{

    /// <summary>The updates between the accepted gunship save's closed box and the start of its saving sound's wait.</summary>
    public const int UpdatesBeforeGunshipSavingWait = 1;

    /// <summary>
    /// The pause before an ordinary box opens, after the gameplay update that opened it: one update with no music or
    /// sound, then six of music and sound.
    /// </summary>
    private static readonly ImmutableArray<MessageBoxFrameAudio> BeforeOpening =
    [
        default,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
    ];

    /// <summary>The pause before the gunship's completion notice opens: five updates of music and sound.</summary>
    private static readonly ImmutableArray<MessageBoxFrameAudio> BeforeCompletionNotice =
    [
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
    ];

    /// <summary>
    /// The pause after the window has closed: two updates of music and sound, one with neither, one of music and sound
    /// that also advances the liquid wave and the statues' descent, one of music and sound, and the update returning to
    /// gameplay, whose own sound pass follows.
    /// </summary>
    private static readonly ImmutableArray<MessageBoxFrameAudio> AfterClosing =
    [
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds,
        default,
        MessageBoxFrameAudio.MusicAndSounds with { AdvancesLiquidWaveAndStatueDescent = true },
        MessageBoxFrameAudio.MusicAndSounds,
        MessageBoxFrameAudio.MusicAndSounds with { SoundQueuePasses = 2 },
    ];

    /// <summary>The pause running now, one entry per update.</summary>
    private ImmutableArray<MessageBoxFrameAudio> _pause = [];

    /// <summary>The run of <see cref="_pause"/>, one update per entry: its age is the updates already run.</summary>
    private UpdateCountdown _pauseRun = UpdateCountdown.Ended();

    /// <summary>Whether every update of the current pause has run: the box opens, or gameplay resumes.</summary>
    public bool IsOver => _pauseRun.IsOver;

    /// <summary>Starts the pause before an ordinary box opens; the gameplay update that opened the box was its first.</summary>
    public void BeginBeforeOpening() => Begin(BeforeOpening);

    /// <summary>Starts the pause before the gunship's completion notice opens, after its saving sound's wait.</summary>
    public void BeginBeforeCompletionNotice() => Begin(BeforeCompletionNotice);

    /// <summary>Starts the pause after a box's window has closed.</summary>
    public void BeginAfterClosing() => Begin(AfterClosing);

    /// <summary>Runs the pause's next update and returns its music and sound passes and whether it advances the liquid wave and statue descent.</summary>
    /// <exception cref="InvalidOperationException">Every update of the pause has already run.</exception>
    public MessageBoxFrameAudio Advance()
    {
        if (IsOver)
            throw new InvalidOperationException("The message box's pause has already ended.");
        MessageBoxFrameAudio update = _pause[_pauseRun.Age];
        _pauseRun.Count();
        return update;
    }

    /// <summary>Starts <paramref name="pause"/> from its first update.</summary>
    private void Begin(ImmutableArray<MessageBoxFrameAudio> pause)
    {
        _pause = pause;
        _pauseRun = UpdateCountdown.For(pause.Length);
    }
}
