using System.Collections.Frozen;
using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the title sequence's artwork and card lists (#1416): the import pipeline's private knowledge
/// of the cartridge, decoded once into the installed title documents.
/// </summary>
internal static class TitleSourceDefinitions
{
    /// <summary>$8C:E1E9, Palettes_TitleScreen: the title's full 256-colour palette.</summary>
    internal const int Palette = 0x8ce1e9;

    /// <summary>$94:E000, Tiles_Title_Background_Mode7: the title floor's compressed characters.</summary>
    internal const int FloorCharacters = 0x94e000;

    /// <summary>$96:FC04, Title_Mode7_Tilemap: the title floor's compressed map.</summary>
    internal const int FloorMap = 0x96fc04;

    /// <summary>$95:80D8, Tiles_Title_Sprite: the compressed characters of the title's text, logo and copyright.</summary>
    internal const int ObjectCharacters = 0x9580d8;

    /// <summary>$95:A5E1, Tiles_Baby_Metroid_Mode7: the Baby Metroid's compressed animation pages.</summary>
    internal const int BabyMetroidCharacters = 0x95a5e1;

    /// <summary>
    /// The bank-$8B instruction list of each title card, initialized by $8B:9CBC, $8B:9D4A, $8B:9DC3 and $8B:9E45.
    /// </summary>
    internal static readonly FrozenDictionary<TitleSequencePhase, int> TextLists = new Dictionary<TitleSequencePhase, int>
    {
        [TitleSequencePhase.YearText] = 0x8ba03d,
        [TitleSequencePhase.NintendoText] = 0x8ba055,
        [TitleSequencePhase.PresentsText] = 0x8ba079,
        [TitleSequencePhase.MetroidThreeText] = 0x8ba09d,
    }.ToFrozenDictionary();

    /// <summary>The bytes of one timed card-list entry: a duration word, then a bank-$8C spritemap word.</summary>
    internal const int TimedEntryByteCount = 4;

    /// <summary>The high bit that marks a card-list word as a command rather than a duration.</summary>
    internal const ushort CommandBit = 0x8000;

    /// <summary>The OBSEL value $03 title setup writes: sprite characters at video byte $C000, size selector zero.</summary>
    internal const byte ObjectSizeAndBaseSelector = 0x03;
}
