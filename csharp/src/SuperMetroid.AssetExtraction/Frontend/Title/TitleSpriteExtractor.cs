using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// Decodes the title compositions from the bank-$8C spritemaps the four native title text lists select. This codec is
/// the only place that knows which cartridge spritemap holds each <see cref="TitleSprite"/> and each card's typing step.
/// </summary>
[EncodingOwner("cartridge title sprite instruction words")]
internal static partial class TitleSpriteExtractor
{
    /// <summary>Decodes the logo's and the copyright's ordered OAM parts, keyed by their semantic names.</summary>
    public static TitleSpriteFrame[] Extract(ISnesAddressSpace bus)
    {
        TitleSpritemaps spritemaps = MapSprites(bus);
        return TitleGraphicsFormat.Sprites.Select(sprite => new TitleSpriteFrame
        {
            Sprite = sprite,
            Parts = ReadParts(bus, spritemaps.Sprites[sprite]),
        }).ToArray();
    }

    /// <summary>Decodes each card typing step's ordered OAM parts, keyed by its card and the letters it has typed (#1979).</summary>
    public static TitleCardFrame[] ExtractCards(ISnesAddressSpace bus)
    {
        TitleSpritemaps spritemaps = MapSprites(bus);
        return TitleGraphicsFormat.Typings.Select(typed => new TitleCardFrame
        {
            Card = typed.Card,
            Letters = typed.Letters,
            Parts = ReadParts(bus, spritemaps.Cards[typed]),
        }).ToArray();
    }

    /// <summary>
    /// Maps each title composition and each card typing step to the bank-$8C spritemap that stores it. Each card's native
    /// list is walked beside its typed steps: timed entry N shows what typed step N shows, a blank typed frame must select
    /// no spritemap, and the list must end with a command where the typed card does. The logo comes from the title's
    /// logo pointer and the copyright from its fixed spritemap.
    /// </summary>
    /// <exception cref="InvalidDataException">A native list disagrees with its typed card, or two compositions share a spritemap.</exception>
    internal static TitleSpritemaps MapSprites(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = bus;
        var cards = new Dictionary<TitleCardTyping, int>();
        var spritemaps = new Dictionary<TitleSprite, int>
        {
            [TitleSprite.SuperMetroidLogo] = RomDataReader.ReadWordFixedBank(cartridge, FrontendSourceDefinitions.TitleLogoSpritemapOperand),
            [TitleSprite.NintendoCopyright] = FrontendSourceDefinitions.TitleCopyrightSpritemap,
        };
        foreach (TitleTextSequenceDefinition sequence in TitleSequenceDefinitions.TextSequences.All)
        {
            int entry = TitleSourceDefinitions.TextLists[sequence.Phase];
            foreach (AnimationBeat<TitleCardTyping, TitleCardCue> beat in TitleCards.Segments.Beats(sequence.Card.Card))
            {
                int durationOrCommand = RomDataReader.ReadWordFixedBank(cartridge, entry);
                bool nativeCommand = (durationOrCommand & TitleSourceDefinitions.CommandBit) != 0;
                if (beat is AnimationBeat<TitleCardTyping, TitleCardCue>.Event)
                {
                    if (!nativeCommand)
                        throw new InvalidDataException($"Title card {sequence.Phase} continues where its typed steps run a command.");
                    break;
                }
                if (nativeCommand)
                    throw new InvalidDataException($"Title card {sequence.Phase} ends before its typed steps do.");
                int spritemap = RomDataReader.ReadWordFixedBank(cartridge, entry + sizeof(ushort));
                if (beat.ShownFrame is not { } typed)
                {
                    if (spritemap != FrontendSourceDefinitions.TitleBlankSpritemap)
                        throw new InvalidDataException($"Title card {sequence.Phase} shows art where its typed step is blank.");
                }
                else if (!cards.TryAdd(typed, spritemap))
                {
                    throw new InvalidDataException($"Title card typing {typed} appears twice in the title cards.");
                }
                entry += TitleSourceDefinitions.TimedEntryByteCount;
                if (SnesAddressMath.OffsetInBank(entry) >= FrontendSourceDefinitions.TitleLogoSpritemapOperand.Offset)
                    throw new InvalidDataException("Title text list did not terminate before the logo pointer.");
            }
        }

        var mapped = new TitleSpritemaps(spritemaps, cards);
        if (spritemaps.Count != TitleGraphicsFormat.Sprites.Count || cards.Count != TitleGraphicsFormat.Typings.Count ||
            !mapped.AllDistinct)
            throw new InvalidDataException(
                "Imported title spritemaps do not give every title composition and card typing its own spritemap.");
        return mapped;
    }

    /// <summary>Decodes one bank-$8C title spritemap into bounded offsets, tile positions, and flip attributes.</summary>
    private static SpriteVisualPart[] ReadParts(ISnesAddressSpace bus, int pointer)
    {
        int address = ((int)SnesAddress.InBank(FrontendSourceDefinitions.TitleSpritemapBank, pointer));
        int count = RomDataReader.ReadWordFixedBank(bus, address);
        if (count > TitleGraphicsFormat.MaximumSpriteParts)
            throw new InvalidDataException($"Title sprite $8C:{pointer:X4} exceeds OAM capacity.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int source = address + 2 + index * 5;
            var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(bus, source));
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, source + 3));
            parts[index] = new SpriteVisualPart
            {
                OffsetX = x.SignedOffset,
                OffsetY = unchecked((sbyte)bus.ReadCartridgeByte(source + 2)),
                TileColumn = attributes.TileNumber % TitleGraphicsFormat.ObjectTileColumns,
                TileRow = attributes.TileNumber / TitleGraphicsFormat.ObjectTileColumns,
                Size = x.IsLarge ? 16 : 8,
                Depth = attributes.Depth,
                Palette = null,
                FlipX = attributes.FlipHorizontally,
                FlipY = attributes.FlipVertically,
            };
        }
        return parts;
    }
}
