using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxRecordDefinition"/> members production does not use.</summary>
internal static class RoomFxRecordDefinitionAccess
{
    /// <summary>
    /// Encodes the typed room-FX record back to its 16 native bytes through the record codec (#1415), its effect lists
    /// through its header's area's lists (#1526).
    /// </summary>
    extension(RoomFxRecordDefinition self)
    {
        /// <summary>
        /// The compiled record's native door word, which its header's selection stores (#1546): zero for a default or a Mother
        /// Brain record, else its entry door's pointer.
        /// </summary>
        internal ushort DoorPointer => RoomFxDefinitionsAccess.DoorPointerOf(self);

        /// <summary>
        /// Exposes one native byte for consumers that use offset-based FX fields. The two blend bytes and the liquid options
        /// byte are not exposed: the record holds the visor animation and the effect blend the import read from the blend
        /// bytes, which several bytes share (#1545), and the liquid it read from the options byte, which drops the bits its
        /// liquid does not read (#1695).
        /// </summary>
        /// <exception cref="NotSupportedException"><paramref name="offset"/> is a blend byte or the liquid options byte.</exception>
        internal byte ReadByte(int offset) => offset switch
        {
            0 => (byte)self.DoorPointer,
            1 => LittleEndianBytes.HighByte(self.DoorPointer),
            2 => (byte)RoomFxRecordEncoding.SurfaceWord(self.BaseYPosition),
            3 => LittleEndianBytes.HighByte(RoomFxRecordEncoding.SurfaceWord(self.BaseYPosition)),
            4 => (byte)RoomFxRecordEncoding.SurfaceWord(self.TargetYPosition),
            5 => LittleEndianBytes.HighByte(RoomFxRecordEncoding.SurfaceWord(self.TargetYPosition)),
            6 => (byte)self.YVelocity.RawValue,
            7 => LittleEndianBytes.HighByte(self.YVelocity.RawValue),
            8 => checked((byte)self.Timer),
            9 => RoomFxRecordEncoding.TypeByte(self.Type),
            10 or 11 =>
                throw new NotSupportedException($"The record holds the blend the import read from byte {offset}, not the byte."),
            12 => throw new NotSupportedException("The record holds the liquid the import read from byte 12, not the byte."),
            13 => RoomFxRecordEncoding.PaletteEffectsByte(self.PaletteEffects, self.HeaderArea),
            14 => RoomFxRecordEncoding.AnimatedTilesByte(self.AnimatedTiles, self.HeaderArea),
            15 => RoomFxRecordEncoding.PaletteBlendByte(self.PaletteBlend),
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
