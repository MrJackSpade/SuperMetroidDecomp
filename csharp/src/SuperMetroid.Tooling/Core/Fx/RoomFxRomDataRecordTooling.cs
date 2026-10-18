namespace SuperMetroid.Core.Fx;

/// <summary>Development-tool members of <see cref="RoomFxRomData.Record"/>; never linked by player hosts.</summary>
internal static class RoomFxRomDataRecordTooling
{
        /// <summary>Byte offset of the two-byte door selector that begins a native room-FX record.</summary>
        public const int DoorPointerOffset = 0;
        /// <summary>Door word that terminates an FX list without selecting a record.</summary>
        public const ushort TerminatorDoorPointer = ushort.MaxValue;
        /// <summary>Byte offset of the liquid's starting Y position word.</summary>
        public const int BaseYPositionOffset = 2;
        /// <summary>Byte offset of the liquid's destination Y position word.</summary>
        public const int TargetYPositionOffset = 4;
        /// <summary>Byte offset of the packed liquid Y velocity word.</summary>
        public const int YVelocityOffset = 6;
        /// <summary>Byte offset of the liquid movement delay byte after the four leading words.</summary>
        public const int TimerOffset = 8;
        /// <summary>Byte offset of the native room-FX type selector.</summary>
        public const int TypeOffset = 9;
        /// <summary>Byte offset of the default layer blend configuration used outside the layer-3 override.</summary>
        public const int DefaultLayerBlendConfigurationOffset = 10;
        /// <summary>Byte offset of the layer-3-specific blend configuration.</summary>
        public const int Layer3LayerBlendConfigurationOffset = 11;
        /// <summary>Byte offset of the bitfield controlling native liquid behavior.</summary>
        public const int LiquidOptionsOffset = 12;
        /// <summary>Byte offset of the palette-FX selection bitset.</summary>
        public const int PaletteFxBitsetOffset = 13;
        /// <summary>Byte offset of the animated-tile selection bitset.</summary>
        public const int AnimatedTileBitsetOffset = 14;
        /// <summary>Byte offset of the layer palette blend selector at the record's tail.</summary>
        public const int PaletteBlendOffset = 15;
}
