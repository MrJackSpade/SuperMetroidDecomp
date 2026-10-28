using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Spore Spawn colour document.</summary>
public static class SporeSpawnColorFile
{
    /// <summary>Loads version-1 <c>spore-spawn-colors.json</c>, validating exact palette-row dimensions and RGB5 channels from 0 through 31 while keeping health thresholds and death timing in boss code.</summary>
    /// <param name="json">Caller-owned stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled colors retaining independent edits and using calculated shared colors or fade samples only where supplied values agree.</returns>
    public static SporeSpawnColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        SporeSpawnColorDocument document;
        document = JsonAssetDocument.Read<SporeSpawnColorDocument>(json, ArtworkJson.Options, "Spore Spawn color");
        if (document.Version != SporeSpawnColorFormat.Version)
            throw new InvalidDataException("Spore Spawn colors require the supported version.");
        return new(
            Compile(document.Spores, "spores"),
            CompileFrames(document.Health, SporeSpawnColorDefinitions.HealthFrameCount, "health"),
            CompileFrames(document.DeathSprite, SporeSpawnColorDefinitions.DeathSpriteFrameCount,
                "death sprite"),
            CompileFrames(document.DeathLevel, SporeSpawnColorDefinitions.DeathSceneFrameCount,
                "death level"),
            CompileFrames(document.DeathBackground, SporeSpawnColorDefinitions.DeathSceneFrameCount,
                "death background"));
    }

    /// <summary>Serializes the editable palette document as indented camel-case UTF-8 JSON, validating its version, dimensions, and RGB5 channel bounds before returning the bytes.</summary>
    public static byte[] Write(SporeSpawnColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Requires the native number of ordered palette phases and compiles each sixteen-ink RGB5 row.</summary>
    private static RgbColor[][] CompileFrames(PaletteRgb5[][]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Spore Spawn {name} requires {count} frames.");
        return source.Select((frame, index) => Compile(frame, $"{name} frame {index}"))
            .ToArray();
    }

    /// <summary>Validates one complete sixteen-ink row and packs each bounded RGB5 color.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new InvalidDataException(
                $"Spore Spawn {name} requires {SporeSpawnColorDefinitions.ColorsPerFrame} RGB5 colors.");
        var compiled = new RgbColor[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Spore Spawn {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToRgbColor();
        }
        return compiled;
    }
}
