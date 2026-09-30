using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

/// <summary>Conspicuous, valid disk edits used only by the paired Samus isolation fixtures.</summary>
internal static class SamusIsolationArtworkEdits
{
    internal static void WriteBody(GameInstallation installation)
    {
        Edit(SamusBodyArtworkFiles.ManifestFileName, document =>
        {
            Shift(document["graphicsYOffsets"]!, 3, signed: true);
            Shift(document["landingYOffsets"]!, 3, signed: false);
            Shift(document["postureYOffsets"]!, 3, signed: true);
            Shift(document["drainedYOffsets"]!, 3, signed: true);
            foreach (JsonNode? map in document["spritemaps"]!.AsArray())
            foreach (JsonNode? part in map!["parts"]!.AsArray())
                part!["x"] = (part["x"]!.GetValue<int>() + 2) & 0xffff;
            // Keep the native frame-list identities and animation-frame count. Replace
            // only each valid DMA character selection, not the compiled delay program.
            foreach (JsonNode? frame in document["frames"]!.AsArray())
            {
                if (frame!["topSet"]!.GetValue<int>() < SamusBodyArtworkCatalog.TopSetCount)
                    frame["topPosition"] = 0;
                if (frame["bottomSet"]!.GetValue<int>() < SamusBodyArtworkCatalog.BottomSetCount)
                    frame["bottomPosition"] = 0;
            }
        });
        Edit(SamusDeathPaletteArtworkFiles.ArtworkFileName, document =>
        {
            RecolorWords(document["suited"]!);
            RecolorWords(document["suitless"]!);
            RecolorWords(document["whiteout"]!);
            JsonArray selectors = document["explosionPaletteIndices"]!.AsArray();
            for (int index = 0; index < selectors.Count; index++)
                selectors[index] = (selectors[index]!.GetValue<int>() + 1) % SamusPaletteRomData.Death.PaletteCount;
        });

        void Edit(string name, Action<JsonNode> change)
        {
            JsonNode document = JsonNode.Parse(File.ReadAllBytes(
                Path.Combine(installation.SamusBodyDirectory, name)))!;
            change(document);
            File.WriteAllText(Path.Combine(installation.SamusBodyOverrideDirectory, name),
                document.ToJsonString());
        }
    }

    /// <summary>Reloads both the stock and a real edited JSON file through the production compiler.</summary>
    internal static (T Stock, T Edited) Colors<T>(GameInstallation source,
        GameInstallation fixture, string name, Func<Stream, T> compile)
    {
        string stockPath = Path.Combine(source.MapDirectory, name);
        using var stockStream = File.OpenRead(stockPath);
        T stock = compile(stockStream);
        JsonNode document = JsonNode.Parse(File.ReadAllBytes(stockPath))!;
        RecolorRgb(document);
        Directory.CreateDirectory(fixture.MapOverrideDirectory);
        string editedPath = Path.Combine(fixture.MapOverrideDirectory, name);
        File.WriteAllText(editedPath, document.ToJsonString());
        using var editedStream = File.OpenRead(editedPath);
        return (stock, compile(editedStream));
    }

    private static void Shift(JsonNode node, int amount, bool signed)
    {
        JsonArray values = node.AsArray();
        for (int index = 0; index < values.Count; index++)
        {
            int value = values[index]!.GetValue<int>() + amount;
            values[index] = signed ? unchecked((sbyte)value) : unchecked((byte)value);
        }
    }

    private static void RecolorWords(JsonNode node)
    {
        JsonArray values = node.AsArray();
        for (int index = 0; index < values.Count; index++)
            if (values[index] is JsonArray array) RecolorWords(array);
            else values[index] = values[index]!.GetValue<int>() ^ 0x7fff;
    }

    private static void RecolorRgb(JsonNode node)
    {
        if (node is JsonObject record)
        {
            if (record.ContainsKey("red") && record.ContainsKey("green") && record.ContainsKey("blue"))
            {
                foreach (string channel in new[] { "red", "green", "blue" })
                    record[channel] = 31 - record[channel]!.GetValue<int>();
            }
            else foreach (JsonNode? child in record.Select(entry => entry.Value).ToArray())
                if (child is not null) RecolorRgb(child);
        }
        else if (node is JsonArray array)
            foreach (JsonNode? child in array) if (child is not null) RecolorRgb(child);
    }
}
