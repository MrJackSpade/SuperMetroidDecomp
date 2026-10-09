namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RoomFxRomData.Record"/>; never linked by player hosts.</summary>
internal static class RoomFxRomDataRecordTooling
{
        /// <summary>Byte offset of the door-pointer word used to select a door-specific FX record or terminate its list.</summary>
        public const int DoorPointerOffset = 0;
        /// <summary>Byte offset of the FX liquid or surface's starting vertical position word.</summary>
        public const int BaseYPositionOffset = 2;
        /// <summary>Byte offset of the destination vertical position word for an animated liquid effect.</summary>
        public const int TargetYPositionOffset = 4;
        /// <summary>Byte offset of the signed fixed-point vertical movement speed word.</summary>
        public const int YVelocityOffset = 6;
        /// <summary>Byte offset of the per-effect timing byte in the native FX record.</summary>
        public const int TimerOffset = 8;
        /// <summary>Byte offset of the discriminator that selects the room FX behavior.</summary>
        public const int TypeOffset = 9;
        /// <summary>Byte offset of the default PPU layer-blending configuration for the effect.</summary>
        public const int DefaultLayerBlendConfigurationOffset = 10;
        /// <summary>Byte offset of the alternate layer-blending configuration used by layer-three effects.</summary>
        public const int Layer3LayerBlendConfigurationOffset = 11;
        /// <summary>Byte offset of the option bits that configure liquid physics and visual behavior.</summary>
        public const int LiquidOptionsOffset = 12;
        /// <summary>Byte offset of the bitset selecting the room's palette-effect programs.</summary>
        public const int PaletteFxBitsetOffset = 13;
        /// <summary>Byte offset of the bitset selecting the room's animated-tile effects.</summary>
        public const int AnimatedTileBitsetOffset = 14;
        /// <summary>Byte offset of the palette-blend selector used to resolve the effect's color blend.</summary>
        public const int PaletteBlendOffset = 15;
}
