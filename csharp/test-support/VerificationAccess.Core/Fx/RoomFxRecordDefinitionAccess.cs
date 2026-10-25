using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxRecordDefinition"/> members production does not use.</summary>
internal static class RoomFxRecordDefinitionAccess
{
    /// <summary>Encodes the typed room-FX record back to its 16 native bytes through the record codec (#1415).</summary>
    extension(RoomFxRecordDefinition self)
    {
        /// <summary>The record's native door word: FFFF for a terminator, zero for a default record, else the door's pointer.</summary>
        internal ushort DoorPointer => self.Terminates ? ushort.MaxValue : self.Door is { } door ? DoorHeaderSourcesConstants.PointerOf(door) : (ushort)0;

        /// <summary>Exposes one native byte for consumers that use offset-based FX fields.</summary>
        internal byte ReadByte(int offset) => offset switch
        {
            0 => (byte)self.DoorPointer,
            1 => SnesLittleEndian.HighByte(self.DoorPointer),
            2 => (byte)self.BaseYPosition,
            3 => SnesLittleEndian.HighByte(self.BaseYPosition),
            4 => (byte)self.TargetYPosition,
            5 => SnesLittleEndian.HighByte(self.TargetYPosition),
            6 => (byte)self.PackedYVelocity,
            7 => (byte)(SnesEightEight.WholeOf(self.PackedYVelocity)),
            8 => self.Timer,
            9 => RoomFxRecordCodec.TypeByte(self.Type),
            10 => RoomFxRecordCodec.LayerBlendByte(self.DefaultLayerBlend),
            11 => RoomFxRecordCodec.LayerBlendByte(self.Layer3LayerBlend),
            12 => RoomFxRecordCodec.LiquidOptionsByte(self.LiquidOptions),
            13 => RoomFxRecordCodec.SlotsByte(self.PaletteFxSlots),
            14 => RoomFxRecordCodec.SlotsByte(self.AnimatedTileSlots),
            15 => RoomFxRecordCodec.PaletteBlendByte(self.PaletteBlend),
            _ => throw new ArgumentOutOfRangeException(nameof(offset)),
        };

        /// <summary>Combines two adjacent native FX-record bytes in little-endian order.</summary>
        internal ushort ReadWord(int offset)
        {
            if ((uint)offset > RoomFxToolingRomData.Record.ByteCount - sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(offset));
            return SnesLittleEndian.Word(self.ReadByte(offset), self.ReadByte(offset + 1));
        }
    }
}
