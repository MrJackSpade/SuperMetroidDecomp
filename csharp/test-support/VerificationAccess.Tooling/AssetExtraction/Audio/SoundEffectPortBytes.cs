namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Encodes acknowledgements as the original sound program's port bytes, which recordings of its ports hold.</summary>
internal static class SoundEffectPortBytes
{
    /// <summary>The byte library one's port held while the original sound program loaded new music.</summary>
    internal const byte MusicLoadPortByte = 187;

    /// <summary>
    /// The byte the original sound program's port held for <paramref name="acknowledgement"/>: the acknowledged sound's
    /// request number, zero for nothing, or <see cref="MusicLoadPortByte"/> while music loads. Recordings of the
    /// original's ports are compared through it.
    /// </summary>
    internal static byte PortByte(SoundAcknowledgement<SoundEffectId> acknowledgement) =>
        acknowledgement.IsMusicLoading ? MusicLoadPortByte
            : acknowledgement.Sound is SoundEffectId sound ? checked((byte)SoundEffectCommands.Command(sound)) : (byte)0;
}
