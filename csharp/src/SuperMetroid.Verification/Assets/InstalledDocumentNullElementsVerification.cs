using System.Text.Json;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms an installed document's collection whose elements it declares present rejects a JSON null element at load
/// (#2554), naming the document and the member, while a collection that declares its elements nullable keeps a null.
/// </summary>
internal static class InstalledDocumentNullElementsVerification
{
    /// <summary>A document whose lists declare their elements present, one of them nested, and one whose elements may be absent.</summary>
    private sealed class ElementsProbe
    {
        /// <summary>Names every element of which is present.</summary>
        public required List<string> Names { get; init; }

        /// <summary>Rows of names every element of which is present.</summary>
        public required List<List<string>> Rows { get; init; }

        /// <summary>Names any of which may be absent.</summary>
        public required List<string?> Optional { get; init; }
    }

    /// <summary>The installed documents' resolver over the serializer's own options.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        TypeInfoResolver = InstalledDocumentElements.Resolver,
        RespectNullableAnnotations = true,
    };

    /// <summary>Checks null elements of declared-present collections are rejected, by name, and nullable ones kept.</summary>
    internal static void VerifyInstalledDocumentNullElements()
    {
        ElementsProbe whole = Read("""{"Names":["a","b"],"Rows":[["c"]],"Optional":["d",null]}""");
        AssertEqual(2, whole.Names.Count, "a document with every declared-present element reads whole");
        AssertTrue(whole.Optional[1] is null, "a collection declaring its elements nullable keeps a null element");

        JsonException names = AssertThrows<JsonException>(() => Read("""{"Names":["a",null],"Rows":[],"Optional":[]}"""),
            "a null element of a declared-present list is rejected at load");
        AssertTrue(names.Message.Contains("ElementsProbe.Names", StringComparison.Ordinal),
            "the rejection names the document and the member");

        JsonException rows = AssertThrows<JsonException>(() => Read("""{"Names":[],"Rows":[["c",null]],"Optional":[]}"""),
            "a null element of a nested declared-present list is rejected at load");
        AssertTrue(rows.Message.Contains("ElementsProbe.Rows", StringComparison.Ordinal),
            "the nested rejection names the document and the member");
        Console.WriteLine("Installed document null elements: declared-present elements reject null by document and member; nullable elements keep it.");
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
