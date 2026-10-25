using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="ManagedSpcPlayer"/> members production does not use.</summary>
internal static class ManagedSpcPlayerAccess
{
    /// <summary>Exposes deterministic DSP and APU memory views for friend-assembly checks.</summary>
    extension(ManagedSpcPlayer self)
    {
        /// <summary>Exposes the mirrored DSP register file to deterministic verification.</summary>
        internal byte ReadDspRegisterForVerification(byte address) => self.dsp.ReadRegister(address);

        /// <summary>Exposes one APU byte only to deterministic friend-assembly verification.</summary>
        internal byte ReadApuByteForVerification(int address)
        {
            if ((uint)address >= self.ram.Length)
                throw new ArgumentOutOfRangeException(nameof(address), address, "APU address must be 0..65535.");
            return self.ram[address];
        }
    }
}
