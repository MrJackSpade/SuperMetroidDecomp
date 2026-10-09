using SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="ManagedSnesDsp"/> members production does not use.</summary>
internal static class ManagedSnesDspAccess
{
    /// <summary>Defines verification-only helpers that inspect the DSP's mirrored hardware register file.</summary>
    extension(ManagedSnesDsp self)
    {
        /// <summary>Reads a DSP register through its seven-bit mirrored register address.</summary>
        /// <param name="address">DSP register address; upper bits are ignored as in the hardware register map.</param>
        /// <returns>The current byte stored in the selected DSP register.</returns>
        internal byte ReadRegister(byte address) => PrivateState.Field<byte[]>(self, "registers")[address & 0x7f];
    }
}

/// <summary>Verification access to <see cref="ManagedSpcPlayer"/> members production does not use.</summary>
internal static class ManagedSpcPlayerAccess
{
    /// <summary>Defines deterministic verification access to the player's APU RAM and mirrored DSP registers.</summary>
    extension(ManagedSpcPlayer self)
    {
        /// <summary>Exposes the mirrored DSP register file to deterministic verification.</summary>
        internal byte ReadDspRegisterForVerification(byte address) => PrivateState.Field<ManagedSnesDsp>(self, "dsp").ReadRegister(address);

        /// <summary>Exposes one APU byte only to deterministic friend-assembly verification.</summary>
        internal byte ReadApuByteForVerification(int address)
        {
            if ((uint)address >= PrivateState.Field<byte[]>(self, "ram").Length)
                throw new ArgumentOutOfRangeException(nameof(address), address, "APU address must be 0..65535.");
            return PrivateState.Field<byte[]>(self, "ram")[address];
        }
    }
}

/// <summary>Verification access to <see cref="MusicCommandDelay"/> members production does not use.</summary>
internal static class MusicCommandDelayAccess
{
    /// <summary>Defines verification helpers for restoring and checking effective music-command delays.</summary>
    extension(MusicCommandDelay)
    {
        /// <summary>
        /// Rehydrates an already-effective delay published by a translated subsystem.
        /// </summary>
        internal static MusicCommandDelay FromEffectiveFrames(ushort frames)
        {
            if (frames < AudioRomData.Queues.MinimumMusicDelayFrames)
            {
                throw new InvalidDataException(
                    $"Effective music delay {frames} is below bank $80's minimum of eight frames.");
            }

            return ((MusicCommandDelay)PrivateState.Construct(typeof(MusicCommandDelay), (ushort)(frames)));
        }
    }
}

/// <summary>Verification access to <see cref="SoundEffectLibraries"/> members production does not use.</summary>
internal static class SoundEffectLibrariesAccess
{
    /// <summary>Defines the verification conversion from raw cartridge sound-library numbers to validated identifiers.</summary>
    extension(SoundEffectLibraries)
    {
        /// <summary>Validates a raw cartridge library number before it enters typed code.</summary>
        internal static SoundEffectLibrary FromCartridge(byte value, string source) => value switch
        {
            1 => SoundEffectLibrary.Library1,
            2 => SoundEffectLibrary.Library2,
            3 => SoundEffectLibrary.Library3,
            _ => throw new InvalidDataException(
                $"{source} contains sound-effect library {value}, expected one through three."),
        };
    }
}
