using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareProductionVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFlarePresentationVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrapplePointAnimationVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleTileArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Compares editable charge-flare sprite compositions with native OAM output and production rendering.</summary>
internal static class ChargeFlareCompositionsVerification
{
    /// <summary>Checks extracted parts across selector, position, and OAM capacity cases, then validates visual edits and malformed assets.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyChargeFlareCompositions(SuperMetroidAddressSpace bus,
        bool flareOnly = false, string sourceRom = "Super Metroid.smc")
    {
        byte[] json = ChargeFlareSpriteExtractor.Extract(bus);
        var stock = ChargeFlareSpriteCatalog.Load(new MemoryStream(json));
        var allEdited = RequiredJson.Parse(json);
        foreach (var frame in allEdited.Required("frames").AsObject())
        foreach (var part in (frame.Value ?? throw new InvalidOperationException("Verification requires frame.Value.")).AsArray().RequiredElements())
            part["offsetX"] = part.Required("offsetX").GetValue<int>() + 7;
        // The retail ROM's installation is shared and only read here; another ROM installs its own.
        using var temporaryDirectory = new TestScratchDirectory("map-catalog");
        GameInstallation installation = Path.GetFullPath(sourceRom) == Path.GetFullPath("Super Metroid.smc")
            ? RepositoryInstallation.Installation
            : GameAssetInstaller.Install(Path.GetFullPath(sourceRom), temporaryDirectory.Root);
        SamusBodyArtworkCatalog body = installation.LoadSamusBodyArt();
        var roomAssets = new MapPresentationInstalledRoomAssets(installation);
        var maps = installation.LoadMaps();
        Suite(nameof(VerifyChargeFlareProduction), () => VerifyChargeFlareProduction(bus, stock, Load(allEdited), body,
            roomAssets, maps));
        Suite(nameof(VerifyGrappleFlarePresentation), () => VerifyGrappleFlarePresentation(bus, stock, Load(allEdited), body,
            roomAssets, maps, installation.LoadProjectiles().GrappleTiles));
        if (!flareOnly)
        {
            VerifyGrapplePointAnimation(bus);
            VerifyGrappleTileArtwork(bus);
        }
        int cases = 0;
        for (ushort selector = 0; selector < ChargeFlareSpriteDefinitions.Selectors.Count; selector++)
        {
            AssertEqual(ReadNativeChargeFlareSelector(bus, selector),
                ProjectileSpriteNative.Address(ChargeFlareSpriteDefinitions.Selectors[selector]), "Compiled charge-flare selector matches pinned cartridge");
            foreach (ushort x in new ushort[] { 0, 1, 127, 255, 256, 511, 65535 })
            foreach (ushort y in new ushort[] { 0, 1, 127, 255, 256, 511, 65535 })
            foreach (int occupied in new[] { 0, 126, 127 })
            {
                var native = new OamBuffer(); var actual = new OamBuffer();
                for (int i = 0; i < occupied; i++)
                {
                    native.AddProjectileSpritePart(default, 0, default, new Position16<OamPixels>(10, 20));
                    actual.AddProjectileSpritePart(default, 0, default, new Position16<OamPixels>(10, 20));
                }
                DrawImportedFlareSpritemap(bus, native, selector, new(x, y));
                stock.Draw(selector, actual, new(x, y));
                AssertTrue(native.ImageEquals(actual),
                    "Extracted flare matches complete native OAM, including signed placement, high bits and capacity wrap");
                AssertEqual(native.NextByteOffset, actual.NextByteOffset, "Flare composition retains native OAM cursor");
                cases++;
            }
        }
        var document = RequiredJson.Parse(json);
        string first = ChargeFlareSpriteDefinitions.Selectors[0].Name;
        int oldX = document.Required("frames").Required(first).Required(0).Required("offsetX").GetValue<int>();
        document.Required("frames").Required(first).Required(0)["offsetX"] = oldX + 7;
        var edited = Load(document);
        var before = new OamBuffer(); var after = new OamBuffer();
        stock.Draw(0, before, new(100, 100)); edited.Draw(0, after, new(100, 100));
        AssertEqual((before.GetEntry(0).Position.X + 7) & 511, after.GetEntry(0).Position.X, "Composition edit changes emitted flare position");
        AssertEqual(before.GetEntry(0).Position.Y, after.GetEntry(0).Position.Y, "X-only composition edit preserves Y");
        document.Required("frames").AsObject().Remove(first);
        AssertThrows<InvalidDataException>(() => Load(document), "Missing charge-flare composition rejected");
        document = JsonNode.Parse(json); (document ?? throw new InvalidOperationException("Verification requires document."))["damage"] = 1;
        AssertThrows<InvalidDataException>(() => Load(document), "Charge-flare composition rejects mechanics fields");
        document = JsonNode.Parse(json); (document ?? throw new InvalidOperationException("Verification requires document.")).Required("frames").Required(first).Required(0)["palette"] = 8;
        AssertThrows<InvalidDataException>(() => Load(document), "Invalid flare palette rejected");
        AssertThrows<InvalidDataException>(() => ChargeFlareSpriteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))), "Duplicate flare JSON rejected");
        AssertThrows<InvalidDataException>(() => stock.Draw(54, new OamBuffer(), new(0, 0)), "Non-charge selector is not silently substituted");
        AssertThrows<InvalidDataException>(() => stock.Draw(ushort.MaxValue, new OamBuffer(), new(0, 0)), "Invalid selector is not clamped");
        Console.WriteLine($"Charge-flare compositions: {cases} native OAM comparisons, editable displacement and invalid-resource checks pass.");
        static ChargeFlareSpriteCatalog Load(JsonNode node) => ChargeFlareSpriteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(node.ToJsonString())));
    }

    /// <summary>Import-only oracle for the bank-$93 flare selector and part records.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the imported charge-flare spritemap drawer")]
    internal static void DrawImportedFlareSpritemap(SuperMetroidAddressSpace bus, OamBuffer oam,
        ushort selector, Position16<OamPixels> origin)
    {
        int table = SnesAddressMath.AddWithinBank(0x93a1a1, selector * 2);
        int record = 0x930000 | CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(table);
        ushort partCount = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(record);
        int partAddress = SnesAddressMath.AddWithinBank(record, 2);
        for (int part = 0; part < partCount; part++)
        {
            oam.AddProjectileSpritePart(new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(partAddress)),
                bus.ReadCartridgeByte(SnesAddressMath.AddWithinBank(partAddress, 2)),
                new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(SnesAddressMath.AddWithinBank(partAddress, 3))),
                origin);
            partAddress = SnesAddressMath.AddWithinBank(partAddress, 5);
        }
    }

    /// <summary>Reads one native pointer in the bank-$93 charge-flare selector table for comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native charge-flare selector table")]
    private static ushort ReadNativeChargeFlareSelector(ISnesAddressSpace bus, int selector) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            ChargeFlareSpriteDefinitions.SelectorTable + selector * 2);

    /// <summary>Runs focused charge-flare composition checks against the repository ROM.</summary>
    [VerificationCommand("--charge-flare-compositions")]
    internal static int ChargeFlareCompositionsCommand()
    {
        string flareRom = RepositoryRomPath;
        Suite(nameof(VerifyChargeFlareCompositions), () => VerifyChargeFlareCompositions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(flareRom)),
            flareOnly: true, sourceRom: flareRom));
        return 0;
    }
}
