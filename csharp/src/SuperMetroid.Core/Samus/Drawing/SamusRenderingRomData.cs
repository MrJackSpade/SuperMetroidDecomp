namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Immutable cartridge tables used to draw Samus.</summary>
/// <remarks>
/// This catalog owns rendering data only. Animation state, the shown body definitions,
/// sprite positions, and frame counters remain on their runtime state objects.
/// </remarks>
public static class SamusRenderingRomData
{
    /// <summary>Native banks used by the rendering data's same-bank pointers.</summary>
    public static class Banks
    {
        /// <summary>Bank containing drawing offsets and arm-cannon metadata.</summary>
        public const int Movement = 0x900000;

        /// <summary>Bank containing decompressed-source character data.</summary>
        public const int CharacterData = 0x9a0000;
    }

    /// <summary>Body spritemap selection, position offsets, and ordinary suit palettes.</summary>
    public static class Body
    {

        /// <summary><c>$9B:9400</c>, ordinary Power Suit palette.</summary>
        /// <remarks>
        /// Issue #860 / #625: sixteen authored BGR555 words in the pinned NTSC
        /// J/U v1.0 ROM, indexed only by color 0..15 and copied unchanged to
        /// Samus OBJ CGRAM 192..207. All sixteen match the normal row of the
        /// Power loading palette-FX program (#856) and the duplicate bank-$9B
        /// palettes at <c>$9C20</c> and <c>$9CA0</c>. Eleven color slots are
        /// shared with the normal Varia and Gravity palettes; slots
        /// 0, 2, 10, 11, 12 vary by suit. These deliberately selected colors
        /// remain a palette table rather than a computed channel sequence.
        /// </remarks>
        public const int PowerSuitPalette = 0x9b9400;

        /// <summary><c>$9B:9520</c>, ordinary Varia Suit palette.</summary>
        /// <remarks>
        /// Issue #861 / #625: sixteen authored BGR555 words in the pinned NTSC
        /// J/U v1.0 ROM, indexed only by color 0..15 and copied unchanged to
        /// Samus OBJ CGRAM 192..207. All sixteen match the normal row of the
        /// Varia loading palette-FX program (#857) and duplicate bank-$9B
        /// palettes at <c>$9920</c>, <c>$9D20</c>, and <c>$9DA0</c>. Eleven
        /// color slots are shared with normal Power and Gravity; slots
        /// 0, 2, 10, 11, 12 vary by suit. These authored choices remain a
        /// palette table rather than a computed channel sequence.
        /// </remarks>
        public const int VariaSuitPalette = 0x9b9520;

        /// <summary><c>$9B:9800</c>, ordinary Gravity Suit palette.</summary>
        /// <remarks>
        /// Issue #862 / #625: sixteen authored BGR555 words in the pinned NTSC
        /// J/U v1.0 ROM, indexed only by color 0..15 and copied unchanged to
        /// Samus OBJ CGRAM 192..207. All sixteen match the normal row of the
        /// Gravity loading palette-FX program (#858). The nearby speed-boost
        /// base <c>$9B:9F20</c> is not an alias: colors 0, 1, and 12 differ.
        /// Eleven color slots are shared with normal Power and Varia; slots
        /// 0, 2, 10, 11, 12 vary by suit. These authored choices remain a
        /// palette table rather than a computed channel sequence.
        /// </remarks>
        public const int GravitySuitPalette = 0x9b9800;

        /// <summary><c>$90:8D28</c>, packed landing-frame vertical offsets.</summary>
        public const int LandingVerticalOffsets = 0x908d28;
        /// <summary>Sixteen landing bytes plus the adjacent byte read by the final unaligned word.</summary>
        public const int LandingVerticalOffsetByteCount = 17;

        /// <summary><c>$90:8D80</c>, crouch/morph transition vertical offsets.</summary>
        public const int PostureTransitionVerticalOffsets = 0x908d80;
        /// <summary>Twelve transition poses with two visual offset bytes each.</summary>
        public const int PostureTransitionVerticalOffsetByteCount = 24;

        /// <summary><c>$90:8DEF</c>, drained-body vertical offsets.</summary>
        public const int DrainedVerticalOffsets = 0x908def;
        /// <summary>Native contiguous drained-body visual offset interval.</summary>
        public const int DrainedVerticalOffsetByteCount = 32;
    }

    /// <summary>Arm-cannon cover state, OAM, character-list, and DMA definitions.</summary>
    public static class ArmCannon
    {

        /// <summary><c>$90:C791</c>, packed OBJ attributes for ten directions.</summary>
        public const int SpriteAttributes = 0x90c791;


        /// <summary>Bytes uploaded for the cover's single 4bpp 8x8 character.</summary>
        public const ushort TileUploadByteCount = 0x20;

        /// <summary>Number of direction selectors accepted by the native dispatcher.</summary>
        public const int DirectionCount = 10;
    }
}
