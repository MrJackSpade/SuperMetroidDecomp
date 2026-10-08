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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate projectile frame-binding property {name}."), descendArrays: false);

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

/// <summary>Editable mapping from bank-$93 timed projectile records to presentation-only sprite identities, leaving native instruction mechanics unchanged.</summary>
public sealed record ProjectileFrameBindingDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="ProjectileFrameBindingFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 805 required frame_XXXX record keys from <see cref="ProjectileFrameBindingFormat.FrameName"/>, each assigned a sprite_XXXX identity from <see cref="ProjectileSpriteDefinitions.NativePointers"/>; multiple records may share one sprite.</summary>
    public required Dictionary<string, string> Frames { get; init; }
}

/// <summary>Stable visual-file identity for all authored timed projectile instructions.</summary>
public static class ProjectileFrameBindingFormat
{
    /// <summary>JSON resource filename for timed projectile record-to-spritemap assignments, separate from the composition artwork file.</summary>
    public const string FileName = "projectile-frame-bindings.json";
    /// <summary>Supported schema revision, one, requiring an explicit visual assignment for every authored timed record.</summary>
    public const int Version = 1;
    /// <summary>805 distinct timed bank-$93 record starts requiring bindings; not the number of sprite compositions or updates in one animation.</summary>
    public const int FrameCount = 805;
    /// <summary>Formats a bank-relative timed-record identity as the canonical JSON key, such as frame_86DB for the upward Power beam record.</summary>
    /// <param name="pointer">Sixteen-bit record-start offset; formatting does not validate that it names a required timed record.</param>
    /// <returns>The frame_ prefix followed by exactly four uppercase hexadecimal digits.</returns>
    public static string FrameName(ushort pointer) => $"frame_{pointer:X4}";
}
