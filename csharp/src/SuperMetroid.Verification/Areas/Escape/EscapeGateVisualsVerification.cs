using SuperMetroid.AssetExtraction;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateStockVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks Mother Brain escape gate visuals against PLM collision and install contracts.</summary>
internal static class EscapeGateVisualsVerification
{
    /// <summary>Checks the stock frames, catalog rejection and the installed stock.</summary>
    internal static void VerifyEscapeGateVisuals(SuperMetroidAddressSpace rom)
    {
        RoomPlmVisualEntry[] entries =
            RoomPlmEscapeGateVisualFiles.All
                .OrderBy(draw => PlmDrawAddressCodec.Of(draw.Draw))
                .Select(draw => new RoomPlmVisualEntry(
                    RoomPlmEscapeGateVisualFiles.VisualId(draw.Draw),
                    draw.Runs.Span[0].Blocks.Span
                        .SelectToArray(block => block.Appearance)))
                .ToArray();
        AssertEqual(entries.Single(entry => entry.Id == "closed").Blocks[0],
            RoomPlmEscapeGateVisualCatalog.Stock().GetAppearance(
                PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
            "stock escape-gate catalog retains cartridge appearance");

        AssertThrows<InvalidDataException>(
            () => RoomPlmEscapeGateVisualFiles.Create(entries.Skip(1)),
            "escape-gate catalog rejects a missing frame");
        Suite(nameof(VerifyEscapeGateVisualInstallation), () => VerifyEscapeGateVisualInstallation(rom));
        Console.WriteLine(
            "Escape-gate visuals: the stock frames, the installed stock and invalid content checks pass.");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyEscapeGateVisualInstallation(SuperMetroidAddressSpace rom)
    {
        // A private installation copy, whose stock file is tampered below.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmEscapeGateVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomPlmEscapeGateVisualFilesConstants.ValidateStock(
            installation.RoomPlmEscapeGateVisualDirectory);
        VerifyEscapeGateStockMapping(rom, installation.LoadRoomPlmEscapeGateVisuals());
        string stockPath = Path.Combine(
            installation.RoomPlmEscapeGateVisualDirectory,
            RoomPlmEscapeGateVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => RoomPlmEscapeGateVisualFilesConstants.ValidateStock(
                installation.RoomPlmEscapeGateVisualDirectory),
            "tampered escape-gate stock fails loudly where it is read");
    }

}
