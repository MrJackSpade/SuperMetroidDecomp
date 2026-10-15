namespace SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="ManagedSnesDsp"/> members production does not use.</summary>
internal static class ManagedSnesDspAccess
{
    /// <summary>Exposes the private DSP register array to the register-boundary oracle.</summary>
    extension(ManagedSnesDsp self)
    {
        /// <summary>Reads a mirrored S-DSP register byte using its seven-bit hardware index.</summary>
        [DspRegisterAccess]
        internal byte ReadRegister(byte address) => self.registers[address & 0x7f];
    }
}
