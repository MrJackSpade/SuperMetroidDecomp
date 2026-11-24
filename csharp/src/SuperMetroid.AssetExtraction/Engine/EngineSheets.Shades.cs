using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
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
    /// Cuts the frames of an enemy's sheets with the frame of a big boss's body of the same name laid over each: a
    /// boss the old game drew as sprites over a body, each frame of the two named alike, cut as the one picture a frame
    /// of it is. They are cut on one canvas, with the boss's place at the same point of each, as <see cref="Cut"/> does.
    /// </summary>
    /// <param name="tree">The tree the pictures are written to.</param>
    /// <param name="content">The installation's content directory, which holds the list of the bosses' bodies.</param>
    /// <param name="folder">The folder the pictures are written under.</param>
    /// <param name="owner">The boss, as the installation names its body.</param>
    /// <param name="sheets">The enemy's sheets.</param>
    /// <exception cref="InvalidDataException">The installation has no such body or sheet.</exception>
    internal EngineSheetPictures CutWithBossBody(EngineAssetTree tree, string content, string folder, string owner, params ReadOnlySpan<string> sheets)
    {
        const string bodyFamily = GameInstallationLayout.BossBodySheetDirectoryName;
        string list = Path.Combine(content, bodyFamily, BossBodySheetFiles.ManifestFileName);
        JsonObject bodyFrames = JsonNode.Parse(File.ReadAllText(list))?["sheets"]?[owner]?["frames"]?.AsObject()
            ?? throw new InvalidDataException($"The list of the bosses' bodies {list} has no body of {owner}.");
        string bodySheet = SheetNamed(bodyFamily, "BossBody." + owner);
        InstalledSheetDocument bodyDocument = _documents[(bodyFamily, bodySheet)];
        var documents = new List<(string Sheet, InstalledSheetDocument Document)>();
        foreach (string sheet in sheets)
        {
            documents.Add((sheet, _documents.TryGetValue((EnemyFamily, sheet), out InstalledSheetDocument? found)
                ? found
                : throw new InvalidDataException($"The installation has no sheet {sheet} in {EnemyFamily}.")));
        }

        // Where the body's frame of a name lies from the boss's place, and its frame on the body's sheet.
        (int X, int Y, InstalledSheetFrameDocument Frame)? BodyOf(string name)
        {
            if (bodyFrames[name] is not JsonObject listed)
                return null;
            string picture = listed["picture"]!.GetValue<string>();
            InstalledSheetFrameDocument frame = bodyDocument.Frames.TryGetValue(picture, out InstalledSheetFrameDocument? found)
                ? found
                : throw new InvalidDataException($"The sheet {bodySheet} has no frame {picture}.");
            return (listed["anchor"]!["x"]!.GetValue<int>() - frame.Origin.X, listed["anchor"]!["y"]!.GetValue<int>() - frame.Origin.Y, frame);
        }

        int left = 0, top = 0, right = 1, bottom = 1;
        foreach ((string name, InstalledSheetFrameDocument frame) in documents.SelectMany(sheet => sheet.Document.Frames))
        {
            left = Math.Min(left, -frame.Origin.X);
            top = Math.Min(top, -frame.Origin.Y);
            right = Math.Max(right, frame.Size.X - frame.Origin.X);
            bottom = Math.Max(bottom, frame.Size.Y - frame.Origin.Y);
            if (BodyOf(name) is { } body)
            {
                left = Math.Min(left, body.X);
                top = Math.Min(top, body.Y);
                right = Math.Max(right, body.X + body.Frame.Size.X);
                bottom = Math.Max(bottom, body.Y + body.Frame.Size.Y);
            }
        }

        int width = right - left, height = bottom - top;
        var paths = new Dictionary<string, string>(StringComparer.Ordinal);
        var canvas = new RgbaPixel[width * height];
        SpriteImagePixels bodyImage = _store.Assemble(bodySheet, bodyDocument);
        foreach ((string sheet, InstalledSheetDocument document) in documents)
        {
            SpriteImagePixels image = _store.Assemble(sheet, document);
            foreach ((string name, InstalledSheetFrameDocument frame) in document.Frames)
            {
                if (paths.ContainsKey(name))
                    continue;
                (int X, int Y, InstalledSheetFrameDocument Frame)? over = BodyOf(name);
                if (frame.Picture is null && over is not { Frame.Picture: not null })
                {
                    paths[name] = "";
                    continue;
                }

                Array.Clear(canvas);
                if (frame.Picture is not null)
                {
                    int across = -frame.Origin.X - left, down = -frame.Origin.Y - top;
                    for (int y = 0; y < frame.Size.Y; y++)
                    {
                        image.Pixels.AsSpan((frame.Corner.Y + y) * document.Size.X + frame.Corner.X, frame.Size.X)
                            .CopyTo(canvas.AsSpan((down + y) * width + across, frame.Size.X));
                    }
                }

                if (over is { Frame.Picture: not null } body)
                {
                    // The body lies over the sprites: only where it shows something does it cover them.
                    for (int y = 0; y < body.Frame.Size.Y; y++)
                    {
                        int from = (body.Frame.Corner.Y + y) * bodyDocument.Size.X + body.Frame.Corner.X, to = (body.Y - top + y) * width + body.X - left;
                        for (int x = 0; x < body.Frame.Size.X; x++)
                        {
                            if (bodyImage.Pixels[from + x].A != 0)
                                canvas[to + x] = bodyImage.Pixels[from + x];
                        }
                    }
                }

                string path = $"{folder}/{FileName(name)}.png";
                tree.WritePicture(path, width, height, canvas);
                paths[name] = path;
            }

            foreach (string blank in document.Blank ?? [])
                paths.TryAdd(blank, "");
        }

        return new EngineSheetPictures(left, top, width, height, paths);
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
