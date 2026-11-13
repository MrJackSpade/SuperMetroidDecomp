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
    private static readonly Lazy<SamusBodySources> renderingFixtureStock =
        new(() => InstalledFixtures.SamusBody);

    /// <summary>Caches complete catalogs whose every pose uses a requested graphics Y offset.</summary>
    private static readonly Dictionary<sbyte, SamusBodySources> graphicsOffsetFixtures = [];

    /// <summary>Builds or reuses a stock body catalog with every pose's graphics Y offset replaced.</summary>
    internal static SamusBodySources SamusGraphicsOffsetFixture(sbyte offset)
    {
        if (graphicsOffsetFixtures.TryGetValue(offset, out var existing)) return existing;
        var stock = renderingFixtureStock.Value;
        PlacementTables tables = StockPlacementTables(stock);
        var result = new SamusBodySources(new SamusBodyArtworkParts { TopSetStarts = stock.TopSetStarts.ToArray(), BottomSetStarts = stock.BottomSetStarts.ToArray(), PoseFirstFrames = stock.PoseFirstFrames.ToDictionary(), Placements = SamusBodyPlacementDecoding.Decode(Enumerable.Repeat((int)offset, SamusBodySources.PoseCount).ToArray(), tables.Landing, tables.Posture, tables.Drained), Frames = stock.Frames.ToArray(), Spritemaps = stock.Spritemaps, Atmosphere = stock.Atmosphere, DeathSteps = stock.DeathSteps, ArmCannon = stock.ArmCannon });
        graphicsOffsetFixtures.Add(offset, result);
        return result;
    }

    /// <summary>Points one pose's <c>$92:D94E</c> tile-animation list pointer at a synthetic record.</summary>
    internal static void WriteSamusTileAnimationPointer(TestAddressSpace bus, SamusPoseId pose, ushort pointer) =>
        WriteTestWord(bus, SamusSourceDefinitions.AnimationDefinitionListPointers.Value + SamusPoseNumbers.Number(pose) * 2, pointer);

    /// <summary>Seeds one pose's <c>$92:9263/$92:945D</c> top and bottom spritemap base indices.</summary>
    internal static void WriteSamusSpritemapBases(TestAddressSpace bus, SamusPoseId pose, ushort top, ushort bottom)
    {
        WriteTestWord(bus, SamusSpritemapSources.TopBaseAddress + SamusPoseNumbers.Number(pose) * 2, top);
        WriteTestWord(bus, SamusSpritemapSources.BottomBaseAddress + SamusPoseNumbers.Number(pose) * 2, bottom);
    }
    /// <summary>
    /// Adapts sparse seeded rendering tables to the installed catalog boundary. Unspecified
    /// complete DMA groups retain stock layout; spritemaps use the fixture's own compact
    /// compositions, including zero pointers. The body is built from the seeded bytes once for each state of the bus, so
    /// rebinding over an unchanged bus binds the body already built from it.
    /// </summary>
    internal static void BindSyntheticSamusRendering(TestAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (!syntheticBodies.TryGetValue(bus, out SyntheticBody? built) || built.Version != bus.Version)
        {
            built = new SyntheticBody(bus.Version, BuildSyntheticBody(bus));
            syntheticBodies.AddOrUpdate(bus, built);
        }
        samus.Presentation.BindBodyFixture(built.Artwork);
    }

    /// <summary>The body last built from each bus, with the bus's version it was built at.</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TestAddressSpace, SyntheticBody> syntheticBodies = new();

    /// <summary>A body built from a bus's seeded bytes.</summary>
    /// <param name="Version">The bus's version when the body was built.</param>
    /// <param name="Artwork">The body and the characters it shows.</param>
    private sealed record SyntheticBody(int Version, SamusBodyCharacterArtwork Artwork);

    /// <summary>The body the bus's seeded tables describe over the installed stock body.</summary>
    private static SamusBodyCharacterArtwork BuildSyntheticBody(TestAddressSpace bus)
    {
        SamusBodySources stock = renderingFixtureStock.Value;
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
        int[] OverrideSignedBytes(int address, int[] original)
        {
            for (int index = 0; index < original.Length; index++)
                if (bus.HasSeededByte(address + index)) original[index] = unchecked((sbyte)bus.ReadByte(address + index));
            return original;
        }
        PlacementTables tables = StockPlacementTables(stock);
        var topPointers = OverrideWords(SamusSourceDefinitions.TopDefinitionListPointers.Value, stock.TopSetPointers.ToArray());
        var bottomPointers = OverrideWords(SamusSourceDefinitions.BottomDefinitionListPointers.Value, stock.BottomSetPointers.ToArray());
        // A record no pose names installs no frame list, so its table entry is read by nothing.
        IReadOnlyDictionary<SamusPoseId, ushort> stockPosePointers = stock.PosePointers;
        var posePointers = OverrideWords(SamusSourceDefinitions.AnimationDefinitionListPointers.Value,
            [.. Enumerable.Range(0, SamusBodySources.PoseCount).Select(pose => SamusPoseNumbering.NamesPose(pose) ? stockPosePointers[SamusPoseNumbering.FromNumber(pose)] : (ushort)0)]);
        int[] offsets = tables.GraphicsYOffsets;
        for (int pose = 0; pose < offsets.Length; pose++)
        {
            int address = SamusSourceDefinitions.PoseDefinitions.Value + pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4;
            if (bus.HasSeededByte(address)) offsets[pose] = unchecked((sbyte)bus.ReadByte(address));
        }
        SamusBodyFrameSelection[] frames = stock.Frames.ToArray();
        for (int index = 0; index < frames.Length; index++)
        {
            int address = SamusBodySources.FirstFrameAddress + index * 4;
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
        ushort[] spritePointers = Words(SamusSpritemapSources.PointerTableAddress, SamusSpritemapSources.EntryCount);
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
            SamusSpritemapNative.Bases(Words(SamusSpritemapSources.TopBaseAddress, SamusBodySources.PoseCount)),
            SamusSpritemapNative.Bases(Words(SamusSpritemapSources.BottomBaseAddress, SamusBodySources.PoseCount)), SamusSpritemapNative.Entries(spritePointers), maps);
        int[] landing = tables.Landing;
        for (int index = 0; index < landing.Length; index++)
            if (bus.HasSeededByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + index))
                landing[index] = bus.ReadByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + index);
        return SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(topPointers), BottomSetStarts = SamusBodyNative.Records(bottomPointers), PoseFirstFrames = SamusBodyNative.Frames(posePointers), Placements = SamusBodyPlacementDecoding.Decode(offsets, landing, OverrideSignedBytes(SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value, tables.Posture), OverrideSignedBytes(SamusSourceDefinitions.DrainedVerticalOffsets.Value, tables.Drained)), Frames = frames, Spritemaps = sprites, Atmosphere = stock.Atmosphere, DeathSteps = stock.DeathSteps, ArmCannon = stock.ArmCannon }, Groups(true, topPointers), Groups(false, bottomPointers));
    }

    /// <summary>
    /// The cartridge tables <paramref name="stock"/>'s placements were decoded from (#1517), recovered so a fixture can
    /// override their seeded bytes and decode them again: each pose's origin, and the whole landing, posture-transition and
    /// drained tables, which the first pose of each family installs from its first frame to the table's end.
    /// </summary>
    private static PlacementTables StockPlacementTables(SamusBodySources stock)
    {
        int[] Rises(SamusPoseId pose) => stock.Placements[pose].FrameRises is { } rises
            ? [.. rises]
            : throw new InvalidDataException($"Stock pose {pose} installs no frame placements.");
        return new PlacementTables(
            // A record no pose names is skipped by the decoder, so its table entry is read by nothing.
            [.. Enumerable.Range(0, SamusBodySources.PoseCount).Select(pose => SamusPoseNumbering.NamesPose(pose) ? stock.Placements[SamusPoseNumbering.FromNumber(pose)].GraphicsYOffset : 0)],
            Rises(SamusPoseId.NormalLandingRightPose),
            [.. Rises(SamusPoseId.CrouchingTransitionRightPose).Select(rise => -rise)],
            [.. Rises(SamusPoseId.DrainedCrouchingRightPose).Select(rise => -rise)]);
    }

    /// <summary>The cartridge's body placement tables, as a fixture overrides them.</summary>
    /// <param name="GraphicsYOffsets">Each pose's signed art origin, in the cartridge's pose order.</param>
    /// <param name="Landing">The landing table's unsigned rises.</param>
    /// <param name="Posture">The posture-transition table's signed offsets.</param>
    /// <param name="Drained">The drained-body table's signed offsets.</param>
    private sealed record PlacementTables(int[] GraphicsYOffsets, int[] Landing, int[] Posture, int[] Drained);
}
