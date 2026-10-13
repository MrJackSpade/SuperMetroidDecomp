using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

/// <summary>
/// Supplies the sound-effect ports' SPC acknowledgements from the native capture.
/// </summary>
/// <remarks>
/// <c>HandleSounds</c> ($82:89EF) waits in state 1 for the SPC to echo the sent sound and in
/// state 3 for it to echo the cleared request. When that echo arrives depends on where the
/// CPU's port write falls against the SPC driver's service period, i.e. on the CPU's
/// per-frame load, which the port removes. The capture shows whether each library left its
/// waiting state during an update; the replay reproduces exactly that outcome. Music
/// (port 0), updates without native evidence, and normal play keep the session's SPC model.
/// </remarks>
internal sealed class EvidencedSoundAcknowledgements : ISoundAcknowledgementSource
{
    /// <summary>$0649..$064B: APU_SoundStateLib1..3.</summary>
    private const int SoundStateLib1 = 0x0649;

    /// <summary>$064D..$064F: APU_CurrentSoundLib1..3, the byte each waiting state compares.</summary>
    private const int CurrentSoundLib1 = 0x064d;

    /// <summary>The <c>HandleSounds</c> library states that wait for an SPC echo.</summary>
    private enum WaitingState : byte
    {
        /// <summary>State 1 waits for the SPC to echo the sent sound.</summary>
        RequestAcknowledgement = 1,
        /// <summary>State 3 waits for the SPC to echo the cleared request.</summary>
        ClearAcknowledgement = 3,
    }

    /// <summary>The three sound-effect libraries occupy adjacent handler-state bytes.</summary>
    private const int LibraryCount = 3;

    /// <summary>Last captured APU_SoundStateLib values used to detect waiting-state exits.</summary>
    private readonly byte[] states = new byte[LibraryCount];
    /// <summary>Last captured APU_CurrentSoundLib values compared with port echoes.</summary>
    private readonly byte[] currents = new byte[LibraryCount];
    /// <summary>False after an update without a native checkpoint, until the next capture.</summary>
    private bool captured;
    /// <summary>Native memory expected after the current update, captured once that update completes.</summary>
    private NativeCheckpointMemory? expectedAfter;

    /// <summary>Seeds sound-handler states from the native memory before replay starts.</summary>
    public EvidencedSoundAcknowledgements(NativeCheckpointMemory nativeMemory) => Capture(nativeMemory);

    /// <summary>
    /// Selects the native result of the next update, or <see langword="null"/> when that update
    /// has no evidence of its own and reads the session's model.
    /// </summary>
    public void Expect(NativeCheckpointMemory? nativeAfter) => expectedAfter = nativeAfter;

    /// <summary>Forgets the captured handler state after an update the capture does not cover.</summary>
    public void Invalidate() => captured = false;

    /// <inheritdoc/>
    public CartridgeAudioAcknowledgements ForNextFrame(CartridgeAudioAcknowledgements modeled) =>
        captured && expectedAfter is not null ? ForUpdate(modeled, expectedAfter) : modeled;

    /// <summary>
    /// Acknowledgements for the update whose native result is <paramref name="nativeAfter"/>.
    /// A waiting library that left its state saw its echo; one that stayed did not.
    /// </summary>
    public CartridgeAudioAcknowledgements ForUpdate(CartridgeAudioAcknowledgements spc, NativeCheckpointMemory nativeAfter)
    {
        byte[] ports = [spc.Port1, spc.Port2, spc.Port3];
        for (int library = 0; library < LibraryCount; library++)
        {
            byte before = states[library];
            // Other handler states send or clear requests without awaiting an echo.
            if (!Enum.IsDefined((WaitingState)before))
                continue;
            byte compared = currents[library];
            bool echoed = nativeAfter[SoundStateLib1 + library] != before;
            if (echoed)
                ports[library] = compared;
            else if (ports[library] == compared)
                // Any other byte reproduces the unmatched read; keep the SPC model's byte
                // unless it already matches.
                ports[library] = unchecked((byte)(compared + 1));
        }
        return new CartridgeAudioAcknowledgements(spc.Port0, ports[0], ports[1], ports[2]);
    }

    /// <summary>Records the native sound-handler state after an update.</summary>
    public void Capture(NativeCheckpointMemory nativeMemory)
    {
        for (int library = 0; library < LibraryCount; library++)
        {
            states[library] = nativeMemory[SoundStateLib1 + library];
            currents[library] = nativeMemory[CurrentSoundLib1 + library];
        }
        captured = true;
    }
}
