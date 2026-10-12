using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly Lazy<SamusBodyArtworkCatalog> renderingFixtureStock =
        new(() => RepositoryInstallation.SamusBody);

    private static readonly Dictionary<sbyte, SamusBodyArtworkCatalog> graphicsOffsetFixtures = [];

    private static SamusBodyArtworkCatalog SamusGraphicsOffsetFixture(sbyte offset)
    {
        if (graphicsOffsetFixtures.TryGetValue(offset, out var existing)) return existing;
        var stock = renderingFixtureStock.Value;
        var result = new SamusBodyArtworkCatalog(new SamusBodyArtworkParts { TopPointers = stock.TopSetPointers.ToArray(), BottomPointers = stock.BottomSetPointers.ToArray(), PosePointers = stock.PosePointers.ToArray(), GraphicsYOffsets = Enumerable.Repeat(offset, SamusBodyArtworkCatalog.PoseCount).ToArray(), Frames = stock.Frames.ToArray(), Top = Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => stock.TopSet(index).ToArray()).ToArray(), Bottom = Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => stock.BottomSet(index).ToArray()).ToArray(), Spritemaps = stock.Spritemaps, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTiles = stock.DeathTiles, ArmCannon = stock.ArmCannon, LandingYOffsets = stock.LandingYOffsets.ToArray(), PostureYOffsets = stock.PostureYOffsets.ToArray(), DrainedYOffsets = stock.DrainedYOffsets.ToArray() });
        graphicsOffsetFixtures.Add(offset, result);
        return result;
    }

    /// <summary>Points one pose's <c>$92:D94E</c> tile-animation list pointer at a synthetic record.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one Samus tile-animation list pointer")]
    private static void WriteSamusTileAnimationPointer(TestAddressSpace bus, SamusPoseId pose, ushort pointer) =>
        WriteTestWord(bus, SamusRenderingRomData.TileTransfers.AnimationDefinitionListPointers + (int)pose * 2, pointer);

    /// <summary>Seeds one pose's <c>$92:9263/$92:945D</c> top and bottom spritemap base indices.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one Samus pose's spritemap base indices")]
    private static void WriteSamusSpritemapBases(TestAddressSpace bus, SamusPoseId pose, ushort top, ushort bottom)
    {
        WriteTestWord(bus, SamusSpritemapArtworkCatalog.TopBaseAddress + (int)pose * 2, top);
        WriteTestWord(bus, SamusSpritemapArtworkCatalog.BottomBaseAddress + (int)pose * 2, bottom);
    }
    // Adapt this fixture's sparse, deliberately distinctive artwork to the installed
    // catalog boundary. Unspecified complete DMA groups retain valid stock layout;
    // sprite maps remain the fixture's own compact compositions, including zero pointers.
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture decoding seeded Samus rendering tables into an artwork catalog")]
    private static void BindSyntheticSamusRendering(TestAddressSpace bus, SamusState samus)
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
        var topPointers = OverrideWords(SamusRenderingRomData.TileTransfers.TopDefinitionListPointers, stock.TopSetPointers.ToArray());
        var bottomPointers = OverrideWords(SamusRenderingRomData.TileTransfers.BottomDefinitionListPointers, stock.BottomSetPointers.ToArray());
        var posePointers = OverrideWords(SamusRenderingRomData.TileTransfers.AnimationDefinitionListPointers, stock.PosePointers.ToArray());
        sbyte[] offsets = stock.GraphicsYOffsets.ToArray();
        for (int pose = 0; pose < offsets.Length; pose++)
        {
            int address = SamusMovementRomData.Poses.Definitions + pose * SamusMovementRomData.Poses.DefinitionByteCount + 4;
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
                    int source = Word(address) | bus.ReadByte(address + 2) << 16;
                    ushort first = Word(address + 3), second = Word(address + 5);
                    byte[] bytes = Enumerable.Range(0, first + second).Select(index => bus.ReadByte(source + index)).ToArray();
                    groups[set][position] = new(source, first, second, bytes);
                }
            }
            return groups;
        }
        ushort[] spritePointers = Words(SamusSpritemapArtworkCatalog.PointerTableAddress, SamusSpritemapArtworkCatalog.PointerCount);
        SamusSpritemapDefinition[] maps = spritePointers.Where(pointer => pointer != 0).Distinct().Select(pointer =>
        {
            int address = 0x920000 | pointer;
            var parts = new SamusSpritePart[Word(address)];
            for (int index = 0; index < parts.Length; index++)
            {
                int part = address + 2 + index * 5;
                parts[index] = new(Word(part), bus.ReadByte(part + 2), Word(part + 3));
            }
            return new SamusSpritemapDefinition(pointer, parts);
        }).ToArray();
        var sprites = new SamusSpritemapArtworkCatalog(
            Words(SamusSpritemapArtworkCatalog.TopBaseAddress, SamusBodyArtworkCatalog.PoseCount),
            Words(SamusSpritemapArtworkCatalog.BottomBaseAddress, SamusBodyArtworkCatalog.PoseCount), spritePointers, maps);
        ushort[] landing = stock.LandingYOffsets.ToArray();
        for (int index = 0; index < landing.Length; index++)
            if (bus.HasSeededByte(SamusRenderingRomData.Body.LandingVerticalOffsets + index))
                landing[index] = bus.ReadByte(SamusRenderingRomData.Body.LandingVerticalOffsets + index);
        samus.TileTransfers.BindArtwork(new SamusBodyArtworkCatalog(new SamusBodyArtworkParts { TopPointers = topPointers, BottomPointers = bottomPointers, PosePointers = posePointers, GraphicsYOffsets = offsets, Frames = frames, Top = Groups(true, topPointers), Bottom = Groups(false, bottomPointers), Spritemaps = sprites, Atmosphere = stock.Atmosphere, DeathPalettes = stock.DeathPalettes, DeathTiles = stock.DeathTiles, ArmCannon = stock.ArmCannon, LandingYOffsets = landing, PostureYOffsets = OverrideBytes(SamusRenderingRomData.Body.PostureTransitionVerticalOffsets, stock.PostureYOffsets.ToArray()), DrainedYOffsets = OverrideBytes(SamusRenderingRomData.Body.DrainedVerticalOffsets, stock.DrainedYOffsets.ToArray()) }));
    }
}
