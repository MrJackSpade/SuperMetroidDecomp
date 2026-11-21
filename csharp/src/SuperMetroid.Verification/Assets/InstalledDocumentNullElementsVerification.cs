using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Assets;

/// <summary>The probe document's contract, generated at build time as the installed documents' is.</summary>
[JsonSerializable(typeof(InstalledDocumentNullElementsVerification.ElementsProbe))]
internal sealed partial class InstalledDocumentProbeJsonContext : JsonSerializerContext;

/// <summary>
/// Confirms an installed document's collection of present references rejects a JSON null element at load (#2554), naming
/// the document and the member, while a collection of nullable values keeps a null.
/// </summary>
internal static class InstalledDocumentNullElementsVerification
{
    /// <summary>A document whose lists hold present names, one of them nested, and one whose elements may be absent.</summary>
    internal sealed class ElementsProbe
    {
        /// <summary>Names every element of which is present.</summary>
        public required List<string> Names { get; init; }

        /// <summary>Rows of names every element of which is present.</summary>
        public required List<List<string>> Rows { get; init; }

        /// <summary>Counts any of which may be absent.</summary>
        public required List<int?> Optional { get; init; }
    }

    /// <summary>The installed documents' element check over the probe's generated contract.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        TypeInfoResolver = InstalledDocumentElements.Over(InstalledDocumentProbeJsonContext.Default),
        RespectNullableAnnotations = true,
    };

    /// <summary>Checks null elements of collections of references are rejected, by name, and nullable values kept.</summary>
    internal static void VerifyInstalledDocumentNullElements()
    {
        ElementsProbe whole = Read("""{"Names":["a","b"],"Rows":[["c"]],"Optional":[4,null]}""");
        var expected = new ElementsProbe { Names = ["a", "b"], Rows = [["c"]], Optional = [4, null] };
        AssertTrue(whole.Names.SequenceEqual(expected.Names) && whole.Rows.Single().SequenceEqual(expected.Rows.Single())
            && whole.Optional.SequenceEqual(expected.Optional), "a document with every reference element present reads whole");
        AssertTrue(whole.Optional[1] is null, "a collection of nullable values keeps a null element");

        JsonException names = AssertThrows<JsonException>(() => Read("""{"Names":["a",null],"Rows":[],"Optional":[]}"""),
            "a null element of a list of references is rejected at load");
        AssertTrue(names.Message.Contains("ElementsProbe.Names", StringComparison.Ordinal),
            "the rejection names the document and the member");

        JsonException rows = AssertThrows<JsonException>(() => Read("""{"Names":[],"Rows":[["c",null]],"Optional":[]}"""),
            "a null element of a nested list of references is rejected at load");
        AssertTrue(rows.Message.Contains("ElementsProbe.Rows", StringComparison.Ordinal),
            "the nested rejection names the document and the member");
        Console.WriteLine("Installed document null elements: reference elements reject null by document and member; nullable values keep it.");
    }

    /// <summary>Reads <paramref name="json"/> as the probe document.</summary>
    private static ElementsProbe Read(string json) =>
        JsonSerializer.Deserialize<ElementsProbe>(json, Options) ?? throw new InvalidDataException("The probe document read as null.");

    [VerificationCommand("--installed-document-null-elements")]
    internal static int InstalledDocumentNullElementsCommand()
    {
        Suite(nameof(VerifyInstalledDocumentNullElements), VerifyInstalledDocumentNullElements);
        return 0;
    }
}
