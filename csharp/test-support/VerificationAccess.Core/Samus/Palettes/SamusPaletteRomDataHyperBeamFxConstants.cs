namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.HyperBeamFx"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataHyperBeamFxConstants
{
    /// <summary>Instruction <c>$C595</c>: finish the current timed palette record.</summary>
    public const ushort Done = 0xc595;
    /// <summary>Bytes occupied by a duration, eight colors, and the done opcode.</summary>
    public const int FrameByteCount = 20;
    /// <summary>Number of timed color records in the loop.</summary>
    public const int FrameCount = 10;
    /// <summary>Instruction <c>$C61E</c>: jump to the instruction pointer in Y.</summary>
    public const ushort Goto = 0xc61e;
    /// <summary>Initial instruction list stored by the object definition.</summary>
    public const ushort InitialList = 0xd900;
    /// <summary>Instruction <c>$C655</c>: select palette-buffer byte index from Y.</summary>
    public const ushort SetColorIndex = 0xc655;

    /// <summary>Provides Hyper Beam palette FX commands, list address, frame count, and frame width.</summary>
    extension(SamusPaletteRomData.HyperBeamFx)
    {
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.Done"/>
        internal static ushort Done => SamusPaletteRomDataHyperBeamFxConstants.Done;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.FrameByteCount"/>
        internal static int FrameByteCount => SamusPaletteRomDataHyperBeamFxConstants.FrameByteCount;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.FrameCount"/>
        internal static int FrameCount => SamusPaletteRomDataHyperBeamFxConstants.FrameCount;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.Goto"/>
        internal static ushort Goto => SamusPaletteRomDataHyperBeamFxConstants.Goto;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.InitialList"/>
        internal static ushort InitialList => SamusPaletteRomDataHyperBeamFxConstants.InitialList;
        /// <inheritdoc cref="SamusPaletteRomDataHyperBeamFxConstants.SetColorIndex"/>
        internal static ushort SetColorIndex => SamusPaletteRomDataHyperBeamFxConstants.SetColorIndex;
    }
}
