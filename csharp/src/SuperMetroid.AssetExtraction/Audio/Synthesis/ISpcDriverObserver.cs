using SuperMetroid.Core.Compatibility;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Observes the translated SPC driver's timing points for offline analysis (#1305): which driver
/// tick is running, when the music sequencer reaches its next top-level phrase, and which voices
/// each tick keys on or off. Playback never installs one; it does not influence the driver.
/// </summary>
internal interface ISpcDriverObserver
{
    /// <summary>Called before the driver services <paramref name="tick"/>.</summary>
    void TickStarting(SoundClockStep<byte> tick);

    /// <summary>Called when the music sequencer is about to read its next top-level record (the native NextPhrase).</summary>
    void PhraseStarting();

    /// <summary>Called as the tick publishes its pending key-off and key-on voice masks to the DSP.</summary>
    void KeysPublished(int keyOn, int keyOff);
}
