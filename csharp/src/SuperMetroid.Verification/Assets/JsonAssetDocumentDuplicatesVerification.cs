using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms installed-document duplicate checking treats an escaped property name as the name it spells: the serializer
/// escapes '+' as + in frame names, and those names are indexed by their decoded text rather than compared against
/// every other name, so a duplicate spelled once plainly and once escaped is still rejected.
/// </summary>
internal static class JsonAssetDocumentDuplicatesVerification
{
    /// <summary>The serializer's own options for exact and case-folded property names.</summary>
    private static readonly JsonSerializerOptions Exact = new(), CaseFolded = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks escaped/plain duplicate rejection, distinct escaped names and a large escaped frame table.</summary>
    internal static void VerifyJsonAssetDocumentDuplicates()
    {
        AssertThrows<InvalidDataException>(() => Read(Exact, """{"frames":{"a+b":1,"a+b":2}}"""),
            "an escaped spelling of a plain name is a duplicate");
        AssertThrows<InvalidDataException>(() => Read(Exact, """{"frames":{"a+b":1,"a+b":2}}"""),
            "two escaped spellings of one name are duplicates");
        AssertThrows<InvalidDataException>(() => Read(CaseFolded, """{"frames":{"A+b":1,"a+B":2}}"""),
            "case-folded escaped and plain spellings are duplicates");
        AssertEqual(2, Read(Exact, """{"frames":{"a+b":1,"a+c":2}}""")["frames"].Count,
            "an escaped name that spells a different name is distinct");
        AssertEqual(2, Read(Exact, """{"frames":{"A+b":1,"a+B":2}}""")["frames"].Count,
            "exact names differing only in case are distinct");

        // A frame table the size of a room block sheet's: every name escaped, every name distinct.
        var table = new StringBuilder("""{"frames":{""");
        for (int frame = 0; frame < 20000; frame++)
            table.Append(frame == 0 ? "" : ",").Append("\"tileset characters Ceres\\u002B").Append(frame).Append("\":").Append(frame);
        table.Append("}}");
        Dictionary<string, Dictionary<string, int>> frames = Read(Exact, table.ToString());
        AssertEqual(20000, frames["frames"].Count, "a large table of distinct escaped names reads whole");
        AssertEqual(19999, frames["frames"]["tileset characters Ceres+19999"], "escaped names decode to their spelling");
        Console.WriteLine("JSON asset duplicates: escaped and plain spellings collide, distinct escaped names and a 20000-name escaped table pass.");
    }

    /// <summary>Reads a nested name table through the installed-document reader.</summary>
    private static Dictionary<string, Dictionary<string, int>> Read(JsonSerializerOptions options, string json) =>
        JsonAssetDocument.Read<Dictionary<string, Dictionary<string, int>>>(Encoding.UTF8.GetBytes(json), options);

    [VerificationCommand("--json-asset-duplicates")]
    internal static int JsonAssetDocumentDuplicatesCommand()
    {
        Suite(nameof(VerifyJsonAssetDocumentDuplicates), VerifyJsonAssetDocumentDuplicates);
        return 0;
    }
}
