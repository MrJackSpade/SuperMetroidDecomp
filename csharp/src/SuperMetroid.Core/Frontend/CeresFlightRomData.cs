namespace SuperMetroid.Core.Frontend;

/// <summary>Native sources and DMA geometry for the opening flight toward Ceres.</summary>
public static class CeresFlightRomData
{
    public static class Assets
    {
        /// <summary>$8C:E5E9, the 256-color Ceres approach palette.</summary>
        public const int Palette = 0x8ce5e9;
        /// <summary>$95:A82F, compressed Mode-7 character bytes.</summary>
        public const int Mode7Characters = 0x95a82f;
        /// <summary>$96:FE69, consecutive front and rear Mode-7 map slices.</summary>
        public const int Mode7Maps = 0x96fe69;
        /// <summary>$96:D10A, compressed ship/asteroid/colony OBJ characters.</summary>
        public const int ObjectCharacters = 0x96d10a;
    }

    public static class Vram
    {
        /// <summary>$8B:BCCD writes 16 KiB through Mode-7's high-byte port.</summary>
        public const int Mode7CharacterByteCount = 0x4000;
        /// <summary>$8B:BCCD fills the corresponding low-byte map plane with tile $8C.</summary>
        public const int Mode7MapFillWordCount = 0x4000;
        public const byte Mode7BlankMapTile = 0x8c;
        /// <summary>The front and rear views each replace 32 by 24 low-byte map cells.</summary>
        public const int Mode7MapSliceByteCount = 0x0300;
        public const int Mode7MapByteCount = 2 * Mode7MapSliceByteCount;
        /// <summary>The Ceres OBJ transfer occupies VRAM bytes $C000-$FFFF.</summary>
        public const int ObjectCharacterDestinationByte = 0xc000;
        public const int ObjectCharacterByteCount = 0x4000;
    }

    public static class Layers
    {
        /// <summary>Mode-1 SPACE COLONY BG1 tilemap base after the approach.</summary>
        public const ushort SpaceColonyTilemapWord = 0x5c00;
        /// <summary>Mode-1 SPACE COLONY BG1 character base within the OBJ transfer.</summary>
        public const ushort SpaceColonyCharacterWord = 0x6000;
    }
}
