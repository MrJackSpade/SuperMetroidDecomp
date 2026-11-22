using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>Checks file-select presentation names, ordering, and exact JSON schema identity.</summary>
internal static class FileSelectNameCasesVerification
{
    /// <summary>Checks page names for main, copy, and clear file-select screens.</summary>
    internal static void VerifyFileSelectPageNames(FileSelectExtraction source) => VerifyFileSelectNames(source, "Pages",
        ["Background", "Main.WithData", "Main.Empty", "Copy.Source", "Copy.Destination",
         "Copy.Confirm", "Copy.Completed", "Clear.Selection", "Clear.Confirm", "Clear.Completed"],
        [FileSelectPresentationFormat.BackgroundPageName,
            .. FileSelectPresentationFormat.Pages.Select(FileSelectParts.PageName)]);

    /// <summary>Checks names for energy, empty-slot, and time-colon patches.</summary>
    internal static void VerifyFileSelectPatchNames(FileSelectExtraction source) => VerifyFileSelectNames(source, "Patches",
        ["Energy", "NoData", "TimeColon"],
        [.. FileSelectPresentationFormat.Patches.Select(FileSelectParts.PatchName)]);

    /// <summary>Checks border anchor names for the three file-select flows.</summary>
    internal static void VerifyFileSelectBorderNames(FileSelectExtraction source) => VerifyFileSelectNames(source, "BorderAnchors",
        ["Main", "Copy", "Clear"],
        [.. FileSelectPresentationFormat.Borders.Select(FileSelectPresentationFormat.BorderName)]);

    /// <summary>Checks copy and clear confirmation anchors that depend on selected slots.</summary>
    internal static void VerifyFileSelectDynamicAnchorNames(FileSelectExtraction source) => VerifyFileSelectNames(source, "DynamicAnchors",
        ["Copy.Destination.Source", "Copy.Confirm.Source", "Copy.Confirm.Destination", "Clear.Confirm.Source"],
        [.. FileSelectPresentationFormat.Anchors.Select(FileSelectPresentationFormat.AnchorName)]);

    /// <summary>Checks border, cursor, and helmet sprite identities in the installed schema.</summary>
    internal static void VerifyFileSelectSpriteNames(FileSelectExtraction source) => VerifyFileSelectNames(source, "Sprites",
        ["Border.Main", "Border.Copy", "Border.Clear", "Cursor.FinUp", "Cursor.FinsUpAndDown", "Cursor.FinDown", "Cursor.FinsFlat",
         "Helmet.TurnedAway", "Helmet.Turning", "Helmet.ShineLeft", "Helmet.ShineLeftOfMiddle", "Helmet.ShineRightOfMiddle",
         "Helmet.ShineRight", "Helmet.SparkleTall", "Helmet.SparkleWide"],
        [.. FileSelectPresentationFormat.SpriteNames]);

    /// <summary>Checks exact names, then rejects missing, extra, or case-changed JSON keys.</summary>
    private static void VerifyFileSelectNames(FileSelectExtraction source, string field, string[] expected,
        string[] names)
    {
        // Independent literal identities preserve the original project-owned JSON schema.
        AssertTrue(names.SequenceEqual(expected), $"{field} complete original identity order");

        // The pages, patches and sprites are the import-data artwork's; the anchors are the installed layout's (#1638).
        bool artwork = field is "Pages" or "Patches" or "Sprites";
        var root = RequiredJson.Parse(artwork ? source.Artwork : source.Layout).AsObject();
        var values = (root.Single(pair => pair.Key.Equals(field, StringComparison.OrdinalIgnoreCase)).Value ?? throw new InvalidOperationException("Verification requires the field's value.")).AsObject();
        AssertTrue(values.Select(pair => pair.Key).ToHashSet(StringComparer.Ordinal).SetEquals(expected),
            $"{field} independent extractor supplies precisely the required identities");
        _ = FileSelectSourcePresentationConstants.Load(source);
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
            AssertThrows<InvalidDataException>(() => FileSelectSourcePresentationConstants.Load(artwork ? source with { Artwork = bytes } : source with { Layout = bytes }),
                $"{field} rejects {reason}");
        }
    }
}
