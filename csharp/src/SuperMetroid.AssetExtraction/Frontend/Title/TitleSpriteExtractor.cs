using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// Decodes the title compositions from the bank-$8C spritemaps the four native title text lists select. This codec is
/// the only place that knows which cartridge spritemap holds each <see cref="TitleSprite"/>.
/// </summary>
[CpuBusBoundary("codec: ROM bank-$8C title spritemap parts → title sprites")]
internal static class TitleSpriteExtractor
{
    /// <summary>Decodes every title composition's ordered OAM parts, keyed by its semantic name.</summary>
    public static TitleSpriteFrame[] Extract(ISnesAddressSpace bus)
    {
        IReadOnlyDictionary<TitleSprite, ushort> spritemaps = MapSprites(bus);
        return TitleSpriteDefinitions.All.Select(sprite => new TitleSpriteFrame
        {
            Sprite = sprite,
            Parts = ReadParts(bus, spritemaps[sprite]),
        }).ToArray();
    }

    /// <summary>
    /// Maps each title composition to the bank-$8C spritemap that stores it. Each card's native list is walked beside
    /// its typed steps: timed entry N shows the composition of typed step N, a blank typed frame must select no
    /// spritemap, and the list must end with a command where the typed card does. The logo comes from the title's
    /// logo pointer and the copyright from its fixed spritemap.
    /// </summary>
    /// <exception cref="InvalidDataException">A native list disagrees with its typed card, or two compositions share a spritemap.</exception>
    internal static IReadOnlyDictionary<TitleSprite, ushort> MapSprites(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        var spritemaps = new Dictionary<TitleSprite, ushort>
        {
            [TitleSprite.SuperMetroidLogo] = RomDataReader.ReadWordFixedBank(cartridge, TitleSequenceRomData.Sprites.LogoPointerAddress),
            [TitleSprite.NintendoCopyright] = TitleSequenceRomData.Sprites.NintendoCopyright,
        };
        foreach (TitleTextSequenceDefinition sequence in new[]
                 {
                     TitleSequenceRomData.TextSequences.Year,
                     TitleSequenceRomData.TextSequences.Nintendo,
                     TitleSequenceRomData.TextSequences.Presents,
                     TitleSequenceRomData.TextSequences.MetroidThree,
                 })
        {
            int entry = sequence.InstructionAddress;
            foreach (TitleCardStep step in TitleCards.Steps(sequence.Phase))
            {
                ushort durationOrCommand = RomDataReader.ReadWordFixedBank(cartridge, entry);
                bool nativeCommand = (durationOrCommand & TitleSequenceRomData.TextSequences.CommandBit) != 0;
                if (step.Command is not null)
                {
                    if (!nativeCommand)
                        throw new InvalidDataException($"Title card {sequence.Phase} continues where its typed steps run a command.");
                    break;
                }
                if (nativeCommand)
                    throw new InvalidDataException($"Title card {sequence.Phase} ends before its typed steps do.");
                ushort spritemap = RomDataReader.ReadWordFixedBank(cartridge, entry + sizeof(ushort));
                if (step.Sprite is not { } sprite)
                {
                    if (spritemap != TitleSequenceRomData.Sprites.Blank)
                        throw new InvalidDataException($"Title card {sequence.Phase} shows art where its typed step is blank.");
                }
                else if (!spritemaps.TryAdd(sprite, spritemap))
                {
                    throw new InvalidDataException($"Title composition {sprite} appears twice in the title cards.");
                }
                entry += TitleSequenceRomData.TextSequences.TimedEntryByteCount;
                if (SnesAddressMath.OffsetInBank(entry) >= SnesAddressMath.OffsetInBank(TitleSequenceRomData.Sprites.LogoPointerAddress))
                    throw new InvalidDataException("Title text list did not terminate before the logo pointer.");
            }
        }

        if (spritemaps.Count != TitleSpriteDefinitions.All.Count ||
            spritemaps.Values.Distinct().Count() != spritemaps.Count)
            throw new InvalidDataException(
                "Imported title spritemaps do not give every title composition its own spritemap.");
        return spritemaps;
    }

    /// <summary>Decodes one bank-$8C title spritemap into bounded offsets, tile positions, and flip attributes.</summary>
    private static SpriteVisualPart[] ReadParts(ISnesAddressSpace bus, ushort pointer)
    {
        int address = ((int)new SnesAddress(TitleSequenceRomData.Sprites.Bank, pointer));
        int count = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
        if (count > TitleGraphicsFormat.MaximumSpriteParts)
            throw new InvalidDataException($"Title sprite $8C:{pointer:X4} exceeds OAM capacity.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int source = address + 2 + index * 5;
            var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source));
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source + 3));
            parts[index] = new SpriteVisualPart
            {
                OffsetX = x.SignedOffset,
                OffsetY = unchecked((sbyte)bus.ReadCartridgeByte(source + 2)),
                TileColumn = attributes.TileNumber % TitleGraphicsFormat.ObjectTileColumns,
                TileRow = attributes.TileNumber / TitleGraphicsFormat.ObjectTileColumns,
                Size = x.IsLarge ? 16 : 8,
                Priority = attributes.Priority,
                Palette = null,
                FlipX = attributes.FlipHorizontally,
                FlipY = attributes.FlipVertically,
            };
        }
        return parts;
    }
}
