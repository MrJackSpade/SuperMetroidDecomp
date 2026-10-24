using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
namespace SuperMetroid.Verification.Frontend.Ending.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Checks extracted staff-credit rows, playback cadence, and installed text overrides.</summary>
internal static class CreditsPresentationVerification
{
    /// <summary>Compares every native credit row and exercises runtime scrolling and strict JSON validation.</summary>
    internal static void VerifyCreditsPresentation(string romPath)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Ending.CreditsPresentationExtractor.Extract(bus);
        byte[] repeated = SuperMetroid.AssetExtraction.Frontend.Ending.CreditsPresentationExtractor.Extract(bus);
        AssertTrue(extracted.SequenceEqual(repeated),
            "staff-credit extraction is deterministic");

        CreditsPresentation presentation = CreditsPresentation.Load(
            new MemoryStream(extracted, writable: false));
        ushort[][] nativeRows =
            SuperMetroid.AssetExtraction.Frontend.Ending.CreditsPresentationExtractor.ReadNativeRows(bus);
        AssertEqual(CreditsPresentationDefinitions.ExpectedCompiledRows,
            presentation.RowCount, "installed credits retain the fixed native schedule");
        for (int row = 0; row < nativeRows.Length; row++)
        {
            AssertTrue(presentation.GetRow(row).SequenceEqual(nativeRows[row]),
                $"installed credits row {row} matches the native row program");
        }

        // Exercise actual runtime cadence through completion. CreditsObjectState has no bus
        // dependency, so this also proves installed playback cannot fall back to ROM text.
        var runtime = new CreditsObjectState(presentation);
        for (int row = 0; row < presentation.RowCount; row++)
        {
            for (int frame = 0; frame < 15; frame++)
                AssertTrue(!runtime.Step().CopiedRow,
                    $"credits row {row} waits for its sixteenth half-pixel frame");
            CreditsObjectStepResult result = runtime.Step();
            AssertTrue(result.CopiedRow && !result.Finished,
                $"credits row {row} copies at the native eight-pixel boundary");
            int destination = row & 31;
            AssertTrue(runtime.Tilemap.Slice(destination * 32, 32)
                .SequenceEqual(presentation.GetRow(row)),
                $"credits runtime row {row} reaches circular staging");
        }
        for (int frame = 0; frame < 15; frame++) runtime.Step();
        AssertTrue(runtime.Step().Finished && !runtime.Enabled,
            "credits finish on the boundary after the final installed row");

        JsonObject editedDocument = RequiredJson.Parse(extracted).AsObject();
        JsonObject editedLine = editedDocument.Required("lines").Required(0).AsObject();
        editedLine["text"] = "TEST STAFF";
        editedLine["column"] = 3;
        editedLine["palette"] = 6;
        byte[] editedBytes = Encoding.UTF8.GetBytes(
            editedDocument.ToJsonString(ArtworkJson.Options));
        CreditsPresentation edited = CreditsPresentation.Load(
            new MemoryStream(editedBytes, writable: false));
        int firstTextRow = CreditsPresentationDefinitions.InitialBlankRows;
        AssertEqual((ushort)(CreditsPresentationDefinitions.CompileGlyph('T', CreditsLineStyle.Small) | 6 << 10),
            edited.GetRow(firstTextRow)[3],
            "edited text, column and palette compile into the live row");
        AssertTrue(!edited.GetRow(firstTextRow).SequenceEqual(
            presentation.GetRow(firstTextRow)), "staff-credit override changes rendered content");

        Suite(nameof(VerifyBadCreditDocument), () => VerifyBadCreditDocument(extracted, document => document["version"] = 2,
            "unsupported staff-credit schema rejected"));
        Suite(nameof(VerifyBadCreditDocument), () => VerifyBadCreditDocument(extracted, document =>
            document.Required("lines").AsArray().RemoveAt(0), "missing staff-credit line rejected"));
        Suite(nameof(VerifyBadCreditDocument), () => VerifyBadCreditDocument(extracted, document =>
            document.Required("lines").Required(0)["id"] = "wrong", "reordered staff-credit identity rejected"));
        Suite(nameof(VerifyBadCreditDocument), () => VerifyBadCreditDocument(extracted, document =>
            document.Required("lines").Required(0)["text"] = "BAD?", "unsupported staff-credit glyph rejected"));
        Suite(nameof(VerifyBadCreditDocument), () => VerifyBadCreditDocument(extracted, document =>
            document.Required("lines").Required(0)["column"] = 31, "overflowing staff-credit line rejected"));
        Suite(nameof(VerifyBadCreditDocument), () => VerifyBadCreditDocument(extracted, document =>
            document.Required("lines").Required(0)["palette"] = 8, "invalid staff-credit palette rejected"));
        AssertThrows<InvalidDataException>(() => CreditsPresentation.Load(
            new MemoryStream("not json"u8.ToArray())), "corrupt staff-credit JSON rejected");

        Console.WriteLine(
            $"Credits presentation: {nativeRows.Length} native rows, 67 editable lines, " +
            "runtime cadence, overrides and strict validation pass.");
    }

    /// <summary>Mutates a valid credit document and checks that loading rejects the specified invalid case.</summary>
    private static void VerifyBadCreditDocument(byte[] extracted,
        Action<JsonObject> mutate, string identity)
    {
        JsonObject document = RequiredJson.Parse(extracted).AsObject();
        mutate(document);
        byte[] bytes = Encoding.UTF8.GetBytes(
            document.ToJsonString(ArtworkJson.Options));
        AssertThrows<InvalidDataException>(() => CreditsPresentation.Load(
            new MemoryStream(bytes, writable: false)), identity);
    }

    /// <summary>Checks override selection, content identity, stock restoration, and corrupt override rejection.</summary>
    internal static void VerifyStaffCreditsAssets(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original)
    {
        Directory.CreateDirectory(overrideDirectory);
        string stockPath = Path.Combine(
            stockDirectory, CreditsPresentationDefinitions.FileName);
        JsonObject document = RequiredJson.Parse(File.ReadAllBytes(stockPath)).AsObject();
        JsonObject first = document.Required("lines").Required(0).AsObject();
        first["text"] = "EDITED STAFF";
        first["column"] = 2;
        first["palette"] = 7;
        string overridePath = Path.Combine(
            overrideDirectory, CreditsPresentationDefinitions.FileName);
        File.WriteAllText(overridePath,
            document.ToJsonString(ArtworkJson.Options));

        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(
            stockDirectory, overrideDirectory);
        int textRow = CreditsPresentationDefinitions.InitialBlankRows;
        AssertTrue(!edited.StaffCredits.GetRow(textRow).SequenceEqual(
            original.StaffCredits.GetRow(textRow)),
            "staff-credit override reaches the installed catalog");
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "staff-credit override changes catalog identity");

        File.Delete(overridePath);
        AreaMapPresentationCatalog restored = MapPresentationFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(restored.StaffCredits.GetRow(textRow).SequenceEqual(
            original.StaffCredits.GetRow(textRow)),
            "removing staff-credit override restores stock content");

        File.WriteAllText(overridePath, "{ bad credits");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(
            stockDirectory, overrideDirectory),
            "corrupt staff-credit override fails without stock fallback");
    }

    /// <summary>Dispatches the cartridge-backed credits presentation verification command.</summary>
    [VerificationCommand("--credits-presentation")]
    internal static int CreditsPresentationCommand()
    {
        string creditsRom = RepositoryRomPath;
        Suite(nameof(VerifyCreditsPresentation), () => VerifyCreditsPresentation(creditsRom));
        return 0;
    }
}
