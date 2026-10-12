using SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="ManagedSnesDsp"/> members production does not use.</summary>
internal static class ManagedSnesDspAccess
{
    extension(ManagedSnesDsp self)
    {
        [DspRegisterAccess]
        internal byte ReadRegister(byte address) => PrivateState.Field<byte[]>(self, "registers")[address & 0x7f];
    }
}

/// <summary>Verification views of FLG's two meanings: the control bits and the noise rate (#631 alias proof).</summary>
internal static class DspFlagsAccess
{
    extension(DspFlags self)
    {
        /// <summary>The reset (bit 7), mute (bit 6) and echo-write-disable (bit 5) bits.</summary>
        [DspRegisterBoundary("exact register test: FLG control-bit view")]
        internal byte Control => unchecked((byte)(self.Value & ~SnesDspRegisterMap.Fields.NoiseRateMask));

        /// <summary>The noise clock rate index (bits 0-4).</summary>
        [DspRegisterBoundary("exact register test: FLG noise-rate view")]
        internal int NoiseRate => self.Value & SnesDspRegisterMap.Fields.NoiseRateMask;
    }
}

/// <summary>Verification access to <see cref="ManagedSpcPlayer"/> members production does not use.</summary>
internal static class ManagedSpcPlayerAccess
{
    extension(ManagedSpcPlayer self)
    {
        /// <summary>Exposes the mirrored DSP register file to deterministic verification.</summary>
        [DspRegisterBoundary("exact register test: checks S-DSP register behaviour")]
        [DspRegisterAccess]
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
