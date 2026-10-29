using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Builds Samus rendering catalogs from distinctive synthetic tables and installed stock art.</summary>
internal static class SamusRenderingFixtureVerification
{
    /// <summary>Loads the installed body artwork only when a rendering fixture first needs it.</summary>
    private static readonly Lazy<InstalledSamusBodyArtwork> renderingFixtureStock =
        new(() => InstalledFixtures.SamusBody);

    /// <summary>Caches complete catalogs whose every pose uses a requested graphics Y offset.</summary>
    private static readonly Dictionary<sbyte, InstalledSamusBodyArtwork> graphicsOffsetFixtures = [];

    /// <summary>Builds or reuses a stock body catalog with every pose's graphics Y offset replaced.</summary>
    internal static InstalledSamusBodyArtwork SamusGraphicsOffsetFixture(sbyte offset)
    {
        if (graphicsOffsetFixtures.TryGetValue(offset, out var existing)) return existing;
        var stock = renderingFixtureStock.Value;
        var result = new InstalledSamusBodyArtwork(new SamusBodyArtworkParts { TopSetStarts = stock.TopSetStarts.ToArray(), BottomSetStarts = stock.BottomSetStarts.ToArray(), PoseFirstFrames = stock.PoseFirstFrames.ToArray(), GraphicsYOffsets = Enumerable.Repeat(offset, InstalledSamusBodyArtwork.PoseCount).ToArray(), Frames = stock.Frames.ToArray(), Spritemaps = stock.Spritemaps, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTilesContentIdentity = stock.DeathTilesContentIdentity, ArmCannon = stock.ArmCannon, LandingYOffsets = stock.LandingYOffsets.ToArray(), PostureYOffsets = stock.PostureYOffsets.ToArray(), DrainedYOffsets = stock.DrainedYOffsets.ToArray() }, stock.BodyCharactersContentIdentity);
        graphicsOffsetFixtures.Add(offset, result);
        return result;
    }

    /// <summary>Points one pose's <c>$92:D94E</c> tile-animation list pointer at a synthetic record.</summary>
    internal static void WriteSamusTileAnimationPointer(TestAddressSpace bus, SamusPoseId pose, ushort pointer) =>
        WriteTestWord(bus, SamusSourceDefinitions.AnimationDefinitionListPointers.Value + (int)pose * 2, pointer);

