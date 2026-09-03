namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge tables and fixed VRAM layouts used to draw Samus.</summary>
/// <remarks>
/// This catalog owns rendering data only. Animation state, the currently selected DMA
/// definition, OAM positions, and frame counters remain on their runtime state objects.
/// </remarks>
public static class SamusRenderingRomData
{
    /// <summary>Native banks used by the rendering data's same-bank pointers.</summary>
    public static class Banks
    {
        /// <summary>Bank containing drawing offsets and arm-cannon metadata.</summary>
        public const int Movement = 0x900000;

        /// <summary>Eight-bit bank number paired with native arm-cannon offsets.</summary>
        public const byte MovementNumber = 0x90;

        /// <summary>Bank containing spritemaps and Samus DMA definitions.</summary>
        public const int GraphicsDefinitions = 0x920000;

        /// <summary>Bank containing decompressed-source character data.</summary>
        public const int CharacterData = 0x9a0000;
    }

    /// <summary>Body spritemap selection, position offsets, and ordinary suit palettes.</summary>
    public static class Body
    {
        /// <summary><c>$92:9263</c>, base top-spritemap index for each pose.</summary>
        public const int TopSpritemapBaseIndices = 0x929263;

        /// <summary><c>$92:945D</c>, base bottom-spritemap index for each pose.</summary>
        public const int BottomSpritemapBaseIndices = 0x92945d;

        /// <summary><c>$9B:9400</c>, ordinary Power Suit palette.</summary>
        public const int PowerSuitPalette = 0x9b9400;

        /// <summary><c>$9B:9520</c>, ordinary Varia Suit palette.</summary>
        public const int VariaSuitPalette = 0x9b9520;

        /// <summary><c>$9B:9800</c>, ordinary Gravity Suit palette.</summary>
        public const int GravitySuitPalette = 0x9b9800;

        /// <summary>Number of colors copied into Samus's OBJ palette.</summary>
        public const int SuitPaletteColorCount = 16;

        /// <summary>First CGRAM color occupied by Samus's OBJ palette.</summary>
        public const int SuitPaletteCgramIndex = 192;

        /// <summary><c>$90:8D28</c>, packed landing-frame vertical offsets.</summary>
        public const int LandingVerticalOffsets = 0x908d28;

        /// <summary><c>$90:8D80</c>, crouch/morph transition vertical offsets.</summary>
        public const int PostureTransitionVerticalOffsets = 0x908d80;

        /// <summary><c>$90:8DEF</c>, drained-body vertical offsets.</summary>
        public const int DrainedVerticalOffsets = 0x908def;
    }

    /// <summary>Seven-byte body-character DMA definitions and their fixed destinations.</summary>
    public static class TileTransfers
    {
        /// <summary><c>$92:D94E</c>, animation-record-list pointer for each pose.</summary>
        public const int AnimationDefinitionListPointers = 0x92d94e;

        /// <summary><c>$92:D91E</c>, top-definition-list pointer for each graphics set.</summary>
        public const int TopDefinitionListPointers = 0x92d91e;

        /// <summary><c>$92:D938</c>, bottom-definition-list pointer for each graphics set.</summary>
        public const int BottomDefinitionListPointers = 0x92d938;

        /// <summary>Number of top-half graphics sets before the bottom pointer table.</summary>
        public const int TopDefinitionSetCount =
            (BottomDefinitionListPointers - TopDefinitionListPointers) / sizeof(ushort);

        /// <summary>Number of bottom-half graphics sets before the animation pointer table.</summary>
        public const int BottomDefinitionSetCount =
            (AnimationDefinitionListPointers - BottomDefinitionListPointers) / sizeof(ushort);

        /// <summary>Bytes in one pose/frame selector: top set/position and bottom set/position.</summary>
        public const int AnimationRecordByteCount = 4;

        /// <summary>Bytes in one source-address/two-size DMA definition.</summary>
        public const int DefinitionByteCount = 7;

        /// <summary>Bottom-set sentinel meaning that a frame does not upload lower-body art.</summary>
        public const byte NoBottomTransferSet = 0xff;

        /// <summary>VRAM word destinations for the two pieces of one body-half upload.</summary>
        public readonly record struct SplitVramDestinations(ushort First, ushort Second);

        /// <summary>Fixed destinations used by the top-half definition.</summary>
        public static readonly SplitVramDestinations TopDestinations = new(0x6000, 0x6100);

        /// <summary>Fixed destinations used by the bottom-half definition.</summary>
        public static readonly SplitVramDestinations BottomDestinations = new(0x6080, 0x6180);
    }

    /// <summary>Arm-cannon cover state, OAM, character-list, and DMA definitions.</summary>
    public static class ArmCannon
    {
        /// <summary><c>$90:C7D9</c>, desired open flag for each HUD selection.</summary>
        public const int OpenFlags = 0x90c7d9;

        /// <summary><c>$90:C7DF</c>, pose-indexed drawing-data pointers.</summary>
        public const int PoseDrawingDataPointers = 0x90c7df;

        /// <summary><c>$90:C791</c>, packed OBJ attributes for ten directions.</summary>
        public const int SpriteAttributes = 0x90c791;

        /// <summary><c>$90:C7A5</c>, direction-indexed tile-list pointers.</summary>
        public const int TileListPointers = 0x90c7a5;

        /// <summary>Bytes uploaded for the cover's single 4bpp 8x8 character.</summary>
        public const ushort TileUploadByteCount = 0x20;

        /// <summary>Encoded VRAM destination occupied by OBJ character $1F.</summary>
        public const ushort TileVramDestination = 0x61f0;

        /// <summary>Number of direction selectors accepted by the native dispatcher.</summary>
        public const int DirectionCount = 10;
    }
}
