using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.RenderVerification;

/// <summary>Conspicuous, valid disk edits used only by the paired Samus isolation fixtures.</summary>
internal static class SamusIsolationArtworkEdits
{
    /// <summary>Writes valid body-frame, offset, spritemap, and death-palette overrides while retaining native frame identities and timing.</summary>
    internal static void WriteBody(GameInstallation installation)
    {
        Edit(SamusBodyArtworkFiles.ManifestFileName, document =>
        {
            Shift(document.Required("graphicsYOffsets"), 3, signed: true);
            Shift(document.Required("landingYOffsets"), 3, signed: false);
            Shift(document.Required("postureYOffsets"), 3, signed: true);
            Shift(document.Required("drainedYOffsets"), 3, signed: true);
            foreach (JsonNode? map in document.Required("spritemaps").AsArray().RequiredElements())
            foreach (JsonNode? part in map.Required("parts").AsArray().RequiredElements())
                part["x"] = unchecked((ushort)(part.Required("x").GetValue<int>() + 2));
            // Keep the native frame-list identities and animation-frame count. Replace
            // only each valid DMA character selection, not the compiled delay program.
            foreach (JsonNode? frame in document.Required("frames").AsArray().RequiredElements())
            {
                if (frame.Required("topSet").GetValue<int>() < SamusBodyArtworkCatalog.TopSetCount)
                    frame["topPosition"] = 0;
                if (frame.Required("bottomSet").GetValue<int>() < SamusBodyArtworkCatalog.BottomSetCount)
                    frame["bottomPosition"] = 0;
            }
        });
        Edit(SamusDeathPaletteArtworkFiles.ArtworkFileName, document =>
        {
            RecolorWords(document.Required("suited"));
            RecolorWords(document.Required("suitless"));
            RecolorWords(document.Required("whiteout"));
            JsonArray selectors = document.Required("explosionPaletteIndices").AsArray();
            for (int index = 0; index < selectors.Count; index++)
                selectors[index] = (selectors.Required(index).GetValue<int>() + 1) % SamusPaletteRomData.Death.PaletteCount;
        });

        void Edit(string name, Action<JsonNode> change)
        {
            JsonNode document = RequiredJson.Parse(File.ReadAllBytes(
                Path.Combine(installation.SamusBodyDirectory, name)));
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
        JsonNode document = RequiredJson.Parse(File.ReadAllBytes(stockPath));
        RecolorRgb(document);
        Directory.CreateDirectory(fixture.MapOverrideDirectory);
        string editedPath = Path.Combine(fixture.MapOverrideDirectory, name);
        File.WriteAllText(editedPath, document.ToJsonString());
        using var editedStream = File.OpenRead(editedPath);
        return (stock, compile(editedStream));
    }

    /// <summary>Offsets every JSON entry using the source field's signed or unsigned byte representation.</summary>
    private static void Shift(JsonNode node, int amount, bool signed)
    {
        JsonArray values = node.AsArray();
        for (int index = 0; index < values.Count; index++)
        {
            int value = values.Required(index).GetValue<int>() + amount;
            values[index] = signed ? unchecked((sbyte)value) : unchecked((byte)value);
        }
    }

    /// <summary>Inverts every RGB5 color word recursively while preserving the document shape.</summary>
    private static void RecolorWords(JsonNode node)
    {
        JsonArray values = node.AsArray();
        for (int index = 0; index < values.Count; index++)
            if (values[index] is JsonArray array) RecolorWords(array);
            else values[index] = values.Required(index).GetValue<int>() ^ 0x7fff;
    }

    /// <summary>Inverts RGB5 channels in nested color objects without changing other JSON fields.</summary>
    private static void RecolorRgb(JsonNode node)
    {
        if (node is JsonObject record)
        {
            if (record.ContainsKey("red") && record.ContainsKey("green") && record.ContainsKey("blue"))
            {
                foreach (string channel in new[] { "red", "green", "blue" })
                    record[channel] = 31 - record.Required(channel).GetValue<int>();
            }
            else foreach (JsonNode? child in record.Select(entry => entry.Value).ToArray())
                if (child is not null) RecolorRgb(child);
        }
        else if (node is JsonArray array)
            foreach (JsonNode? child in array) if (child is not null) RecolorRgb(child);
    }
}
