using SuperMetroid.AssetExtraction.Audio;
namespace SuperMetroid.Verification.Harness;

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
internal sealed class EvidencedSoundAcknowledgements : ISoundAcknowledgementTiming<SoundEffectId>
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
    /// <summary>Last captured APU_CurrentSoundLib values, decoded to the sounds each waiting state compares with port echoes.</summary>
    private readonly SoundEffectId?[] currents = new SoundEffectId?[LibraryCount];
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
    public SoundAcknowledgements<SoundEffectId> ForNextUpdate(SoundAcknowledgements<SoundEffectId> modeled, SoundAcknowledgements<SoundEffectId> portWrites) =>
        captured && expectedAfter is not null ? ForUpdate(modeled, expectedAfter) : modeled;

    /// <summary>
    /// Acknowledgements for the update whose native result is <paramref name="nativeAfter"/>.
    /// A waiting library that left its state saw its echo; one that stayed did not.
    /// </summary>
    public SoundAcknowledgements<SoundEffectId> ForUpdate(SoundAcknowledgements<SoundEffectId> spc, NativeCheckpointMemory nativeAfter)
    {
        SoundAcknowledgement<SoundEffectId>[] ports = [spc[0], spc[1], spc[2]];
        for (int library = 0; library < LibraryCount; library++)
        {
            byte before = states[library];
            // Other handler states send or clear requests without awaiting an echo.
            if (!Enum.IsDefined((WaitingState)before))
                continue;
            SoundEffectId? compared = currents[library];
            bool echoed = nativeAfter[SoundStateLib1 + library] != before;
            if (echoed)
                ports[library] = SoundAcknowledgement<SoundEffectId>.Of(compared);
            else if (ports[library].Acknowledges(compared))
                // Any other acknowledgement reproduces the unmatched read; keep the SPC model's
                // acknowledgement unless it already matches.
                ports[library] = Unmatched(library, compared);
        }
        return new SoundAcknowledgements<SoundEffectId>(ports[0], ports[1], ports[2]);
    }

    /// <summary>
    /// An acknowledgement that does not match <paramref name="compared"/>: nothing while a sound is awaited, or the
    /// library's silence while the withdrawal is awaited.
    /// </summary>
    private static SoundAcknowledgement<SoundEffectId> Unmatched(int library, SoundEffectId? compared) =>
        compared is not null
            ? SoundAcknowledgement<SoundEffectId>.None
            : SoundAcknowledgement<SoundEffectId>.Of(library switch
            {
                0 => Library1Sound.Silence,
                1 => Library2Sound.Silence,
                2 => Library3Sound.Silence,
                _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Sound libraries are indexed 0 through 2."),
            });
    }

    /// <summary>Records the native sound-handler state after an update.</summary>
    public void Capture(NativeCheckpointMemory nativeMemory)
    {
        for (int library = 0; library < LibraryCount; library++)
        {
            states[library] = nativeMemory[SoundStateLib1 + library];
            currents[library] = SoundEffectCommands.DecodeOptional(
                SoundEffectLibraries.FromQueueIndex(library), nativeMemory[CurrentSoundLib1 + library]);
        }
        captured = true;
    }
}
