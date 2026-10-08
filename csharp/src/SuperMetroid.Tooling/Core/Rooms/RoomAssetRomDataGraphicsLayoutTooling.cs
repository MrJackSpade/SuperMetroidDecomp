namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="RoomAssetRomData.GraphicsLayout"/>; never linked by player hosts.</summary>
internal static class RoomAssetRomDataGraphicsLayoutTooling
{
        /// <summary>Bytes occupied by one SNES four-bit-per-pixel 8x8 character.</summary>
        public const int BytesPer4BppCharacter = 32;
        /// <summary>Bytes modeled by the static room renderer's BG character allocation.</summary>
        public const int BackgroundCharacterVramByteCount = 0x8000;
        /// <summary>Bytes in the decompressed CRE 16x16 block-definition table.</summary>
        public const int CreBlockDefinitionsByteCount = 0x0800;
        /// <summary>Number of CRE block definitions preceding area-specific blocks.</summary>
        public const int CreBlockDefinitionCount =
            CreBlockDefinitionsByteCount / RoomAssetRomData.GraphicsLayout.BytesPerBlockDefinition;
}
