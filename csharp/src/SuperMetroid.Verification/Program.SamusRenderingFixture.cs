using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Lazily loaded installed artwork that supplies valid defaults for unseeded synthetic rendering data.</summary>
    private static readonly Lazy<SamusBodyArtworkCatalog> renderingFixtureStock =
        new(() => RepositoryInstallation.SamusBody);

    /// <summary>Caches stock-derived artwork catalogs with one uniform pose graphics offset per fixture.</summary>
    private static readonly Dictionary<sbyte, SamusBodyArtworkCatalog> graphicsOffsetFixtures = [];

    /// <summary>Builds or reuses a stock catalog whose pose graphics offsets all use the requested fixture value.</summary>
    /// <param name="offset">Signed graphics Y offset assigned to every pose in the returned catalog.</param>
    /// <returns>A cached catalog containing the requested offset and otherwise copied stock artwork data.</returns>
    private static SamusBodyArtworkCatalog SamusGraphicsOffsetFixture(sbyte offset)
    {
        if (graphicsOffsetFixtures.TryGetValue(offset, out var existing)) return existing;
        var stock = renderingFixtureStock.Value;
        var result = new SamusBodyArtworkCatalog(stock.TopSetPointers.ToArray(),
            stock.BottomSetPointers.ToArray(), stock.PosePointers.ToArray(),
            Enumerable.Repeat(offset, SamusBodyArtworkCatalog.PoseCount).ToArray(),
            stock.Frames.ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => stock.TopSet(index).ToArray()).ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => stock.BottomSet(index).ToArray()).ToArray(),
            stock.Spritemaps, stock.Atmosphere, stock.DeathPalettes, stock.DeathTiles,
            stock.ArmCannon, stock.LandingYOffsets.ToArray(), stock.PostureYOffsets.ToArray(),
            stock.DrainedYOffsets.ToArray());
        graphicsOffsetFixtures.Add(offset, result);
        return result;
    }
    // Adapt this fixture's sparse, deliberately distinctive artwork to the installed
    // catalog boundary. Unspecified complete DMA groups retain valid stock layout;
    // sprite maps remain the fixture's own compact compositions, including zero pointers.
    /// <summary>Binds synthetic Samus artwork assembled from seeded bus bytes, using installed assets for unspecified data.</summary>
    /// <param name="bus">Fixture memory whose seeded ROM addresses override selected rendering definitions and assets.</param>
    /// <param name="samus">Samus instance whose tile-transfer system receives the assembled artwork catalog.</param>
    private static void BindSyntheticSamusRendering(TestAddressSpace bus, SamusState samus)
    {
        SamusBodyArtworkCatalog stock = renderingFixtureStock.Value;
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
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
        samus.TileTransfers.BindArtwork(new SamusBodyArtworkCatalog(topPointers, bottomPointers, posePointers, offsets,
            frames, Groups(true, topPointers), Groups(false, bottomPointers), sprites,
            stock.Atmosphere, stock.DeathPalettes, stock.DeathTiles, stock.ArmCannon, landing,
            OverrideBytes(SamusRenderingRomData.Body.PostureTransitionVerticalOffsets, stock.PostureYOffsets.ToArray()),
            OverrideBytes(SamusRenderingRomData.Body.DrainedVerticalOffsets, stock.DrainedYOffsets.ToArray())));
    }
}
