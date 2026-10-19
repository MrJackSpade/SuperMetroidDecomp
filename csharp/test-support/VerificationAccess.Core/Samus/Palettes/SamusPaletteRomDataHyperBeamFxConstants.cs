using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Abilities;
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
        /// <summary><c>$8D:E1F0</c>, two-word Hyper Beam palette-FX definition.</summary>
        /// <remarks>
        /// Issue #887 / #625: the pinned NTSC J/U v1.0 ROM stores
        /// <c>$C685,$D900</c> here, selecting the no-op setup and
        /// bank-$8D program. Its entry at <c>$D900</c> is
        /// <c>$C655,$01C2</c>. Ten records start at
        /// <c>$D904+20*i</c>, <c>i=0..9</c>; every duration is two,
        /// every terminator at record offset eighteen is
        /// <c>$C595</c>, and the terminal <c>$D9CC</c> command is
        /// <c>$C61E,$D904</c>. All 26 control words match the pinned
        /// ROM and native bank-$8D listing. The compiled resolver
        /// accepts only entry, aligned records, and terminal loop;
        /// it rejects adjacent or unaligned restored pointers.
        /// The caller loads eight live color words at record offset
        /// two into OBJ palette six, color one, displays each record
        /// for two handler calls, and loops after record nine.
        /// Color payloads are a separate authored table.
        /// </remarks>
        public const int ObjectDefinition = 0x8de1f0;
}
