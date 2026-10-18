using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxRecordDefinition"/> members production does not use.</summary>
internal static class RoomFxRecordDefinitionAccess
{
    /// <summary>Reads the packed 16-byte room-FX record as native little-endian fields.</summary>
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
            9 => self.Type,
            10 => self.DefaultLayerBlend,
            11 => self.Layer3LayerBlend,
            12 => self.LiquidOptions,
            13 => self.PaletteFxBitset,
            14 => self.AnimatedTileBitset,
            15 => self.PaletteBlend,
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
