using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>Verified bank-$85 message-box tables, routines, layout, and PPU values.</summary>
public static class GameplayMessageRomData
{
    public static class Assets
    {
        public const int DefinitionTable = 0x85869b;
        public const int LargeBorder = 0x858000;
        public const int SmallBorder = 0x858040;
        public const int SaveSelectionTilemap = 0x859581;
        public const int BankBase = 0x850000;
    }

    public static class Routines
    {
        public const ushort DrawLargeTilemap = 0x825a;
        public const ushort DrawSmallTilemap = 0x8289;
        public const ushort PatchShootButton = 0x83c5;
        public const ushort PatchRunButton = 0x83cc;
        public const ushort SetupSmall = 0x8436;
        public const ushort SetupLarge = 0x8441;
    }

    public static class Layout
    {
        public const int DefinitionBytes = 6;
        public const int TilemapWidth = 32;
        public const int BorderRows = 2;
        public const int MinimumRows = 3;
        public const int MaximumRows = 6;
        public const int TilePixels = 8;
        public const int SaveSelectionDestinationWord = 128;
        public const int SaveSelectionRowWords = 32;
        public const int SaveSelectionYesSourceWord = 32;
        public const int SaveSelectionNoSourceWord = 64;
        public const ushort CharacterBaseWord = 0x4000;
        public const int VramWordMask = 0x7fff;
        public const int WindowCenterY = 124;
    }

    public static class Timing
    {
        public const int MaximumRadiusPixels = 24;
        public const int RadiusStepPixels = 2;
        public const int ItemMinimumDisplayFrames = 360;
        public const int StationMinimumDisplayFrames = 10;
    }

    public static class Palette
    {
        public const int TemporaryLightIndex = 25;
        public const ushort TemporaryLightColor = 0x0bb1;
        public const int TemporaryDarkIndex = 26;
        public const ushort TemporaryDarkColor = 0x001f;
    }

    public static class Buttons
    {
        // Order is observable for malformed multi-button bindings because the cartridge
        // uses sequential BIT tests and accepts the first match.
        private static readonly GameplayMessageButtonGlyph[] OrderedGlyphs =
        [
            new(SnesButton.A, new SnesBgTilemapWord(0x28e0)),
            new(SnesButton.B, new SnesBgTilemapWord(0x3ce1)),
            new(SnesButton.X, new SnesBgTilemapWord(0x2cf7)),
            new(SnesButton.Y, new SnesBgTilemapWord(0x38f8)),
            new(SnesButton.Select, new SnesBgTilemapWord(0x38d0)),
            new(SnesButton.L, new SnesBgTilemapWord(0x38eb)),
            new(SnesButton.R, new SnesBgTilemapWord(0x38f1)),
        ];

        public static ReadOnlySpan<GameplayMessageButtonGlyph> Glyphs => OrderedGlyphs;

        public static readonly SnesBgTilemapWord UnknownGlyph = new(0x284e);

        // Literal byte offsets from MessageBoxTilemap at $85:8749, indexed by ID - 1.
        public static ReadOnlySpan<ushort> SpecialGlyphByteOffsets =>
        [
            0x000, 0x12a, 0x12a, 0x12c, 0x12c, 0x12c, 0x000, 0x000,
            0x000, 0x000, 0x000, 0x000, 0x120, 0x000, 0x000, 0x000,
            0x000, 0x000, 0x12a, 0x000, 0x000, 0x000, 0x000, 0x000,
            0x000, 0x000, 0x000,
        ];
    }
}

/// <summary>One ordered controller-mask to ROM tilemap-glyph mapping.</summary>
public readonly record struct GameplayMessageButtonGlyph(
    SnesButton Button,
    SnesBgTilemapWord Glyph);
