using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomEffectDefinition"/> members production does not use.</summary>
internal static class RoomEffectDefinitionAccess
{
    /// <summary>
    /// Encodes the typed room-FX record back to its 16 native bytes through the record codec (#1415), its effect lists
    /// through its header's area's lists (#1526).
    /// </summary>
    extension(RoomEffectDefinition self)
    {
        /// <summary>
        /// The compiled record's native door word, which its header's selection stores (#1546): zero for a default or a Mother
        /// Brain record, else its entry door's pointer.
        /// </summary>
        internal ushort DoorPointer => RoomFxDefinitionsAccess.DoorPointerOf(self);

        /// <summary>
        /// Exposes one native byte for consumers that use offset-based FX fields. The two blend bytes, the liquid options
        /// byte and the block-animation byte are not exposed: the record holds the visor animation and the effect blend the
        /// import read from the blend bytes, which several bytes share (#1545), the liquid it read from the options byte,
        /// which drops the bits its liquid does not read (#1695), and the block animations it read from the block-animation
        /// byte, which drops the bits of still objects (#2054).
        /// </summary>
        /// <exception cref="NotSupportedException"><paramref name="offset"/> is a blend byte, the liquid options byte or the block-animation byte.</exception>
        internal byte ReadByte(int offset) => offset switch
        {
            0 => (byte)self.DoorPointer,
            1 => LittleEndianBytes.HighByte(self.DoorPointer),
            2 => (byte)RoomFxRecordEncoding.SurfaceWord(self.LiquidStartLevel),
            3 => LittleEndianBytes.HighByte(RoomFxRecordEncoding.SurfaceWord(self.LiquidStartLevel)),
            4 => (byte)RoomFxRecordEncoding.SurfaceWord(self.LiquidTargetLevel),
            5 => LittleEndianBytes.HighByte(RoomFxRecordEncoding.SurfaceWord(self.LiquidTargetLevel)),
            6 => (byte)self.LiquidLevelSpeed.RawValue,
            7 => LittleEndianBytes.HighByte(self.LiquidLevelSpeed.RawValue),
            8 => checked((byte)self.LiquidMoveDelay),
            9 => RoomFxRecordEncoding.TypeByte(self.Type),
            10 or 11 =>
                throw new NotSupportedException($"The record holds the blend the import read from byte {offset}, not the byte."),
            12 => throw new NotSupportedException("The record holds the liquid the import read from byte 12, not the byte."),
            13 => RoomFxRecordEncoding.PaletteEffectsByte(self.ColourEffects, self.HeaderArea),
            14 => throw new NotSupportedException("The record holds the block animations the import read from byte 14, not the byte."),
            15 => RoomFxRecordEncoding.PaletteBlendByte(self.Tint),
            _ => throw new ArgumentOutOfRangeException(nameof(offset)),
        };

        /// <summary>Combines two adjacent native FX-record bytes in little-endian order.</summary>
        internal ushort ReadWord(int offset)
        {
            if ((uint)offset > RoomFxToolingRomData.Record.ByteCount - sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(offset));
            return LittleEndianBytes.Word(self.ReadByte(offset), self.ReadByte(offset + 1));
        }
    }
}
