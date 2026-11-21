namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Encodes acknowledgements as the original sound program's port bytes, which recordings of its ports hold.</summary>
internal static class SoundEffectPortBytes
{
    /// <summary>The byte library one's port held while the original sound program loaded new music.</summary>
    internal const byte MusicLoadPortByte = 187;

    /// <summary>
    /// The byte the original sound program's port for <paramref name="library"/> held for <paramref name="acknowledgement"/>:
    /// the number of the library's request it acknowledges, zero for nothing, or <see cref="MusicLoadPortByte"/> while
    /// music loads. Recordings of the original's ports are compared through it.
    /// </summary>
    /// <exception cref="InvalidDataException">The acknowledgement acknowledges none of the library's requests.</exception>
    internal static byte PortByte(SoundEffectLibrary library, SoundAcknowledgement<SoundEffectId> acknowledgement)
    {
        if (acknowledgement.Equals(SoundAcknowledgement<SoundEffectId>.MusicLoading))
            return MusicLoadPortByte;
        if (acknowledgement.IsNone)
            return 0;
        for (int command = 1; command <= byte.MaxValue; command++)
        {
            if (SoundEffectCommands.Names(library, command)
                && acknowledgement.Acknowledges(new RequestedSound<SoundEffectId>(SoundEffectCommands.Decode(library, command))))
                return (byte)command;
        }
        throw new InvalidDataException($"{library} acknowledges {acknowledgement}, none of its requests.");
    }
}
