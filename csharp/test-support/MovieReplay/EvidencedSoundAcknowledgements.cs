using SuperMetroid.AssetExtraction.Audio;
namespace SuperMetroid.TestSupport.MovieReplay;

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
internal sealed class EvidencedSoundAcknowledgements : ISoundAcceptanceTiming<SoundEffectLibrary, SoundEffectId>
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

    /// <summary>Last captured APU_SoundStateLib values used to detect waiting-state exits.</summary>
    private readonly byte[] states = new byte[SoundEffectLibraries.Count];
    /// <summary>Last captured APU_CurrentSoundLib values, decoded to the sounds each waiting state compares with port echoes.</summary>
    private readonly SoundEffectId?[] currents = new SoundEffectId?[SoundEffectLibraries.Count];
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
    public SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> ForNextUpdate(SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> clocked, SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> requested) =>
        captured && expectedAfter is not null ? ForUpdate(clocked, expectedAfter) : clocked;

    /// <summary>
    /// Acknowledgements for the update whose native result is <paramref name="nativeAfter"/>.
    /// A waiting library that left its state saw its echo; one that stayed did not.
    /// </summary>
    public SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> ForUpdate(SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> spc, NativeCheckpointMemory nativeAfter)
    {
        return new SoundAcknowledgements<SoundEffectLibrary, SoundEffectId>(SoundEffectLibraries.InQueueOrder,
            library => ForLibrary(library, spc[library], nativeAfter));
    }

    /// <summary>
    /// <paramref name="library"/>'s acknowledgement for the update whose native result is <paramref name="nativeAfter"/>,
    /// given the SPC model's <paramref name="modeled"/> one.
    /// </summary>
    private SoundAcknowledgement<SoundEffectId> ForLibrary(
        SoundEffectLibrary library, SoundAcknowledgement<SoundEffectId> modeled, NativeCheckpointMemory nativeAfter)
    {
        // The native handler keeps each library's state at the driver's index of the library.
        int position = SoundLibraryNumbers.DriverIndex(library);
        byte before = states[position];
        // Other handler states send or clear requests without awaiting an echo.
        if (!Enum.IsDefined((WaitingState)before))
            return modeled;
        SoundEffectId? compared = currents[position];
        bool echoed = nativeAfter[SoundStateLib1 + position] != before;
        if (echoed)
            return SoundAcknowledgement<SoundEffectId>.Of(compared);
        // Any other acknowledgement reproduces the unmatched read; keep the SPC model's
        // acknowledgement unless it already matches.
        return modeled.Acknowledges(compared) ? Unmatched(library, compared) : modeled;
    }

    /// <summary>
    /// An acknowledgement that does not match <paramref name="compared"/>: nothing while a sound is awaited, or the
    /// library's silence while the withdrawal is awaited.
    /// </summary>
    private static SoundAcknowledgement<SoundEffectId> Unmatched(SoundEffectLibrary library, SoundEffectId? compared) =>
        compared is not null
            ? SoundAcknowledgement<SoundEffectId>.Of(null)
            : SoundAcknowledgement<SoundEffectId>.Of(library switch
            {
                SoundEffectLibrary.Samus => SamusSound.Silence,
                SoundEffectLibrary.Enemies => EnemySound.Silence,
                SoundEffectLibrary.Room => RoomSound.Silence,
                _ => throw new ArgumentOutOfRangeException(nameof(library), library, "Unknown sound-effect library."),
            });

    /// <summary>Records the native sound-handler state after an update.</summary>
    public void Capture(NativeCheckpointMemory nativeMemory)
    {
        // The sound-effect libraries occupy adjacent handler-state bytes, in queue order.
        for (int library = 0; library < SoundEffectLibraries.Count; library++)
        {
            states[library] = nativeMemory[SoundStateLib1 + library];
            currents[library] = SoundEffectCommands.DecodeOptional(
                SoundLibraryNumbers.FromDriverIndex(library), nativeMemory[CurrentSoundLib1 + library]);
        }
        captured = true;
    }
}
