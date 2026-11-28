using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The pictures of a big boss's body.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>
    /// Cuts the frames of a big boss's body, each a picture of its own size, under <paramref name="folder"/>, in the
    /// body sheet's own colours. The installation says where each frame lies from the boss's place, and each picture
    /// comes back with that.
    /// </summary>
    /// <param name="tree">The tree the pictures are written to.</param>
    /// <param name="content">The installation's content directory, which holds the list of the bosses' bodies.</param>
    /// <param name="folder">The folder the pictures are written under.</param>
    /// <param name="owner">The boss, as the installation names its body.</param>
    /// <exception cref="InvalidDataException">The installation has no such body.</exception>
    internal IReadOnlyDictionary<string, EngineBodyFrame> CutBossBody(EngineAssetTree tree, string content, string folder, string owner)
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
            EngineSheetPictures one = CutFrames(tree, family, folder, [(picture, sheet)]);
            cut[name] = new EngineBodyFrame(one.Of(picture), frame["anchor"]!["x"]!.GetValue<int>() + one.OffsetX, frame["anchor"]!["y"]!.GetValue<int>() + one.OffsetY);
        }

        return cut;
    }

    /// <summary>
    /// Cuts the frames of an enemy's sheets with the frame of a big boss's body of the same name: a boss the old game
    /// drew as sprites with a body, each frame of the two named alike, cut as the one picture a frame of it is. The body
    /// lies over a frame's sprites, but under those the old game draws in front of every tile, which lie in front of
    /// it as they lie in front of the room (#3748). The sprites are shown as <paramref name="spriteShade"/> of their
    /// sheets shows them: what the old game draws in a row whose colours it loads for that boss alone, as Crocomire's
    /// skeleton's arm (#3752); a shade changes only the colours of the regions it names, so parts drawn in another row
    /// keep their own. They are cut on one canvas, with the boss's place at the same point of each, as <see cref="Cut"/> does.
    /// </summary>
    /// <param name="tree">The tree the pictures are written to.</param>
    /// <param name="content">The installation's content directory, which holds the list of the bosses' bodies.</param>
    /// <param name="folder">The folder the pictures are written under.</param>
    /// <param name="owner">The boss, as the installation names its body.</param>
    /// <param name="spriteShade">The shade of the enemy's sheets its sprites are shown in.</param>
    /// <param name="sheets">The enemy's sheets.</param>
    /// <exception cref="InvalidDataException">The installation has no such body or sheet, or a sheet lacks the shade.</exception>
    internal EngineSheetPictures CutWithBossBody(
        EngineAssetTree tree, string content, string folder, string owner, string spriteShade, params ReadOnlySpan<string> sheets) =>
        CutBossBodyLayers(tree, content, folder, owner, null, spriteShade, sheets.ToArray());

    /// <summary>
    /// As <see cref="CutWithBossBody"/>, for an enemy whose sheets keep a part of each frame apart, to be drawn behind
    /// the rest: the frame named as the frame is with <paramref name="behind"/> before it. That part is laid down
    /// first, the frame's own sprites over it and the body over both, and it is no picture of its own.
    /// </summary>
    internal EngineSheetPictures CutWithBossBodyAndBehind(
        EngineAssetTree tree, string content, string folder, string owner, string behind, params ReadOnlySpan<string> sheets) =>
        CutBossBodyLayers(tree, content, folder, owner, behind, null, sheets.ToArray());

    /// <summary>Cuts the frames of an enemy's sheets with what lies behind and over each, the sprites in <paramref name="spriteShade"/> where one is given.</summary>
    private EngineSheetPictures CutBossBodyLayers(
        EngineAssetTree tree, string content, string folder, string owner, string? behind, string? spriteShade, string[] sheets)
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
            Dictionary<(int Colour, int Region), RgbaPixel>? shaded = spriteShade is null ? null : ShadeOf(sheet, document, spriteShade);
            foreach ((string name, InstalledSheetFrameDocument frame) in document.Frames)
            {
                if (paths.ContainsKey(name))
                    continue;
                // A part kept apart to lie behind a frame is no frame of its own.
                if (behind is not null && name.StartsWith(behind, StringComparison.Ordinal))
                    continue;
                (int X, int Y, InstalledSheetFrameDocument Frame)? over = BodyOf(name);
                InstalledSheetFrameDocument? under = behind is not null && document.Frames.TryGetValue(behind + name, out InstalledSheetFrameDocument? kept)
                    ? kept
                    : null;
                if (frame.Picture is null && under is not { Picture: not null } && over is not { Frame.Picture: not null })
                {
                    paths[name] = "";
                    continue;
                }

                Array.Clear(canvas);
                // Lays one of the sheet's frames on the canvas: only where it shows something does it cover what is there.
                void Lay(InstalledSheetFrameDocument laid)
                {
                    if (laid.Picture is null)
                        return;
                    int across = -laid.Origin.X - left, down = -laid.Origin.Y - top;
                    for (int y = 0; y < laid.Size.Y; y++)
                    {
                        int from = (laid.Corner.Y + y) * document.Size.X + laid.Corner.X, to = (down + y) * width + across;
                        for (int x = 0; x < laid.Size.X; x++)
                        {
                            RgbaPixel pixel = image.Pixels[from + x];
                            if (pixel.A == 0)
                                continue;
                            // A shade shows some of the sprites' colours as others, some of them only in one region of them.
                            int region = image.RecolourRegions is { } regions ? regions[from + x] : 0;
                            canvas[to + x] = shaded is not null && shaded.TryGetValue(((pixel.R << 16) | (pixel.G << 8) | pixel.B, region), out RgbaPixel recoloured)
                                ? recoloured with { A = pixel.A }
                                : pixel;
                        }
                    }
                }

                // Lays the body: only where it shows something does it cover what is there.
                void LayBody()
                {
                    if (over is not { Frame.Picture: not null } body)
                        return;
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

                if (under is not null)
                    Lay(under);

                // Sprites the old game draws in front of every tile lie in front of the body too; any others lie under it.
                if (frame.Depth == ActorDepth.InFront)
                {
                    LayBody();
                    Lay(frame);
                }
                else
                {
                    Lay(frame);
                    LayBody();
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
}

/// <summary>One frame of a big boss's body as it was cut.</summary>
/// <param name="Picture">Its picture.</param>
/// <param name="OffsetX">Where the picture's left edge lies from the boss's place.</param>
/// <param name="OffsetY">Where the picture's top edge lies from the boss's place.</param>
internal sealed partial record EngineBodyFrame(string Picture, int OffsetX, int OffsetY);
