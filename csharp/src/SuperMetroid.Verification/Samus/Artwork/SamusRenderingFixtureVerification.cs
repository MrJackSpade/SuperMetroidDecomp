using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Builds Samus rendering catalogs from distinctive synthetic tables and installed stock art.</summary>
internal static class SamusRenderingFixtureVerification
{
    /// <summary>Loads the installed body artwork only when a rendering fixture first needs it.</summary>
    private static readonly Lazy<SamusBodyArtworkCatalog> renderingFixtureStock =
        new(() => InstalledFixtures.SamusBody);

    /// <summary>Caches complete catalogs whose every pose uses a requested graphics Y offset.</summary>
    private static readonly Dictionary<sbyte, SamusBodyArtworkCatalog> graphicsOffsetFixtures = [];

    /// <summary>Builds or reuses a stock body catalog with every pose's graphics Y offset replaced.</summary>
    internal static SamusBodyArtworkCatalog SamusGraphicsOffsetFixture(sbyte offset)
    {
        if (graphicsOffsetFixtures.TryGetValue(offset, out var existing)) return existing;
        var stock = renderingFixtureStock.Value;
        var result = new SamusBodyArtworkCatalog(new SamusBodyArtworkParts { TopSetStarts = stock.TopSetStarts.ToArray(), BottomSetStarts = stock.BottomSetStarts.ToArray(), PoseFirstFrames = stock.PoseFirstFrames.ToArray(), GraphicsYOffsets = Enumerable.Repeat(offset, SamusBodyArtworkCatalog.PoseCount).ToArray(), Frames = stock.Frames.ToArray(), Top = Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => stock.TopSet(index).ToArray()).ToArray(), Bottom = Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => stock.BottomSet(index).ToArray()).ToArray(), Spritemaps = stock.Spritemaps, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTilesContentIdentity = stock.DeathTilesContentIdentity, ArmCannon = stock.ArmCannon, LandingYOffsets = stock.LandingYOffsets.ToArray(), PostureYOffsets = stock.PostureYOffsets.ToArray(), DrainedYOffsets = stock.DrainedYOffsets.ToArray() });
        graphicsOffsetFixtures.Add(offset, result);
        return result;
    }

    /// <summary>Points one pose's <c>$92:D94E</c> tile-animation list pointer at a synthetic record.</summary>
    internal static void WriteSamusTileAnimationPointer(TestAddressSpace bus, SamusPoseId pose, ushort pointer) =>
        WriteTestWord(bus, SamusSourceDefinitions.AnimationDefinitionListPointers.Value + (int)pose * 2, pointer);

    /// <summary>Seeds one pose's <c>$92:9263/$92:945D</c> top and bottom spritemap base indices.</summary>
    internal static void WriteSamusSpritemapBases(TestAddressSpace bus, SamusPoseId pose, ushort top, ushort bottom)
    {
        WriteTestWord(bus, SamusSpritemapArtworkCatalog.TopBaseAddress + (int)pose * 2, top);
        WriteTestWord(bus, SamusSpritemapArtworkCatalog.BottomBaseAddress + (int)pose * 2, bottom);
    }
    /// <summary>
    /// Adapts sparse seeded rendering tables to the installed catalog boundary. Unspecified
    /// complete DMA groups retain stock layout; spritemaps use the fixture's own compact
    /// compositions, including zero pointers.
    /// </summary>
    internal static void BindSyntheticSamusRendering(TestAddressSpace bus, SamusState samus)
    {
        SamusBodyArtworkCatalog stock = renderingFixtureStock.Value;
        ushort Word(int address) => SnesLittleEndian.Word(bus.ReadByte(address), bus.ReadByte(address + 1));
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
            int address = SamusBodyArtworkCatalog.FirstFrameAddress + index * 4;
            if (bus.HasSeededByte(address)) frames[index] = new(bus.ReadByte(address), bus.ReadByte(address + 1),
                bus.ReadByte(address + 2), bus.ReadByte(address + 3));
        }
        SamusBodyTileDefinition[][] Groups(bool upper, ushort[] pointers)
        {
            var groups = new SamusBodyTileDefinition[pointers.Length][];
            for (int set = 0; set < groups.Length; set++)
            {
                groups[set] = (upper ? stock.TopSet(set) : stock.BottomSet(set)).ToArray();
                for (int position = 0; position < groups[set].Length; position++)
                {
                    int address = 0x920000 | (pointers[set] + position * 7);
                    if (!bus.HasSeededByte(address)) continue;
                    int source = (int)new SnesAddress(bus.ReadByte(address + 2), Word(address));
                    ushort first = Word(address + 3), second = Word(address + 5);
                    byte[] bytes = Enumerable.Range(0, first + second).Select(index => bus.ReadByte(source + index)).ToArray();
                    groups[set][position] = new(source, first, second, bytes);
                }
            }
            return groups;
        }
        ushort[] spritePointers = Words(SamusSpritemapArtworkCatalog.PointerTableAddress, SamusSpritemapArtworkCatalog.PointerCount);
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
        var sprites = new SamusSpritemapArtworkCatalog(
            Words(SamusSpritemapArtworkCatalog.TopBaseAddress, SamusBodyArtworkCatalog.PoseCount),
            Words(SamusSpritemapArtworkCatalog.BottomBaseAddress, SamusBodyArtworkCatalog.PoseCount), SamusSpritemapNative.Entries(spritePointers), maps);
        ushort[] landing = stock.LandingYOffsets.ToArray();
        for (int index = 0; index < landing.Length; index++)
            if (bus.HasSeededByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + index))
                landing[index] = bus.ReadByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + index);
        samus.Presentation.BodyFrames.BindBodyFixture(new SamusBodyArtworkCatalog(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(topPointers), BottomSetStarts = SamusBodyNative.Records(bottomPointers), PoseFirstFrames = SamusBodyNative.Frames(posePointers), GraphicsYOffsets = offsets, Frames = frames, Top = Groups(true, topPointers), Bottom = Groups(false, bottomPointers), Spritemaps = sprites, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTilesContentIdentity = stock.DeathTilesContentIdentity, ArmCannon = stock.ArmCannon, LandingYOffsets = landing, PostureYOffsets = OverrideBytes(SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value, stock.PostureYOffsets.ToArray()), DrainedYOffsets = OverrideBytes(SamusSourceDefinitions.DrainedVerticalOffsets.Value, stock.DrainedYOffsets.ToArray()) }));
    }
}