    /// <summary>Seeds one pose's <c>$92:9263/$92:945D</c> top and bottom spritemap base indices.</summary>
    internal static void WriteSamusSpritemapBases(TestAddressSpace bus, SamusPoseId pose, ushort top, ushort bottom)
    {
        WriteTestWord(bus, InstalledSamusSpritemaps.TopBaseAddress + (int)pose * 2, top);
        WriteTestWord(bus, InstalledSamusSpritemaps.BottomBaseAddress + (int)pose * 2, bottom);
    }
    /// <summary>
    /// Adapts sparse seeded rendering tables to the installed catalog boundary. Unspecified
    /// complete DMA groups retain stock layout; spritemaps use the fixture's own compact
    /// compositions, including zero pointers.
    /// </summary>
    internal static void BindSyntheticSamusRendering(TestAddressSpace bus, SamusState samus)
    {
        InstalledSamusBodyArtwork stock = renderingFixtureStock.Value;
        SamusBodyCharacterArtwork stockCharacters = SamusBodySheetFixtures.CharactersOf(stock);
        ushort Word(int address) => LittleEndianBytes.Word(bus.ReadByte(address), bus.ReadByte(address + 1));
        ushort[] Words(int address, int count) => Enumerable.Range(0, count)
            .Select(index => Word(address + index * 2)).ToArray();
        ushort[] OverrideWords(int address, ushort[] original)
        {
            for (int index = 0; index < original.Length; index++)
                if (bus.HasSeededByte(address + index * 2)) original[index] = Word(address + index * 2);
            return original;
        }
        sbyte[] OverrideBytes(int address, sbyte[] original)
        {
            for (int index = 0; index < original.Length; index++)
                if (bus.HasSeededByte(address + index)) original[index] = unchecked((sbyte)bus.ReadByte(address + index));
            return original;
        }
        var topPointers = OverrideWords(SamusSourceDefinitions.TopDefinitionListPointers.Value, stock.TopSetPointers.ToArray());
        var bottomPointers = OverrideWords(SamusSourceDefinitions.BottomDefinitionListPointers.Value, stock.BottomSetPointers.ToArray());
        var posePointers = OverrideWords(SamusSourceDefinitions.AnimationDefinitionListPointers.Value, stock.PosePointers.ToArray());
        sbyte[] offsets = stock.GraphicsYOffsets.ToArray();
        for (int pose = 0; pose < offsets.Length; pose++)
        {
            int address = SamusSourceDefinitions.PoseDefinitions.Value + pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4;
            if (bus.HasSeededByte(address)) offsets[pose] = unchecked((sbyte)bus.ReadByte(address));
        }
        SamusBodyFrameSelection[] frames = stock.Frames.ToArray();
        for (int index = 0; index < frames.Length; index++)
        {
            int address = InstalledSamusBodyArtwork.FirstFrameAddress + index * 4;
            if (bus.HasSeededByte(address)) frames[index] = new(bus.ReadByte(address), bus.ReadByte(address + 1),
                bus.ReadByte(address + 2), bus.ReadByte(address + 3));
        }
        SamusBodyTileTransfer[][] Groups(bool upper, ushort[] pointers)
        {
            var groups = new SamusBodyTileTransfer[pointers.Length][];
            for (int set = 0; set < groups.Length; set++)
            {
                groups[set] = (upper ? stockCharacters.TopSet(set) : stockCharacters.BottomSet(set)).ToArray();
                for (int position = 0; position < groups[set].Length; position++)
                {
                    int address = 0x920000 | (pointers[set] + position * 7);
                    if (!bus.HasSeededByte(address)) continue;
                    int source = (int)new SnesAddress(bus.ReadByte(address + 2), Word(address));
                    ushort first = Word(address + 3), second = Word(address + 5);
                    byte[] bytes = Enumerable.Range(0, first + second).Select(index => bus.ReadByte(source + index)).ToArray();
                    groups[set][position] = new(first, second, bytes);
                }
            }
            return groups;
        }
        ushort[] spritePointers = Words(InstalledSamusSpritemaps.PointerTableAddress, SamusSpritemapSources.EntryCount);
        SamusSpritemapDefinition[] maps = SamusSpritemapNative.Compositions(spritePointers).Select(pointer =>
        {
            int address = 0x920000 | pointer;
            var parts = new SpritePart[Word(address)];
            for (int index = 0; index < parts.Length; index++)
            {
                int part = address + 2 + index * 5;
                // The fixture's five-byte records are decoded by the import codec into the pieces they describe.
                parts[index] = new SpritemapPart(new SnesSpritemapXWord(Word(part)), new SnesSpritemapYByte(bus.ReadByte(part + 2)),
                    new SnesObjAttributeWord(Word(part + 3))).ToPart();
            }
            return new SamusSpritemapDefinition(parts);
        }).ToArray();
        var sprites = new SamusSpritemapSources(
            Words(InstalledSamusSpritemaps.TopBaseAddress, InstalledSamusBodyArtwork.PoseCount),
            Words(InstalledSamusSpritemaps.BottomBaseAddress, InstalledSamusBodyArtwork.PoseCount), SamusSpritemapNative.Entries(spritePointers), maps);
        ushort[] landing = stock.LandingYOffsets.ToArray();
        for (int index = 0; index < landing.Length; index++)
            if (bus.HasSeededByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + index))
                landing[index] = bus.ReadByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + index);
        samus.Presentation.BodyFrames.BindBodyFixture(SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(topPointers), BottomSetStarts = SamusBodyNative.Records(bottomPointers), PoseFirstFrames = SamusBodyNative.Frames(posePointers), GraphicsYOffsets = offsets, Frames = frames, Spritemaps = sprites, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTilesContentIdentity = stock.DeathTilesContentIdentity, ArmCannon = stock.ArmCannon, LandingYOffsets = landing, PostureYOffsets = OverrideBytes(SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value, stock.PostureYOffsets.ToArray()), DrainedYOffsets = OverrideBytes(SamusSourceDefinitions.DrainedVerticalOffsets.Value, stock.DrainedYOffsets.ToArray()) }, Groups(true, topPointers), Groups(false, bottomPointers)));
    }
}
