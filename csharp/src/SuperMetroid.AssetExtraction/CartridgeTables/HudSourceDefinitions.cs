
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the gameplay HUD's tilemaps and glyph tables and the message boxes' definitions, borders
/// and selection rows (#1416): the import pipeline's private knowledge, decoded once into the installed HUD and message
/// documents.
/// </summary>
internal static class HudSourceDefinitions
{
    /// <summary>$80:988B, the HUD's static top row: a blank row with the minimap's border.</summary>
    internal const int TopRow = 0x80988b;

    /// <summary>$80:98CB, the HUD's three mutable rows.</summary>
    internal const int Template = 0x8098cb;

    /// <summary>$80:99A3, the Missile, Super Missile, Power Bomb, Grapple and X-Ray icon cells.</summary>
    internal const int Icons = 0x8099a3;

    /// <summary>$80:9DBF, the ten health-counter glyphs.</summary>
    internal const int HealthDigits = 0x809dbf;

    /// <summary>$80:9DD3, the ten ammunition-counter glyphs.</summary>
    internal const int AmmoDigits = 0x809dd3;

    /// <summary>$80:998B, the six filled then six empty AUTO indicator cells.</summary>
    internal const int AutoReserve = 0x80998b;

    /// <summary>The bytes from the filled AUTO cells to the empty ones.</summary>
    internal const int AutoReserveEmptyOffset = 12;

    /// <summary>The message boxes' sources in bank $85.</summary>
    internal static class Messages
    {
        /// <summary>$85:869B, the messages' setup, draw and content definition triples.</summary>
        internal const int DefinitionTable = 0x85869b;

        /// <summary>The bytes of one definition triple.</summary>
        internal const int DefinitionBytes = 6;

        /// <summary>$85:8000, the large box's border cells.</summary>
        internal const int LargeBorder = 0x858000;

        /// <summary>$85:8040, the small box's border cells.</summary>
        internal const int SmallBorder = 0x858040;

        /// <summary>$85:9581, the save prompt's YES/NO selection rows.</summary>
        internal const int SaveSelectionTilemap = 0x859581;

        /// <summary>The cell of <see cref="SaveSelectionTilemap"/> where the selected-YES row starts.</summary>
        internal const int SaveSelectionYesSourceWord = 32;

        /// <summary>The cell of <see cref="SaveSelectionTilemap"/> where the selected-NO row starts.</summary>
        internal const int SaveSelectionNoSourceWord = 64;

        /// <summary>Bank $85, which the definitions' content pointers address.</summary>
        internal const int BankBase = 0x850000;
    }
}
