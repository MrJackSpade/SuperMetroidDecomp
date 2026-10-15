namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values of <see cref="RoomPlmInstruction"/> that only verification reads.</summary>
internal static class RoomPlmInstructionConstants
{
    /// <summary><c>$84:8764 Instruction_PLM_LoadItemPLMGfx</c>: load an item's graphics set.</summary>
    public const ushort LoadItemGraphics = 0x8764;

    /// <summary>Provides the native load-item-graphics PLM instruction address for program decoding.</summary>
    extension(RoomPlmInstruction)
    {
        /// <inheritdoc cref="RoomPlmInstructionConstants.LoadItemGraphics"/>
        internal static ushort LoadItemGraphics => RoomPlmInstructionConstants.LoadItemGraphics;
    }
}
