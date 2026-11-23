using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms an installed document's collection of present references rejects a JSON null element at load (#2554), naming
/// the document and the member, as the game reads documents a player edited in place: the Power Bomb glow's colours, a
/// list of colours, and the boss bodies' dissolve maps, whose rows are lists of runs.
/// </summary>
internal static class InstalledDocumentNullElementsVerification
{
    /// <summary>Checks null elements of installed collections of references, nested or not, are rejected by name.</summary>
    internal static void VerifyInstalledDocumentNullElements()
    {
        // The Power Bomb glow's colours, as the import writes them and as a player who blanks one of them leaves them.
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] glow = PowerBombGlowColorExtractor.Extract(rom);
        _ = PowerBombGlowColorFile.Read(new MemoryStream(glow));
        AssertRejected(() => PowerBombGlowColorFile.Read(new MemoryStream(Edited(glow, document => document["preExplosion"]!.AsArray()[1] = null))),
            "PowerBombGlowColorDocument.preExplosion", "a null element of a list of references");

        // Dissolve maps of a two-by-two picture no pixel of which dissolves: each row is one run of two kept pixels.
        byte[] maps = BossBodyDissolveMapFile.Encode([new("probe", new PictureDissolve(new(2, 2), [.. Enumerable.Repeat<DissolvingPixel?>(null, 4)]))]);
        AssertEqual(new Extent16<ImagePixels>(2, 2), BossBodyDissolveMapFile.Decode(maps, "probe")["probe"].Size,
            "a document with every reference element present reads whole");
        AssertRejected(() => BossBodyDissolveMapFile.Decode(Edited(maps, document => document["maps"]!["probe"]!["rows"]!.AsArray()[1] = null), "probe"),
            "MapDocument.rows", "a null row of a list of rows");
        AssertRejected(() => BossBodyDissolveMapFile.Decode(Edited(maps, document => document["maps"]!["probe"]!["rows"]![1]!.AsArray()[0] = null), "probe"),
            "MapDocument.rows", "a null run of a row nested in the list of rows");
        Console.WriteLine("Installed document null elements: reference elements, nested or not, reject null by document and member.");
    }

    /// <summary>The installed document <paramref name="json"/> after a player's <paramref name="edit"/>.</summary>
    private static byte[] Edited(byte[] json, Action<JsonNode> edit)
    {
        JsonNode document = JsonNode.Parse(json) ?? throw new InvalidDataException("The installed document read as null.");
        edit(document);
        return System.Text.Encoding.UTF8.GetBytes(document.ToJsonString());
    }

    /// <summary>
    /// Confirms <paramref name="read"/> rejects the document as invalid, its serializer's rejection naming
    /// <paramref name="member"/>.
    /// </summary>
    private static void AssertRejected(Action read, string member, string what)
    {
        InvalidDataException rejected = AssertThrows<InvalidDataException>(read, $"{what} is rejected at load");
        AssertTrue(rejected.InnerException is JsonException inner && inner.Message.Contains(member, StringComparison.Ordinal),
            $"{what}'s rejection names the document and the member");
    }

    [VerificationCommand("--installed-document-null-elements")]
    internal static int InstalledDocumentNullElementsCommand()
    {
        Suite(nameof(VerifyInstalledDocumentNullElements), VerifyInstalledDocumentNullElements);
        return 0;
    }
}
