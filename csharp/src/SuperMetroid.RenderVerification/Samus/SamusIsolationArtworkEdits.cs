using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.RenderVerification.Samus;

/// <summary>Conspicuous, valid disk edits used only by the paired Samus isolation fixtures.</summary>
internal static class SamusIsolationArtworkEdits
{
    /// <summary>Writes valid body-frame, offset, spritemap, and death-palette overrides while retaining native frame identities and timing.</summary>
    internal static void WriteBody(GameInstallation installation)
    {
        // Every name is the installed manifest's own member, so a change to its format fails to compile here.
        Edit(SamusBodyArtworkFiles.ManifestFileName, document =>
        {
            Shift(document.Required(Json(nameof(SamusBodyArtworkFiles.Manifest.GraphicsYOffsets))), 3, signed: true);
            Shift(document.Required(Json(nameof(SamusBodyArtworkFiles.Manifest.LandingYOffsets))), 3, signed: false);
            Shift(document.Required(Json(nameof(SamusBodyArtworkFiles.Manifest.PostureYOffsets))), 3, signed: true);
            Shift(document.Required(Json(nameof(SamusBodyArtworkFiles.Manifest.DrainedYOffsets))), 3, signed: true);
            foreach (JsonNode? map in document.Required(Json(nameof(SamusBodyArtworkFiles.Manifest.Spritemaps))).AsArray().RequiredElements())
            foreach (JsonNode? part in map.AsArray().RequiredElements())
                part[Json(nameof(SpriteVisualPart.OffsetX))] = part.Required(Json(nameof(SpriteVisualPart.OffsetX))).GetValue<int>() + 2;
            // Keep the native frame-list identities and animation-frame count. Replace
            // only each valid DMA character selection, not the compiled delay program.
            foreach (JsonNode? frame in document.Required(Json(nameof(SamusBodyArtworkFiles.Manifest.Frames))).AsArray().RequiredElements())
            {
                if (frame.Required(Json(nameof(SamusBodyFrameSelection.TopSet))).GetValue<int>() < SamusBodySources.TopSetCount)
                    frame[Json(nameof(SamusBodyFrameSelection.TopPosition))] = 0;
                if (frame.Required(Json(nameof(SamusBodyFrameSelection.BottomSet))).GetValue<int>() < SamusBodySources.BottomSetCount)
                    frame[Json(nameof(SamusBodyFrameSelection.BottomPosition))] = 0;
            }
        });
        Edit(SamusDeathPaletteArtworkFiles.ArtworkFileName, document =>
        {
            RecolorWords(document.Required("suited"));
            RecolorWords(document.Required("suitless"));
            RecolorWords(document.Required("whiteout"));
            JsonArray selectors = document.Required("explosionPaletteIndices").AsArray();
            for (int index = 0; index < selectors.Count; index++)
                selectors[index] = (selectors.Required(index).GetValue<int>() + 1) % SamusPaletteDefinitions.Death.PaletteCount;
        });

        // The manifests serialize their members in camel case.
        static string Json(string member) => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(member);

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
    internal static StockEditedPair<T> Colors<T>(GameInstallation source,
        GameInstallation fixture, string name, Func<Stream, T> compile)
    {
        string stockPath = Path.Combine(source.MapDirectory, name);
        using var stockStream = File.OpenRead(stockPath);
        T stock = compile(stockStream);
        JsonNode document = RequiredJson.Parse(File.ReadAllBytes(stockPath));
        RecolorRgb(document);
        // The edited document is compiled directly from a file beside the fixture's installation; nothing loads it.
        string editedDirectory = Directory.CreateDirectory(Path.Combine(fixture.Root, "edited-colors")).FullName;
        string editedPath = Path.Combine(editedDirectory, name);
        File.WriteAllText(editedPath, document.ToJsonString());
        using var editedStream = File.OpenRead(editedPath);
        return new StockEditedPair<T>(stock, compile(editedStream));
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
