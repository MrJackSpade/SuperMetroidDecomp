using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What the rooms' own pictures need of the sheets: a frame's pixels as shades show them, and spawned pictures in several shades.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>
    /// The pixels of one frame of a sheet, row by row, with its width and height, as the shades named show it: each
    /// laid over the one before. With none named, as the sheet has it.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame or shade, or the frame shows nothing.</exception>
    internal (RgbaPixel[] Pixels, int Width, int Height) FrameShown(string family, string sheet, string frame, params string[] shades)
    {
        InstalledSheetDocument document = Document(family, sheet);
        if (!document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed) || placed.Picture is null)
            throw new InvalidDataException($"The sheet {sheet} has no picture for the frame {frame}.");
        if (!_images.TryGetValue((family, sheet), out SpriteImagePixels? image))
            _images[(family, sheet)] = image = _store.Assemble(sheet, document);
        Dictionary<(int Colour, int Region), RgbaPixel>? shown = shades.Length == 0 ? null : ShadesOf(sheet, document, shades);
        int width = placed.Size.X, height = placed.Size.Y;
        var pixels = new RgbaPixel[width * height];
        for (int y = 0; y < height; y++)
        {
            int from = (placed.Corner.Y + y) * document.Size.X + placed.Corner.X;
            image.Pixels.AsSpan(from, width).CopyTo(pixels.AsSpan(y * width, width));
            if (shown is null)
                continue;
            for (int x = 0; x < width; x++)
            {
                RgbaPixel pixel = pixels[y * width + x];
                int region = image.RecolourRegions is { } regions ? regions[from + x] : 0;
                if (pixel.A != 0 && shown.TryGetValue(((pixel.R << 16) | (pixel.G << 8) | pixel.B, region), out RgbaPixel recoloured))
                    pixels[y * width + x] = recoloured with { A = pixel.A };
            }
        }

        return (pixels, width, height);
    }

    /// <summary>Whether a sheet has a frame of the name given.</summary>
    internal bool HasFrame(string family, string sheet, string frame) => Document(family, sheet).Frames.ContainsKey(frame);

    /// <summary>
    /// Cuts named pictures of what an enemy spawns, as <see cref="CutSpawned"/> does, as the shades named show them, each
    /// laid over the one before.
    /// </summary>
    internal EngineSheetPictures CutSpawnedShades(EngineAssetTree tree, string folder, string spawner, IEnumerable<string> frames, params string[] shades)
    {
        var placed = new List<(string Name, string Sheet)>();
        foreach (string frame in frames.Distinct(StringComparer.Ordinal))
            placed.Add((frame, SheetHolding(spawner, frame)));
        return CutFrames(tree, EnemyFamily, folder, placed, shades);
    }
}
