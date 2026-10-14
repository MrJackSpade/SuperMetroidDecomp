using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Visual spritemap selection for the 805 timed bank-$93 projectile records. Durations,
/// trail values, radii, damage, and instruction control flow are compiled separately.
/// </summary>
public sealed class ProjectileFrameBindingCatalog
{
    /// <summary>Only frame bindings that differ from the compiled sprite selection.</summary>
    internal readonly Dictionary<ushort, ushort> sprites;
    /// <summary>Retains the validated override lookup detached from editable JSON.</summary>
    private ProjectileFrameBindingCatalog(Dictionary<ushort, ushort> sprites) => this.sprites = sprites;

    /// <summary>Returns the selected spritemap for one native timed projectile record, using an installed override or its compiled visual binding without reading cartridge memory.</summary>
    /// <param name="instructionPointer">Bank-$93 record-start offset naming the duration word, not the spritemap operand at offset plus two or a control-flow opcode.</param>
    /// <returns>Bank-relative spritemap identity consumed by the projectile sprite catalog.</returns>
    /// <exception cref="InvalidDataException">The pointer has neither an installed override nor a calculated timed-record visual binding.</exception>
    public ushort Resolve(ushort instructionPointer) =>
        sprites.TryGetValue(instructionPointer, out ushort sprite)
            ? sprite
            : ProjectileSpriteDefinitions.TryCalculatedFrameSprite(instructionPointer, out sprite) ? sprite
            : throw new InvalidDataException(
                $"Timed projectile frame $93:{instructionPointer:X4} has no installed visual binding.");

    /// <summary>Validates every timed-record visual assignment and compiles only bindings that differ from the reviewed native composition selection.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled visual bindings detached from the document dictionary, with no duration, radius, trail, or damage overrides.</returns>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, an incomplete 805-record set, or a sprite outside the required native composition identities.</exception>
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
        var legalSprites = ProjectileSpriteDefinitions.NativePointers.ToHashSet();
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
            if (!ProjectileSpriteDefinitions.TryCalculatedFrameSprite(pointer, out ushort calculated) || sprite != calculated)
                compiled.Add(pointer, sprite);
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable record-to-sprite bindings to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">Complete timed-record assignments to serialize; the dictionary is not retained.</param>
    /// <returns>Validated JSON bytes for <see cref="ProjectileFrameBindingFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, required-record, or legal-sprite validation.</exception>
    public static byte[] Write(ProjectileFrameBindingDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Rejects repeated frame-binding property names before JSON deserialization can collapse them.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate projectile frame-binding property {name}."), descendArrays: false);

    /// <summary>Strict camel-case schema options used consistently by load and write.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
