using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Visual composition selection for the 805 timed projectile records. Durations,
/// trail values, radii, damage, and instruction control flow are compiled separately.
/// </summary>
public sealed class ProjectileFrameBindingCatalog
{
    /// <summary>Only frame bindings that differ from the records' stock compositions, by record.</summary>
    internal readonly Dictionary<int, ProjectileSpriteId> sprites;

    /// <summary>Retains the validated override lookup detached from editable JSON.</summary>
    private ProjectileFrameBindingCatalog(Dictionary<int, ProjectileSpriteId> sprites) => this.sprites = sprites;

    /// <summary>Returns the composition one timed projectile record draws: an installed override or the record's stock composition.</summary>
    /// <param name="frame">The timed record.</param>
    internal ProjectileSpriteId Resolve(SamusProjectileFrame frame) =>
        sprites.TryGetValue(frame.Record, out ProjectileSpriteId sprite) ? sprite : SamusProjectileRecordDefinitions.Get(frame).Sprite;

    /// <summary>Validates every timed-record visual assignment and compiles only bindings that differ from the reviewed native composition selection.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled visual bindings detached from the document dictionary, with no duration, radius, trail, or damage overrides.</returns>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, an incomplete 805-record set, or a composition outside the required set.</exception>
    public static ProjectileFrameBindingCatalog Load(Stream json)
    {
        ProjectileFrameBindingDocument document;
        document = JsonAssetDocument.Read<ProjectileFrameBindingDocument>(json, Options, "projectile frame-binding");
        if (document.Version != ProjectileFrameBindingFormat.Version || document.Frames is null ||
            document.Frames.Count != ProjectileFrameBindingFormat.FrameCount)
            throw new InvalidDataException($"Projectile frame bindings require version {ProjectileFrameBindingFormat.Version} and all 805 timed records.");
        Dictionary<string, ProjectileSpriteId> named = ProjectileSpriteDefinitions.All.ToDictionary(sprite => sprite.Name, StringComparer.Ordinal);
        var compiled = new Dictionary<int, ProjectileSpriteId>();
        for (int record = 0; record < SamusProjectileRecordDefinitions.Records.Length; record++)
        {
            string key = ProjectileFrameBindingFormat.FrameName(record);
            if (!document.Frames.TryGetValue(key, out string? spriteName) || spriteName is null ||
                !named.TryGetValue(spriteName, out ProjectileSpriteId sprite))
                throw new InvalidDataException($"Projectile frame {key} has no legal sprite identity.");
            if (sprite != SamusProjectileRecordDefinitions.Records[record].Sprite)
                compiled.Add(record, sprite);
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable record-to-sprite bindings to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">Complete timed-record assignments to serialize; the dictionary is not retained.</param>
    /// <returns>Validated JSON bytes for <see cref="ProjectileFrameBindingFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, required-record, or legal-sprite validation.</exception>
    public static byte[] Write(ProjectileFrameBindingDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Load(json));

    /// <summary>Strict camel-case schema options used consistently by load and write.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
