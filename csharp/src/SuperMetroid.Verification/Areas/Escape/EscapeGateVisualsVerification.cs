using SuperMetroid.AssetExtraction;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateStockVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks Mother Brain escape gate visuals against PLM collision and install contracts.</summary>
internal static class EscapeGateVisualsVerification
{
    /// <summary>Checks the stock frames, catalog rejection and the installed stock.</summary>
    internal static void VerifyEscapeGateVisuals(SuperMetroidAddressSpace rom)
    {
        RoomObjectVisualEntry[] entries =
            RoomObjectEscapeGateVisualFiles.All
                .OrderBy(draw => RoomObjectDrawAddressCodec.Of(draw.Draw))
                .Select(draw => new RoomObjectVisualEntry(
                    RoomObjectEscapeGateVisualFiles.VisualId(draw.Draw),
                    draw.Runs.Span[0].Blocks.Span
                        .SelectToArray(block => block.Appearance)))
                .ToArray();
        AssertEqual(entries.Single(entry => entry.Id == "closed").Blocks[0],
            RoomObjectEscapeGateVisualCatalog.Stock().GetAppearance(
                RoomObjectDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
            "stock escape-gate catalog retains cartridge appearance");

        AssertThrows<InvalidDataException>(
            () => RoomObjectEscapeGateVisualFiles.Create(entries.Skip(1)),
            "escape-gate catalog rejects a missing frame");
        Suite(nameof(VerifyEscapeGateVisualInstallation), () => VerifyEscapeGateVisualInstallation(rom));
        Console.WriteLine(
            "Escape-gate visuals: the stock frames, the installed stock and invalid content checks pass.");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyEscapeGateVisualInstallation(SuperMetroidAddressSpace rom)
    {
        // A private installation copy, whose stock file is tampered below.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectEscapeGateVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectEscapeGateVisualFilesConstants.ValidateStock(
            installation.RoomObjectEscapeGateVisualDirectory);
        VerifyEscapeGateStockMapping(rom, installation.LoadRoomObjectEscapeGateVisuals());
        string stockPath = Path.Combine(
            installation.RoomObjectEscapeGateVisualDirectory,
            RoomObjectEscapeGateVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => RoomObjectEscapeGateVisualFilesConstants.ValidateStock(
                installation.RoomObjectEscapeGateVisualDirectory),
            "tampered escape-gate stock fails loudly where it is read");
    }

}
