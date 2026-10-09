using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Verifies the stable identities and ordering of file-select page definitions in the extracted presentation data.</summary>
    /// <param name="source">Serialized file-select presentation document to validate.</param>
    private static void VerifyFileSelectPageNames(byte[] source) => VerifyFileSelectNames(source, "Pages",
        ["Background", "Main.WithData", "Main.Empty", "Copy.Source", "Copy.Destination",
         "Copy.Confirm", "Copy.Completed", "Clear.Selection", "Clear.Confirm", "Clear.Completed"],
        FileSelectPresentationDefinitions.PageNames, FileSelectPresentationDefinitions.PageName);

    /// <summary>Verifies the stable identities and ordering of file-select patch labels.</summary>
    /// <param name="source">Serialized file-select presentation document to validate.</param>
    private static void VerifyFileSelectPatchNames(byte[] source) => VerifyFileSelectNames(source, "Patches",
        ["Energy", "NoData", "TimeColon"],
        FileSelectPresentationDefinitions.PatchNames, FileSelectPresentationDefinitions.PatchName);

    /// <summary>Verifies the stable identities and ordering of file-select border anchors.</summary>
    /// <param name="source">Serialized file-select presentation document to validate.</param>
    private static void VerifyFileSelectBorderNames(byte[] source) => VerifyFileSelectNames(source, "BorderAnchors",
        ["Main", "Copy", "Clear"],
        FileSelectPresentationDefinitions.BorderNames, FileSelectPresentationDefinitions.BorderName);

    /// <summary>Verifies the stable identities and ordering of dynamic anchors used by file-select transitions.</summary>
    /// <param name="source">Serialized file-select presentation document to validate.</param>
    private static void VerifyFileSelectDynamicAnchorNames(byte[] source) => VerifyFileSelectNames(source, "DynamicAnchors",
        ["Copy.Destination.Source", "Copy.Confirm.Source", "Copy.Confirm.Destination", "Clear.Confirm.Source"],
        FileSelectPresentationDefinitions.DynamicAnchorNames, FileSelectPresentationDefinitions.DynamicAnchorName);

    /// <summary>Verifies the stable identities and ordering of file-select sprite definitions.</summary>
    /// <param name="source">Serialized file-select presentation document to validate.</param>
    private static void VerifyFileSelectSpriteNames(byte[] source) => VerifyFileSelectNames(source, "Sprites",
        ["Border.Main", "Border.Copy", "Border.Clear", "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3",
         "Helmet.0", "Helmet.1", "Helmet.2", "Helmet.3", "Helmet.4", "Helmet.5", "Helmet.6", "Helmet.7"],
        FileSelectPresentationDefinitions.SpriteNames, FileSelectPresentationDefinitions.SpriteName);

    /// <summary>Checks that a named JSON collection matches its required identities, order, ordinal lookup, and strict rejection rules.</summary>
    /// <param name="source">Serialized file-select presentation document under test.</param>
    /// <param name="field">Collection property whose names and entries are being validated.</param>
    /// <param name="expected">Independent required name sequence used to check both enumeration and lookup.</param>
    /// <param name="names">Names exposed by the compiled file-select definitions.</param>
    /// <param name="nameAt">Ordinal lookup used to confirm each name and out-of-range behavior.</param>
    /// <remarks>The method also verifies that extraction supplies exactly the required set and that loading rejects missing, extra, replaced, or case-changed identities.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">An invalid ordinal is not rejected by <paramref name="nameAt"/>.</exception>
    /// <exception cref="InvalidDataException">A malformed identity set is accepted by the presentation loader.</exception>
    private static void VerifyFileSelectNames(byte[] source, string field, string[] expected,
        IEnumerable<string> names, Func<int, string> nameAt)
    {
        // Independent literal identities preserve the original project-owned JSON schema.
        AssertTrue(names.SequenceEqual(expected), $"{field} complete original identity order");
        AssertTrue(names.SequenceEqual(expected), $"{field} enumeration is repeatable");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], nameAt(index), $"{field} ordinal identity");
        foreach (int invalid in new[] { int.MinValue, -1, expected.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => nameAt(invalid), $"{field} invalid ordinal");

        var root = JsonNode.Parse(source)!.AsObject();
        var values = root.Single(pair => pair.Key.Equals(field, StringComparison.OrdinalIgnoreCase)).Value!.AsObject();
        AssertTrue(values.Select(pair => pair.Key).ToHashSet(StringComparer.Ordinal).SetEquals(expected),
            $"{field} independent extractor supplies precisely the required identities");
        _ = FileSelectPresentation.Load(new MemoryStream(source));
        foreach (string name in expected)
        {
            JsonNode original = values[name]!;
            values.Remove(name);
            Reject("missing required identity");
            values.Add("Unexpected.Identity", original);
            Reject("same-count replacement identity");
            values.Remove("Unexpected.Identity");
            values.Add(name.ToLowerInvariant(), original);
            Reject("case-changed identity");
            values.Remove(name.ToLowerInvariant());
            values.Add(name, original);
        }
        values.Add("Unexpected.Identity", values[expected[0]]!.DeepClone());
        Reject("additional identity");

        void Reject(string reason)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(root.ToJsonString());
            AssertThrows<InvalidDataException>(() => FileSelectPresentation.Load(new MemoryStream(bytes)),
                $"{field} rejects {reason}");
        }
    }
}
