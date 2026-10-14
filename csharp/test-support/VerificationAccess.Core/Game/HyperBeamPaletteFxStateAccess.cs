using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="HyperBeamPaletteFxState"/> members production does not use.</summary>
internal static class HyperBeamPaletteFxStateAccess
{
    /// <summary>Reads the active Hyper Beam palette-FX instruction cursor for timing diagnostics.</summary>
    extension(HyperBeamPaletteFxState self)
    {
        /// <summary>Current bank-$8D instruction pointer, exposed for exact-step debugging.</summary>
        internal ushort InstructionPointer => self._instructionPointer;
    }
}
