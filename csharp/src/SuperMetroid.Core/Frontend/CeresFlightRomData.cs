namespace SuperMetroid.Core.Frontend;

/// <summary>Native sources and DMA geometry for the opening flight toward Ceres.</summary>
public static class CeresFlightRomData
{
    /// <summary>Import-time cartridge sources for the opening Ceres approach's palette, Mode 7 gunship art/maps, and shared space-scene OBJ characters.</summary>
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

    /// <summary>Native upload sizes and destinations for Mode 7's separate high-byte character and low-byte map planes, plus the ordinary byte-addressed OBJ transfer.</summary>
    public static class Vram
    {
        /// <summary>$8B:BCCD writes 16 KiB through Mode-7's high-byte port.</summary>
        public const int Mode7CharacterByteCount = 0x4000;
        /// <summary>$8B:BCCD fills the corresponding low-byte map plane with tile $8C.</summary>
        public const int Mode7MapFillWordCount = 0x4000;
        /// <summary>Character index $8C loaded at $8B:BD1F and written through $2118 to fill the low-byte map plane before replacing the current gunship map slice; not a palette index or fill color.</summary>
        public const byte Mode7BlankMapTile = 0x8c;
        /// <summary>The front and rear views each replace 32 by 24 low-byte map cells.</summary>
        public const int Mode7MapSliceByteCount = 0x0300;
        /// <summary>$0600 decoded bytes containing consecutive $0300-byte front and rear gunship map slices; only one slice is installed at a time.</summary>
        public const int Mode7MapByteCount = 2 * Mode7MapSliceByteCount;
        /// <summary>The Ceres OBJ transfer occupies VRAM bytes $C000-$FFFF.</summary>
        public const int ObjectCharacterDestinationByte = 0xc000;
        /// <summary>$4000 bytes of 4-bpp space/gunship/Ceres OBJ characters, covering 512 8-by-8 tiles; the later Mode-1 SPACE COLONY BG1 caption reuses this transfer.</summary>
        public const int ObjectCharacterByteCount = 0x4000;
    }

    /// <summary>Music queued by $8B:BCA0 before $8B:BDE4 waits for the queue to drain.</summary>
    public static class Music
    {
        /// <summary>$8B:BDD2: music-data index $2D, queued with the eight-frame delay.</summary>
        public const byte DataIndex = 0x2d;
        /// <summary>$8B:BDD9: track five within that data.</summary>
        public const byte Track = 5;
        /// <summary>$8B:BDDC: the Y delay argument queued with the track.</summary>
        public const ushort TrackDelayArgument = 0x000e;
    }

    /// <summary>Word-addressed BG1 layout used after the Mode 7 approach when the SPACE COLONY caption switches the scene to Mode 1.</summary>
    public static class Layers
    {
        /// <summary>Mode-1 SPACE COLONY BG1 tilemap base after the approach.</summary>
        public const ushort SpaceColonyTilemapWord = 0x5c00;
        /// <summary>Mode-1 SPACE COLONY BG1 character base within the OBJ transfer.</summary>
        public const ushort SpaceColonyCharacterWord = 0x6000;
    }
}
