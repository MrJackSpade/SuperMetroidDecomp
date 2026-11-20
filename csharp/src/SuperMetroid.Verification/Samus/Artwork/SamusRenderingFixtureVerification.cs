using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Builds Samus rendering catalogs from distinctive synthetic tables over the retail cartridge: the import's own decoding
/// reads the fixture's cartridge, so a fixture body is decoded exactly as the installed one is (#2556).
/// </summary>
internal static class SamusRenderingFixtureVerification
{
    /// <summary>The retail cartridge a fixture's seeded bytes lie over, read once.</summary>
    private static readonly Lazy<IImportCartridgeSource> retail = new(() =>
        CartridgeImportSource.Require(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Caches complete catalogs whose every pose uses a requested graphics Y offset.</summary>
    private static readonly Dictionary<sbyte, SamusBodySources> graphicsOffsetFixtures = [];

    /// <summary>Builds or reuses a stock body catalog with every pose's graphics Y offset replaced.</summary>
    internal static SamusBodySources SamusGraphicsOffsetFixture(sbyte offset)
    {
        if (graphicsOffsetFixtures.TryGetValue(offset, out var existing)) return existing;
        int first = SamusSourceDefinitions.PoseDefinitions.Value, size = SamusSourceDefinitions.PoseDefinitionByteCount;
        SamusBodySources result = SamusBodySheetFixtures.Register(Decoded(address =>
            address >= first && address < first + SamusBodyTableDecoding.PoseCount * size && (address - first) % size == 4
                ? unchecked((byte)offset) : null));
        graphicsOffsetFixtures.Add(offset, result);
        return result;
    }

    /// <summary>Points one pose's <c>$92:D94E</c> tile-animation list pointer at a synthetic record.</summary>
    internal static void WriteSamusTileAnimationPointer(TestAddressSpace bus, SamusPoseId pose, ushort pointer) =>
        WriteTestWord(bus, SamusSourceDefinitions.AnimationDefinitionListPointers.Value + SamusPoseNumbers.Number(pose) * 2, pointer);

    /// <summary>Seeds one pose's <c>$92:9263/$92:945D</c> top and bottom spritemap base indices.</summary>
    internal static void WriteSamusSpritemapBases(TestAddressSpace bus, SamusPoseId pose, ushort top, ushort bottom)
    {
        WriteTestWord(bus, SamusSourceDefinitions.SpritemapTopBases.Value + SamusPoseNumbers.Number(pose) * 2, top);
        WriteTestWord(bus, SamusSourceDefinitions.SpritemapBottomBases.Value + SamusPoseNumbers.Number(pose) * 2, bottom);
    }

    /// <summary>
    /// Decodes the body the bus's seeded bytes describe over the retail cartridge and binds it. Unseeded definition groups,
    /// selectors and placements keep the retail bytes; the spritemap bases and pointer table are the fixture's own, read as
    /// zero where unseeded, so spritemaps use the fixture's own compact compositions, including zero pointers. The body is
    /// built from the seeded bytes once for each state of the bus, so rebinding over an unchanged bus binds the body already
    /// built from it.
    /// </summary>
    internal static void BindSyntheticSamusRendering(TestAddressSpace bus, SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        if (!syntheticBodies.TryGetValue(bus, out SyntheticBody? built) || built.Version != bus.Version)
        {
            built = new SyntheticBody(bus.Version, Decoded(address => OwnSpritemapTable(address) || bus.HasSeededByte(address)
                ? bus.ReadByte(address) : null));
            syntheticBodies.AddOrUpdate(bus, built);
        }
        samus.BindBodyFixture(built.Artwork);
    }

    /// <summary>The body last built from each bus, with the bus's version it was built at.</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TestAddressSpace, SyntheticBody> syntheticBodies = new();

    /// <summary>A body built from a bus's seeded bytes.</summary>
    /// <param name="Version">The bus's version when the body was built.</param>
    /// <param name="Artwork">The body and the characters it shows.</param>
    private sealed record SyntheticBody(int Version, SamusBodyCharacterArtwork Artwork);

    /// <summary>Whether <paramref name="address"/> lies in the spritemap bases or pointer table, which a fixture owns whole.</summary>
    private static bool OwnSpritemapTable(int address) =>
        Within(address, SamusSourceDefinitions.SpritemapPointers.Value, 2096 * 2) ||
        Within(address, SamusSourceDefinitions.SpritemapTopBases.Value, SamusBodyTableDecoding.PoseCount * 2) ||
        Within(address, SamusSourceDefinitions.SpritemapBottomBases.Value, SamusBodyTableDecoding.PoseCount * 2);

    /// <summary>Whether <paramref name="address"/> lies in the <paramref name="length"/> bytes from <paramref name="start"/>.</summary>
    private static bool Within(int address, int start, int length) => address >= start && address < start + length;

    /// <summary>
    /// The body the import decodes from the retail cartridge with <paramref name="seeded"/>'s bytes over it, and the stock
    /// atmosphere, death steps and arm cannon.
    /// </summary>
    private static SamusBodyCharacterArtwork Decoded(Func<int, byte?> seeded)
    {
        SamusBodySources stock = InstalledFixtures.SamusBody;
        SamusBodyDecoded decoded = SamusBodyTableDecoding.Decode(new FixtureCartridge(seeded),
            InstalledFixtures.SamusShownFrameAnalysis.Installed(), out IReadOnlyDictionary<SamusBodyPicture, SamusBodyTileTransfer> pictures);
        return SamusBodyCharacterArtwork.Create(decoded.Parts(stock.Atmosphere, stock.DeathSteps, stock.ArmCannon), pictures);
    }

    /// <summary>The retail cartridge with a fixture's bytes over it.</summary>
    /// <param name="seeded">The fixture's byte at an address, or none where the retail byte shows.</param>
    private sealed class FixtureCartridge(Func<int, byte?> seeded) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <inheritdoc/>
        public byte ReadCartridgeByte(int cpuAddress) => seeded(cpuAddress) ?? retail.Value.ReadCartridgeByte(cpuAddress);
    }
}
