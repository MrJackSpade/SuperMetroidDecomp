using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;

namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>Checks file-select presentation names, ordering, and exact JSON schema identity.</summary>
internal static class FileSelectNameCasesVerification
{
    /// <summary>Checks page names for main, copy, and clear file-select screens.</summary>
    internal static void VerifyFileSelectPageNames(byte[] source) => VerifyFileSelectNames(source, "Pages",
        ["Background", "Main.WithData", "Main.Empty", "Copy.Source", "Copy.Destination",
         "Copy.Confirm", "Copy.Completed", "Clear.Selection", "Clear.Confirm", "Clear.Completed"],
        FileSelectPresentationDefinitions.PageNames, FileSelectPresentationDefinitions.PageName);

    /// <summary>Checks names for energy, empty-slot, and time-colon patches.</summary>
    internal static void VerifyFileSelectPatchNames(byte[] source) => VerifyFileSelectNames(source, "Patches",
        ["Energy", "NoData", "TimeColon"],
        FileSelectPresentationDefinitions.PatchNames, FileSelectPresentationDefinitions.PatchName);

    /// <summary>Checks border anchor names for the three file-select flows.</summary>
    internal static void VerifyFileSelectBorderNames(byte[] source) => VerifyFileSelectNames(source, "BorderAnchors",
        ["Main", "Copy", "Clear"],
        FileSelectPresentationDefinitions.BorderNames, FileSelectPresentationDefinitions.BorderName);

    /// <summary>Checks copy and clear confirmation anchors that depend on selected slots.</summary>
    internal static void VerifyFileSelectDynamicAnchorNames(byte[] source) => VerifyFileSelectNames(source, "DynamicAnchors",
        ["Copy.Destination.Source", "Copy.Confirm.Source", "Copy.Confirm.Destination", "Clear.Confirm.Source"],
        FileSelectPresentationDefinitions.DynamicAnchorNames, FileSelectPresentationDefinitions.DynamicAnchorName);

    /// <summary>Checks border, cursor, and helmet sprite identities in the installed schema.</summary>
    internal static void VerifyFileSelectSpriteNames(byte[] source) => VerifyFileSelectNames(source, "Sprites",
        ["Border.Main", "Border.Copy", "Border.Clear", "Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3",
         "Helmet.0", "Helmet.1", "Helmet.2", "Helmet.3", "Helmet.4", "Helmet.5", "Helmet.6", "Helmet.7"],
        FileSelectPresentationDefinitions.SpriteNames, FileSelectPresentationDefinitions.SpriteName);

    /// <summary>Checks exact names and ordinal lookup, then rejects missing, extra, or case-changed JSON keys.</summary>
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

        var root = RequiredJson.Parse(source).AsObject();
        var values = (root.Single(pair => pair.Key.Equals(field, StringComparison.OrdinalIgnoreCase)).Value ?? throw new InvalidOperationException("Verification requires the field's value.")).AsObject();
        AssertTrue(values.Select(pair => pair.Key).ToHashSet(StringComparer.Ordinal).SetEquals(expected),
            $"{field} independent extractor supplies precisely the required identities");
        _ = FileSelectPresentation.Load(new MemoryStream(source));
        foreach (string name in expected)
        {
            JsonNode original = values.Required(name);
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
        values.Add("Unexpected.Identity", values.Required(expected[0]).DeepClone());
        Reject("additional identity");

        void Reject(string reason)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(root.ToJsonString());
            AssertThrows<InvalidDataException>(() => FileSelectPresentation.Load(new MemoryStream(bytes)),
                $"{field} rejects {reason}");
        }
    }
}
