using System.Text.Json;
using System.Text.RegularExpressions;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// The installed-format audit of #2564: installed content carries no hardware data. Every JSON document of the shared
/// repository installation, as the installer publishes it with the import data deleted, holds pictures, purpose-named
/// colours written as <c>#rrggbb</c> and placements, never the cartridge's representation of them. Only what the game and
/// its player read is audited; the import's own documents never reach the installation.
/// </summary>
internal static partial class InstalledContentHardwareDataVerification
{
    /// <summary>
    /// Fails, naming each file and property, on any hardware field of an installed JSON document: a colour held as red,
    /// green and blue channel levels; a colour or other value held as a number named as a colour word or a native word; a
    /// palette row (a number named as a palette); a palette index (a number named as an ink); a tile's or sprite's priority
    /// beside a palette row; and a tile word's character index (a number named as a tile or character index).
    /// </summary>
    internal static void VerifyInstalledContentCarriesNoHardwareData()
    {
        string content = Path.GetFullPath(RepositoryInstallation.Installation.ContentDirectory);
        string[] documents = [.. Directory.EnumerateFiles(content, "*.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal)];
        AssertTrue(documents.Length != 0, $"the repository installation {content} holds JSON documents to audit");
        var findings = new List<string>();
        var kinds = new Dictionary<string, FieldName>(StringComparer.Ordinal);
        var path = new List<string>();
        foreach (string file in documents)
        {
            using FileStream stream = File.OpenRead(file);
            using JsonDocument document = JsonDocument.Parse(stream);
            Audit(new AuditedDocument(Path.GetRelativePath(content, file).Replace('\\', '/'), findings, kinds, path), document.RootElement);
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} installed document fields hold hardware data:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Installed content: {documents.Length} installed JSON documents hold no hardware data.");
    }

    /// <summary>Adds each hardware field <paramref name="element"/>, at the document's current path, holds.</summary>
    private static void Audit(AuditedDocument document, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (JsonElement item in element.EnumerateArray())
            {
                document.Path.Add($"[{index++}]");
                Audit(document, item);
                document.Path.RemoveAt(document.Path.Count - 1);
            }
            return;
        }
        if (element.ValueKind != JsonValueKind.Object)
            return;
        int channels = 0;
        bool paletteRow = false;
        foreach (JsonProperty property in element.EnumerateObject())
        {
            FieldName kind = document.KindOf(property.Name);
            if (kind == FieldName.Channel && IsNumbers(property.Value))
                channels++;
            paletteRow |= kind == FieldName.PaletteRow && IsNumbers(property.Value);
        }
        if (channels == ChannelNames.Length)
            document.Report("a colour as red, green and blue channel levels");
        foreach (JsonProperty property in element.EnumerateObject())
        {
            document.Path.Add("." + property.Name);
            if (What(document.KindOf(property.Name), property.Value, paletteRow) is { } what)
                document.Report(what);
            Audit(document, property.Value);
            document.Path.RemoveAt(document.Path.Count - 1);
        }
    }

    /// <summary>The hardware field a property named as <paramref name="kind"/> holding <paramref name="value"/> is, or null for none.</summary>
    private static string? What(FieldName kind, JsonElement value, bool paletteRow)
    {
        if (kind is FieldName.None or FieldName.Channel)
            return null;
        if (kind == FieldName.Priority)
            return paletteRow && (IsNumbers(value) || value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                ? "a tile's or sprite's priority beside its palette row" : null;
        if (!IsNumbers(value))
            return null;
        return kind switch
        {
            FieldName.PaletteRow => "a palette row number",
            FieldName.Ink => "a palette index",
            FieldName.Colour => "a colour as a packed colour word",
            FieldName.Word => "a native word",
            FieldName.TileIndex => "a tile word's character index",
            FieldName.None or FieldName.Channel or FieldName.Priority => null,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined field name kind."),
        };
    }

    /// <summary>What a property's name says it holds, by its camel-case words.</summary>
    private static FieldName Classify(string name)
    {
        if (ChannelNames.Contains(name, StringComparer.Ordinal))
            return FieldName.Channel;
        string[] words = Words(name);
        if (words.Any(word => word is "palette" or "palettes"))
            return FieldName.PaletteRow;
        if (words.Any(word => word is "ink" or "inks"))
            return FieldName.Ink;
        if (words.Contains("priority"))
            return FieldName.Priority;
        if (words.Any(word => word is "colour" or "colours" or "color" or "colors"))
            return FieldName.Colour;
        if (words.Any(word => word is "word" or "words"))
            return FieldName.Word;
        if (words.Contains("index") && words.Any(word => word is "tile" or "character"))
            return FieldName.TileIndex;
        return FieldName.None;
    }

    /// <summary>One installed document being audited: its file, where its findings go, the names classified so far and the current path.</summary>
    /// <param name="File">The document's path in the installation.</param>
    /// <param name="Findings">Every finding of the audit.</param>
    /// <param name="Kinds">Each property name's classification, shared by every document.</param>
    /// <param name="Path">The property and element steps from the document's root to the value being audited.</param>
    private sealed record AuditedDocument(string File, List<string> Findings, Dictionary<string, FieldName> Kinds, List<string> Path)
    {
        /// <summary>What a property named <paramref name="name"/> holds, classified once.</summary>
        internal FieldName KindOf(string name)
        {
            if (!Kinds.TryGetValue(name, out FieldName kind))
                Kinds.Add(name, kind = Classify(name));
            return kind;
        }

        /// <summary>Records that the value at the current path holds <paramref name="what"/>.</summary>
        internal void Report(string what) => Findings.Add($"{File} ${string.Concat(Path)}: {what}");
    }

    /// <summary>What a property's name says it holds.</summary>
    private enum FieldName
    {
        /// <summary>Nothing of the hardware's.</summary>
        None,
        /// <summary>One of a colour's red, green and blue channels.</summary>
        Channel,
        /// <summary>A palette row, named as a palette.</summary>
        PaletteRow,
        /// <summary>A palette index, named as an ink.</summary>
        Ink,
        /// <summary>A tile's or sprite's priority.</summary>
        Priority,
        /// <summary>A colour, which installed documents write as text.</summary>
        Colour,
        /// <summary>A native word.</summary>
        Word,
        /// <summary>A tile word's character index.</summary>
        TileIndex,
    }

    /// <summary>Whether <paramref name="value"/> is a number or an array, at any depth, of numbers alone.</summary>
    private static bool IsNumbers(JsonElement value) =>
        value.ValueKind == JsonValueKind.Number ||
        (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() != 0 && value.EnumerateArray().All(IsNumbers));

    /// <summary>The lowercase camel-case words of <paramref name="name"/>.</summary>
    private static string[] Words(string name) =>
        [.. CamelWords().Matches(name).Select(match => match.Value.ToLowerInvariant())];

    /// <summary>The colour channels a colour held as levels names.</summary>
    private static readonly string[] ChannelNames = ["red", "green", "blue"];

    /// <summary>Splits an identifier into its camel-case words.</summary>
    [GeneratedRegex("[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex CamelWords();

    /// <summary>Runs the installed-format audit over the shared repository installation.</summary>
    [VerificationCommand("--installed-content-hardware-data")]
    internal static int InstalledContentHardwareDataCommand()
    {
        Suite(nameof(VerifyInstalledContentCarriesNoHardwareData), VerifyInstalledContentCarriesNoHardwareData);
        return 0;
    }
}
