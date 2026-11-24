using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Pictures in one of their sheet's shades, and the pictures of a big boss's body.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>
    /// Cuts every frame of several sheets together, as <see cref="Cut"/> does, as one of their shades shows them:
    /// the parts of one thing in its other colours. The pictures are of the size, and lie where, the sheets' own do.
    /// </summary>
    /// <exception cref="InvalidDataException">A sheet is not there, or has no such shade.</exception>
    internal EngineSheetPictures CutShadeOf(EngineAssetTree tree, string family, string folder, string shade, params ReadOnlySpan<string> sheets)
    {
        var frames = new List<(string Name, string Sheet)>();
        foreach (string sheet in sheets)
        {
            InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
                ? found
                : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
            frames.AddRange(document.Frames.Keys.Select(name => (name, sheet)));
        }

        return CutFrames(tree, family, folder, frames, shade);
    }

    /// <summary>
    /// Cuts the frames of a big boss's body, each a picture of its own size, under <paramref name="folder"/>; in one
    /// of the body sheet's shades where one is named. The installation says where each frame lies from the boss's
    /// place, and each picture comes back with that.
    /// </summary>
    /// <param name="tree">The tree the pictures are written to.</param>
    /// <param name="content">The installation's content directory, which holds the list of the bosses' bodies.</param>
    /// <param name="folder">The folder the pictures are written under.</param>
    /// <param name="owner">The boss, as the installation names its body.</param>
    /// <param name="shade">The shade the pictures are cut in, or none for the sheet's own colours.</param>
    /// <exception cref="InvalidDataException">The installation has no such body, or its sheet no such shade.</exception>
    internal IReadOnlyDictionary<string, EngineBodyFrame> CutBossBody(EngineAssetTree tree, string content, string folder, string owner, string? shade = null)
    {
        const string family = GameInstallationLayout.BossBodySheetDirectoryName;
        string list = Path.Combine(content, family, BossBodySheetFiles.ManifestFileName);
        JsonObject frames = JsonNode.Parse(File.ReadAllText(list))?["sheets"]?[owner]?["frames"]?.AsObject()
            ?? throw new InvalidDataException($"The list of the bosses' bodies {list} has no body of {owner}.");
        string sheet = SheetNamed(family, "BossBody." + owner);
        var cut = new Dictionary<string, EngineBodyFrame>(StringComparer.Ordinal);
        foreach ((string name, JsonNode? frame) in frames)
        {
            string picture = frame!["picture"]!.GetValue<string>();
            EngineSheetPictures one = CutFrames(tree, family, folder, [(picture, sheet)], shade);
            cut[name] = new EngineBodyFrame(one.Of(picture), frame["anchor"]!["x"]!.GetValue<int>() + one.OffsetX, frame["anchor"]!["y"]!.GetValue<int>() + one.OffsetY);
        }

        return cut;
    }

    /// <summary>
    /// Cuts named pictures of what an enemy spawns as one of their sheets' shades shows them, under
    /// <paramref name="folder"/>. Given the same pictures as <see cref="CutSpawned"/>, they are of the same size and
    /// lie in the same place as the ones it cuts.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture is on no sheet, or its sheet has no such shade.</exception>
    internal EngineSheetPictures CutSpawnedShade(EngineAssetTree tree, string folder, string spawner, IEnumerable<string> frames, string shade)
    {
        var placed = new List<(string Name, string Sheet)>();
        foreach (string frame in frames.Distinct(StringComparer.Ordinal))
            placed.Add((frame, SheetHolding(spawner, frame)));
        return CutFrames(tree, EnemyFamily, folder, placed, shade);
    }
}

/// <summary>One frame of a big boss's body as it was cut.</summary>
/// <param name="Picture">Its picture.</param>
/// <param name="OffsetX">Where the picture's left edge lies from the boss's place.</param>
/// <param name="OffsetY">Where the picture's top edge lies from the boss's place.</param>
internal sealed partial record EngineBodyFrame(string Picture, int OffsetX, int OffsetY);
