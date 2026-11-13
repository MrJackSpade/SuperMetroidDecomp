using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed timed-projectile frame-binding document (#1502).</summary>
public static class ProjectileFrameBindingFile
{
    /// <summary>Loads the frame bindings installed in projectile directory <paramref name="directory"/>, as the player may have edited them.</summary>
    /// <exception cref="InvalidDataException">The document is invalid.</exception>
    public static InstalledProjectileFrameBindings LoadInstalled(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        using FileStream json = File.OpenRead(Path.Combine(directory, ProjectileFrameBindingFormat.FileName));
        return Load(json);
    }

    /// <summary>Validates every timed record's visual assignment and binds each record to the composition its document names.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>The composition of every timed record, detached from the document dictionary; durations, radii, trails and damage are gameplay and not installed here.</returns>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, an incomplete record set, or a composition outside the required set.</exception>
    public static InstalledProjectileFrameBindings Load(Stream json)
    {
        ProjectileFrameBindingDocument document;
        document = JsonAssetDocument.Read<ProjectileFrameBindingDocument>(json, Options, "projectile frame-binding");
        if (document.Version != ProjectileFrameBindingFormat.Version || document.Frames is null ||
            document.Frames.Count != ProjectileFrameBindingFormat.FrameCount)
            throw new InvalidDataException($"Projectile frame bindings require version {ProjectileFrameBindingFormat.Version} and all {ProjectileFrameBindingFormat.FrameCount} timed records.");
        Dictionary<string, ProjectileSpriteId> named = ProjectileSpriteDefinitions.All.ToDictionary(sprite => sprite.Name, StringComparer.Ordinal);
        var compiled = new Dictionary<SamusProjectileFrame, ProjectileSpriteId>();
        foreach (SamusProjectileFrame record in SamusProjectileFrameOrder.Frames)
        {
            string key = ProjectileFrameBindingFormat.FrameName(record);
            if (!document.Frames.TryGetValue(key, out string? spriteName) || spriteName is null ||
                !named.TryGetValue(spriteName, out ProjectileSpriteId sprite))
                throw new InvalidDataException($"Projectile frame {key} has no legal sprite identity.");
            compiled.Add(record, sprite);
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable record-to-sprite bindings to indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">Complete timed-record assignments to serialize; the dictionary is not retained.</param>
    /// <returns>JSON bytes for <see cref="ProjectileFrameBindingFormat.FileName"/>.</returns>
    public static byte[] Write(ProjectileFrameBindingDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, Options);

    /// <summary>Strict camel-case schema options used consistently by load and write.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
