using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Visual spritemap selection for the 805 timed bank-$93 projectile records. Durations,
/// trail values, radii, damage, and instruction control flow are compiled separately.
/// </summary>
public sealed class ProjectileFrameBindingCatalog
{
    private readonly Dictionary<ushort, ushort> sprites;
    private ProjectileFrameBindingCatalog(Dictionary<ushort, ushort> sprites) => this.sprites = sprites;

    public ushort Resolve(ushort instructionPointer) =>
        sprites.TryGetValue(instructionPointer, out ushort sprite)
            ? sprite
            : throw new InvalidDataException(
                $"Timed projectile frame $93:{instructionPointer:X4} has no installed visual binding.");

    public static ProjectileFrameBindingCatalog Load(Stream json)
    {
        ProjectileFrameBindingDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ProjectileFrameBindingDocument>(Options)
                ?? throw new InvalidDataException("Projectile frame bindings are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid projectile frame-binding JSON.", error);
        }
        if (document.Version != ProjectileFrameBindingFormat.Version || document.Frames is null ||
            document.Frames.Count != ProjectileFrameBindingFormat.FrameCount)
            throw new InvalidDataException("Projectile frame bindings require version 1 and all 805 timed records.");
        var legalSprites = ProjectileSpriteDefinitions.NativePointers.ToArray().ToHashSet();
        var compiled = new Dictionary<ushort, ushort>();
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
        {
            string key = ProjectileFrameBindingFormat.FrameName(pointer);
            if (!document.Frames.TryGetValue(key, out string? spriteName) ||
                spriteName is null || !spriteName.StartsWith("sprite_", StringComparison.Ordinal) ||
                spriteName.Length != 11 ||
                !ushort.TryParse(spriteName.AsSpan(7), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out ushort sprite) ||
                !legalSprites.Contains(sprite))
                throw new InvalidDataException($"Projectile frame {key} has no legal sprite identity.");
            compiled.Add(pointer, sprite);
        }
        return new(compiled);
    }

    public static byte[] Write(ProjectileFrameBindingDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object) return;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in value.EnumerateObject())
        {
            if (!names.Add(property.Name))
                throw new InvalidDataException($"Duplicate projectile frame-binding property {property.Name}.");
            RejectDuplicates(property.Value);
        }
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

public sealed record ProjectileFrameBindingDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, string> Frames { get; init; }
}

/// <summary>Stable visual-file identity for all authored timed projectile instructions.</summary>
public static class ProjectileFrameBindingFormat
{
    public const string FileName = "projectile-frame-bindings.json";
    public const int Version = 1;
    public const int FrameCount = 805;
    public static string FrameName(ushort pointer) => $"frame_{pointer:X4}";
}
