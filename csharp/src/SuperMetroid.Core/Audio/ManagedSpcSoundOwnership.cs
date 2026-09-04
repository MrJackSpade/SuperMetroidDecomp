namespace SuperMetroid.Core.Audio;

/// <summary>
/// Shared-state handoff between one music voice and the SFX stream temporarily borrowing it.
/// </summary>
internal static class ManagedSpcSoundOwnership
{
    /// <summary>
    /// Recreates the SPC driver's 16-bit store at its adjacent saved-volume and saved-phase
    /// bytes. Representing those bytes as separate C# fields makes both assignments explicit;
    /// omitting the high byte loses left/right phase inversion when the SFX releases the voice.
    /// </summary>
    public static void CaptureBorrowedMusicState(
        ManagedSpcMusicChannel musicChannel,
        ManagedSpcSoundChannel soundChannel)
    {
        ArgumentNullException.ThrowIfNull(musicChannel);
        ArgumentNullException.ThrowIfNull(soundChannel);
        soundChannel.Volume = musicChannel.FinalVolume;
        soundChannel.PhaseInvert = musicChannel.PanFlags;
    }
}
