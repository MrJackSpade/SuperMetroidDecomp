using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable initial screen positions for the six Ceres-to-Zebes reveal actors.
/// Their native instruction lists, slide acceleration, deletion, and phase handoffs
/// remain in compiled cartridge mechanics.
/// </summary>
public sealed class CeresRevealActorLayout
{
    private readonly CeresRevealActorPlacement[]? placements;

    private CeresRevealActorLayout(CeresRevealActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    /// <summary>Returns the selected initial placement for one actor in the native $8B:C810-C831 Zebes-reveal spawn order.</summary>
    /// <param name="actorIndex">Index 0..5: planet, upper-left stars, upper-right stars, lower-left stars, lower-right stars, then Planet Zebes title.</param>
    /// <returns>Named screen-pixel placement used when creating the actor, without changing its instruction list or motion policy.</returns>
    /// <exception cref="IndexOutOfRangeException">The actor index is outside 0..5.</exception>
    public CeresRevealActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresDestructionActorDefinitions.ZebesActorCount)
                throw new IndexOutOfRangeException();
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves native named-actor initializer placement without a second stock lookup.</summary>
    private static CeresRevealActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresDestructionActorDefinitions.ZebesActor(index);
        return new() { Id = CeresDestructionActorDefinitions.ZebesPlacementSource(index).Id, X = actor.X, Y = actor.Y };
    }
    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(CeresRevealActorLayout), content =>
    {
        content.Append("actors", CeresDestructionActorDefinitions.ZebesActorCount);
        for (int index = 0; index < CeresDestructionActorDefinitions.ZebesActorCount; index++)
        {
            CeresRevealActorPlacement placement = this[index];
            content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
            content.Append("x", placement.X);
            content.Append("y", placement.Y);
        }
    });

    /// <summary>Validates the six fixed reveal-actor identities and compiles their editable initial positions from ceres-zebes-reveal-actors.json.</summary>
    /// <param name="json">UTF-8 JSON source read from its current position and left open.</param>
    /// <returns>Selected placements for the reveal, with stock positions resolved from the compiled native initializers.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, six-entry order, actor identity, or unsigned 16-bit coordinate is invalid.</exception>
    public static CeresRevealActorLayout Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresRevealActorLayoutDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresRevealActorLayoutDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ceres reveal actor layout is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres reveal actor layout JSON.", error);
        }

        if (document.Version != CeresRevealActorLayoutFormat.Version ||
            document.Actors is not { Length: CeresDestructionActorDefinitions.ZebesActorCount })
            throw new InvalidDataException("Ceres reveal requires exactly six ordered actor placements.");
        for (int index = 0; index < document.Actors.Length; index++)
        {
            CeresRevealActorPlacement placement = document.Actors[index];
            if (placement is null ||
                placement.Id != CeresDestructionActorDefinitions.ZebesPlacementSource(index).Id ||
                placement.X is < 0 or > ushort.MaxValue ||
                placement.Y is < 0 or > ushort.MaxValue)
                throw new InvalidDataException(
                    $"Ceres reveal actor {index} must retain its identity and 16-bit coordinates.");
        }
        return new CeresRevealActorLayout(document.Actors);
    }

    /// <summary>Serializes the editable layout to UTF-8 JSON, validates it through <see cref="Load"/>, then writes the validated bytes.</summary>
    /// <param name="json">Destination written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Ordered actor placements to serialize; the document is not retained.</param>
    /// <exception cref="ArgumentNullException">The destination stream is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails schema, identity, order, or coordinate validation.</exception>
    public static void Write(Stream json, CeresRevealActorLayoutDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>One named cinematic actor's initial whole-pixel screen position; animation, palette, sliding, and deletion remain compiled mechanics.</summary>
public sealed record CeresRevealActorPlacement
{
    /// <summary>Case-sensitive native presentation role, required to match its fixed index in <see cref="CeresRevealActorLayoutDocument.Actors"/>.</summary>
    public required string Id { get; init; }
    /// <summary>Initial horizontal screen-pixel coordinate encoded as a native unsigned 16-bit position word, 0..65535; not a room-world or tile coordinate.</summary>
    public required int X { get; init; }
    /// <summary>Initial vertical screen-pixel coordinate encoded as a native unsigned 16-bit position word, 0..65535; increases downward.</summary>
    public required int Y { get; init; }
}

/// <summary>Editable initial-position schema for the planet, four star sheets, and title shown after Ceres destruction.</summary>
public sealed record CeresRevealActorLayoutDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="CeresRevealActorLayoutFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly six nonnull entries in order: planet, stars-upper-left, stars-upper-right, stars-lower-left, stars-lower-right, planet-zebes-title; IDs cannot be reassigned or reordered.</summary>
    public required CeresRevealActorPlacement[] Actors { get; init; }
}

/// <summary>Installed-resource identity and schema revision for the Ceres-to-Zebes reveal's editable actor positions.</summary>
public static class CeresRevealActorLayoutFormat
{
    /// <summary>Supported schema revision, one, requiring the six native actor roles in spawn order.</summary>
    public const int Version = 1;
    /// <summary>JSON resource filename for the six reveal actors' initial screen-coordinate words.</summary>
    public const string FileName = "ceres-zebes-reveal-actors.json";
}
